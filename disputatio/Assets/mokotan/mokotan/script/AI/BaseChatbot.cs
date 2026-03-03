using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using System;
using Fungus; // Fungus 네임스페이스 추가 확인

public class BypassCertificate : CertificateHandler {
    protected override bool ValidateCertificate(byte[] certificateData) => true;
}

public abstract class BaseChatbot : MonoBehaviour
{
    [Header("Server Settings")]
    [SerializeField] protected string localServerUrl = "http://15.165.237.11:8000/chat";
    protected bool isRequestInProgress = false;

    [Header("Base UI Settings")]
    // ⚠️ 중요: 변수 이름을 'Say'가 아닌 'chatSayDialog'로 수정했습니다.
    [SerializeField] protected SayDialog chatSayDialog; 

    protected List<OpenAIMessage> chatHistory = new List<OpenAIMessage>();

    [Serializable]
    public class LocalLlamaPayload {
        public string prompt; 
        public string system; 
    }

    [Serializable]
    public class LocalLlamaResponse {
        public string response;
    }

    [Serializable]
    public class OpenAIMessage {
        public string role;
        public string content;
    }

    protected virtual void Start() {
        InitializeChatHistory();
    }

    public class ParrotChatbot : BaseChatbot
{
    // 수수께끼 앵무새의 엄격한 페르소나 설정
    protected override string BuildFinalSystemPrompt()
    {
        return @"[수수께끼 앵무새 규칙]
1. 모든 답변은 공백 포함 한글 20자 이내로 할 것.
2. 수수께끼를 내거나 사용자의 오답을 비웃을 것.
3. 죽어도 정답은 알려주지 말 것.
4. 말끝에 '깍!', '삐약!', '푸드덕!' 중 하나를 무조건 붙일 것.";
    }

    // AI의 답변이 도착했을 때 처리 (화면에 표시)
    protected override IEnumerator HandleChatbotResponse(string responseMessage)
    {
        // 상속받은 Say 함수를 사용하여 대화창에 표시
        Say(responseMessage);
        yield break;
    }
}

    protected virtual void InitializeChatHistory() {
        chatHistory.Clear();
        chatHistory.Add(new OpenAIMessage { role = "system", content = "당신은 저택의 도우미입니다." });
    }

    // ✅ 충돌 해결된 Say 함수
    protected void Say(string message, System.Action onComplete = null)
    {
        if (chatSayDialog != null)
        {
            if (!chatSayDialog.gameObject.activeInHierarchy) chatSayDialog.gameObject.SetActive(true);
            chatSayDialog.Say(message, true, true, false, true, true, null, onComplete);
        }
        else
        {
            Debug.LogWarning("⚠️ Inspector에서 Chat Say Dialog를 연결해주세요!");
            onComplete?.Invoke();
        }
    }

    protected IEnumerator GetGPTResponse(string userMessage)
    {
        if (isRequestInProgress) yield break;
        isRequestInProgress = true;

        chatHistory.Add(new OpenAIMessage { role = "user", content = userMessage });
        string finalSystemPrompt = BuildFinalSystemPrompt();

        // 서버 규격에 맞춘 데이터 조립
        LocalLlamaPayload payload = new LocalLlamaPayload { 
            prompt = userMessage, 
            system = finalSystemPrompt 
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
            if (request.result != UnityWebRequest.Result.Success) {
                chatbotResponse = "연결 오류: " + request.error;
            } else {
                var res = JsonConvert.DeserializeObject<LocalLlamaResponse>(request.downloadHandler.text);
                chatbotResponse = res.response;
                chatHistory.Add(new OpenAIMessage { role = "assistant", content = chatbotResponse });
            }

            isRequestInProgress = false;
            yield return StartCoroutine(HandleChatbotResponse(chatbotResponse));
        }
    }

    protected abstract string BuildFinalSystemPrompt();
    protected abstract IEnumerator HandleChatbotResponse(string responseMessage);
}