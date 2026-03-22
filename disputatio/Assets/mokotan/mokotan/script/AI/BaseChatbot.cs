using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Fungus;

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
    [Header("Server Settings")]
    [SerializeField] protected string localServerUrl = "http://15.165.237.11:8000/chat";
    protected bool isRequestInProgress = false;

    [Header("Base UI Settings")]
    [SerializeField] protected SayDialog chatSayDialog;

    protected List<OpenAIMessage> chatHistory = new List<OpenAIMessage>();

    [Serializable]
    public class LocalLlamaPayload {
        public string prompt;
        public string system;
        public bool use_tools = true;
    }

    protected virtual void Start() {
        InitializeChatHistory();
    }

    protected virtual void InitializeChatHistory() {
        chatHistory.Clear();
        chatHistory.Add(new OpenAIMessage { role = "system", content = "당신은 저택의 도우미입니다." });
    }

    protected void Say(string message, Action onComplete = null)
    {
        if (chatSayDialog != null)
        {
            if (!chatSayDialog.gameObject.activeInHierarchy) chatSayDialog.gameObject.SetActive(true);
            chatSayDialog.Say(message, true, true, false, true, true, null, onComplete);
        }
        else
        {
            Debug.LogWarning("Inspector에서 Chat Say Dialog를 연결해주세요!");
            onComplete?.Invoke();
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
        string finalSystemPrompt = BuildFinalSystemPrompt();

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
        string finalSystemPrompt = BuildFinalSystemPrompt();

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
