using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Fungus;
using TMPro;
using UnityEngine.SceneManagement;

public class BypassCertificate : CertificateHandler {
    protected override bool ValidateCertificate(byte[] certificateData) => true;
}

[Serializable]
public class FunctionCallData
{
    public string name;
    public Dictionary<string, object> arguments;
}

[Serializable]
public class ChatResponseData
{
    public string response;
    public List<FunctionCallData> function_calls;
}

[Serializable]
public class SSEEventData
{
    public string type;
    public string content;
    public string name;
    public Dictionary<string, object> arguments;
    public string full_text;
}

public abstract class BaseChatbot : MonoBehaviour
{
    private const string ChesterVoiceCommonResource = "ChesterVoiceCommon";

    [Header("Server Settings")]
    [SerializeField] protected string localServerUrl = "http://15.165.237.11:8000/chat";
    protected bool isRequestInProgress = false;

    /// <summary>
    /// false면 Resources/ChesterVoiceCommon을 시스템 프롬프트에 붙이지 않음(예: ParrotChatbot 초단문 규칙과 충돌 방지).
    /// </summary>
    protected virtual bool AppendCommonChesterVoiceBlock => true;

    [Header("Base UI Settings")]
    [SerializeField] protected SayDialog chatSayDialog;
    [SerializeField] private KeyCode dialogAdvanceKey = KeyCode.Space;

    [Header("Chat Input")]
    [SerializeField] private TMP_InputField userInputField;

    protected List<OpenAIMessage> chatHistory = new List<OpenAIMessage>();
    private DialogInput chatDialogInput;

    [Serializable]
    public class LocalLlamaPayload {
        public string prompt;
        public string system;
        public bool use_tools = true;
    }

    protected virtual void Start()
    {
        InitializeChatHistory();
        _ = SceneRevisitTracker.Instance;
        CacheDialogInput();
        if (userInputField != null)
            userInputField.onSubmit.AddListener(OnInputFieldSubmit);
    }

    protected virtual void Update()
    {
        if (chatSayDialog == null || !chatSayDialog.gameObject.activeInHierarchy)
            return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(dialogAdvanceKey))
            TryAdvanceChatDialog();
    }

    /// <summary>
    /// TMP onSubmit may pass an empty string while IME is active or for some line types;
    /// always read the live field text in OnSendButtonClick.
    /// </summary>
    private void OnInputFieldSubmit(string _)
    {
        OnSendButtonClick();
    }

    public void OnSendButtonClick()
    {
        if (userInputField == null)
        {
            Debug.LogWarning($"{GetType().Name}: userInputField가 연결되지 않았습니다.");
            return;
        }

        string message = userInputField.text;

        if (string.IsNullOrEmpty(message))
        {
            Debug.LogWarning("입력값이 비어있습니다.");
            return;
        }

        if (isRequestInProgress)
        {
            Debug.LogWarning("현재 이미 요청 중입니다.");
            return;
        }

        StartCoroutine(GetGPTResponse(message));
        userInputField.text = "";
    }

    protected virtual void OnDestroy()
    {
        if (userInputField != null)
            userInputField.onSubmit.RemoveListener(OnInputFieldSubmit);
    }

    private void CacheDialogInput()
    {
        if (chatSayDialog == null)
            return;

        chatDialogInput = chatSayDialog.GetComponentInChildren<DialogInput>(true);
    }

    private void TryAdvanceChatDialog()
    {
        if (chatDialogInput == null)
            CacheDialogInput();

        if (chatDialogInput != null)
        {
            chatDialogInput.SetNextLineFlag();
            return;
        }

        Debug.LogWarning($"{GetType().Name}: SayDialog의 DialogInput을 찾지 못했습니다.");
    }

    protected virtual void InitializeChatHistory() {
        chatHistory.Clear();
        chatHistory.Add(new OpenAIMessage {
            role = "system",
            content = "당신은 저택의 앵무새 체셔입니다. 방별 지침과 공통 말투 규칙(시스템 끝단)을 모두 따릅니다."
        });
    }

    /// <summary>
    /// 모든 /chat·/chat/stream 요청에 공통으로 붙는 말투 블록(Resources/ChesterVoiceCommon.txt).
    /// </summary>
    protected string ComposeSystemPromptWithCommonRules(string roomSpecificPrompt)
    {
        if (!AppendCommonChesterVoiceBlock || string.IsNullOrEmpty(roomSpecificPrompt))
            return roomSpecificPrompt;

        TextAsset common = Resources.Load<TextAsset>(ChesterVoiceCommonResource);
        if (common == null)
        {
            Debug.LogWarning($"[BaseChatbot] Resources/{ChesterVoiceCommonResource}.txt 없음 — 공통 말투 생략");
            return roomSpecificPrompt;
        }

        return roomSpecificPrompt + "\n\n" + common.text;
    }

    protected virtual HeuristicSignalInput BuildHeuristicSignalInput(string userMessage)
    {
        return new HeuristicSignalInput
        {
            roomName = GetType().Name,
            progressScore = 0.5f,
            accuracyScore = 0.5f
        };
    }

    protected string ComposeSystemPromptWithHeuristics(string basePrompt, string userMessage)
    {
        HeuristicSignalInput signal = BuildHeuristicSignalInput(userMessage);
        signal = SceneRevisitTracker.Instance.FillRevisitSignals(signal, SceneManager.GetActiveScene().name);
        return PromptInfoBudgetComposer.Compose(basePrompt, signal);
    }

    protected void Say(string message, Action onComplete = null)
    {
        bool restoreInput = userInputField != null && userInputField.gameObject.activeSelf;
        if (userInputField != null)
            userInputField.gameObject.SetActive(false);

        void Done()
        {
            if (chatSayDialog != null && chatSayDialog.gameObject.activeInHierarchy)
                chatSayDialog.SetActive(false);
            if (userInputField != null && restoreInput)
                userInputField.gameObject.SetActive(true);
            onComplete?.Invoke();
        }

        if (chatSayDialog != null)
        {
            if (!chatSayDialog.gameObject.activeInHierarchy) chatSayDialog.gameObject.SetActive(true);
            chatSayDialog.Say(message, true, true, false, true, true, null, Done);
        }
        else
        {
            Debug.LogWarning("Inspector에서 Chat Say Dialog를 연결해주세요!");
            Done();
        }
    }

    // ---------------------------------------------------------------
    // Non-streaming /chat (backward-compatible, now with function_calls)
    // ---------------------------------------------------------------
    protected IEnumerator GetGPTResponse(string userMessage)
    {
        if (isRequestInProgress) yield break;
        isRequestInProgress = true;

        chatHistory.Add(new OpenAIMessage { role = "user", content = userMessage });
        string finalSystemPrompt = ComposeSystemPromptWithCommonRules(BuildFinalSystemPrompt());
        finalSystemPrompt = ComposeSystemPromptWithHeuristics(finalSystemPrompt, userMessage);

        LocalLlamaPayload payload = new LocalLlamaPayload {
            prompt = userMessage,
            system = finalSystemPrompt,
            use_tools = true
        };
        string payloadJson = JsonConvert.SerializeObject(payload);

        using (UnityWebRequest request = new UnityWebRequest(localServerUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(payloadJson);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.certificateHandler = new BypassCertificate();
            request.timeout = 60;

            yield return request.SendWebRequest();

            string chatbotResponse;
            List<FunctionCallData> functionCalls = new List<FunctionCallData>();

            if (request.result != UnityWebRequest.Result.Success)
            {
                chatbotResponse = "연결 오류: " + request.error;
            }
            else
            {
                string rawJson = request.downloadHandler.text;
                try
                {
                    var parsed = JsonConvert.DeserializeObject<ChatResponseData>(rawJson);
                    chatbotResponse = parsed.response ?? "";
                    if (parsed.function_calls != null)
                        functionCalls = parsed.function_calls;
                }
                catch (Exception e)
                {
                    Debug.LogError("Response parse error: " + e.Message);
                    chatbotResponse = rawJson;
                }
                chatHistory.Add(new OpenAIMessage { role = "assistant", content = chatbotResponse });
            }

            isRequestInProgress = false;
            yield return StartCoroutine(HandleChatbotResponse(chatbotResponse, functionCalls));
        }
    }

    // ---------------------------------------------------------------
    // SSE streaming /chat/stream
    // ---------------------------------------------------------------
    protected IEnumerator GetGPTResponseStreaming(string userMessage)
    {
        if (isRequestInProgress) yield break;
        isRequestInProgress = true;

        chatHistory.Add(new OpenAIMessage { role = "user", content = userMessage });
        string finalSystemPrompt = ComposeSystemPromptWithCommonRules(BuildFinalSystemPrompt());
        finalSystemPrompt = ComposeSystemPromptWithHeuristics(finalSystemPrompt, userMessage);

        LocalLlamaPayload payload = new LocalLlamaPayload {
            prompt = userMessage,
            system = finalSystemPrompt,
            use_tools = true
        };
        string payloadJson = JsonConvert.SerializeObject(payload);

        string streamUrl = localServerUrl.Replace("/chat", "/chat/stream");

        using (UnityWebRequest request = new UnityWebRequest(streamUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(payloadJson);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "text/event-stream");
            request.certificateHandler = new BypassCertificate();
            request.timeout = 120;

            var op = request.SendWebRequest();

            StringBuilder fullText = new StringBuilder();
            List<FunctionCallData> functionCalls = new List<FunctionCallData>();
            int lastProcessedIndex = 0;
            bool done = false;

            while (!done)
            {
                yield return null;

                if (request.downloadHandler != null)
                {
                    string allData = request.downloadHandler.text;
                    if (allData.Length > lastProcessedIndex)
                    {
                        string newData = allData.Substring(lastProcessedIndex);
                        lastProcessedIndex = allData.Length;

                        string[] lines = newData.Split('\n');
                        foreach (string line in lines)
                        {
                            if (!line.StartsWith("data: ")) continue;
                            string json = line.Substring(6).Trim();
                            if (string.IsNullOrEmpty(json)) continue;

                            SSEEventData evt = null;
                            try { evt = JsonConvert.DeserializeObject<SSEEventData>(json); }
                            catch { continue; }

                            if (evt == null) continue;

                            switch (evt.type)
                            {
                                case "text_delta":
                                    if (evt.content != null) fullText.Append(evt.content);
                                    OnStreamTextDelta(evt.content);
                                    break;

                                case "function_call":
                                    functionCalls.Add(new FunctionCallData {
                                        name = evt.name,
                                        arguments = evt.arguments
                                    });
                                    break;

                                case "done":
                                    if (!string.IsNullOrEmpty(evt.full_text))
                                        fullText = new StringBuilder(evt.full_text);
                                    done = true;
                                    break;

                                case "error":
                                    Debug.LogError("SSE error: " + evt.content);
                                    done = true;
                                    break;
                            }
                        }
                    }
                }

                if (op.isDone && !done) done = true;
            }

            string responseText = fullText.ToString();
            chatHistory.Add(new OpenAIMessage { role = "assistant", content = responseText });
            isRequestInProgress = false;

            yield return StartCoroutine(HandleChatbotResponse(responseText, functionCalls));
        }
    }

    protected virtual void OnStreamTextDelta(string delta)
    {
        // Subclasses can override to display streaming text incrementally
    }

    protected abstract string BuildFinalSystemPrompt();
    protected abstract IEnumerator HandleChatbotResponse(string responseMessage, List<FunctionCallData> functionCalls);
}
