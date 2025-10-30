using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;
using Fungus;
using System;
using Newtonsoft.Json;
using TMPro;

// OpenAI API 데이터 구조 클래스들은 별도 파일에 있어야 합니다.
// 예를 들어, OpenAIMessage, OpenAIPayload, OpenAIResponse 등은 별도의 .cs 파일로 분리하는 것이 좋습니다.
// 편의를 위해 여기에 간략하게 포함합니다. 실제 프로젝트에서는 분리하세요.

[System.Serializable]
public class Choice
{
    public OpenAIMessage message;
    public string finish_reason;
    public int index;
}


public class TutorChatbot : MonoBehaviour
{
    // --- Unity & Fungus 연결 변수 ---
    [SerializeField] private SayDialog chatSayDialog;
    [SerializeField] public Flowchart flowchart;
    // --- OpenAI API 설정 ---
    private string API_KEY; 
    [SerializeField] private string modelName = "gpt-4o";
    private const string API_URL = "https://api.openai.com/v1/chat/completions";

    // --- 내부 변수 ---
    private List<OpenAIMessage> chatHistory = new List<OpenAIMessage>();
    private bool isRequestInProgress = false;

    // API 키를 로드하기 위한 Awake() 함수 추가
    private void Awake()
    {
        LoadAPIKey();
    }

    public void IncrementCorrectAnswerCount()
{
    if (flowchart != null)
    {
        int currentCount = flowchart.GetIntegerVariable("CorrectAnswerCount");
        flowchart.SetIntegerVariable("CorrectAnswerCount", currentCount + 1);
        Debug.Log($"CorrectAnswerCount increased to: {currentCount + 1}");

        // 5번 정답을 맞췄는지 확인하고 종료 로직을 트리거
        if (currentCount + 1 >= 5)
        {
            Debug.Log("5 correct answers achieved! Triggering quiz end.");
            // 여기에 퀴즈 종료 Fungus Block을 실행하는 코드를 추가할 수 있습니다.
            // 예: flowchart.ExecuteBlock("QuizCompleteBlock");
        }
    }
    else
    {
        Debug.LogError("Flowchart가 TutorChatbot에 연결되지 않았습니다.");
    }
}

    private void Start()
    {
        InitializeChatHistory();
    }

    // API 키를 파일에서 읽어오는 함수 추가
    private void LoadAPIKey()
    {
        TextAsset keyFile = Resources.Load<TextAsset>("APIKey"); // Resources 폴더의 APIKey.txt 로드
        if (keyFile != null)
        {
            API_KEY = keyFile.text.Trim(); // Trim()으로 공백 제거
            Debug.Log("OpenAI API Key loaded successfully for TutorChatbot.");
        }
        else
        {
            Debug.LogError("API 키 파일을 찾을 수 없습니다! Assets/Resources/APIKey.txt 경로를 확인해주세요.");
        }
    }

    private void InitializeChatHistory()
    {
        chatHistory.Clear();
        TextAsset introTextAsset = Resources.Load<TextAsset>("TutorRoomPrompt");
        string basePrompt = introTextAsset != null ? introTextAsset.text : "You are a helpful kitchen assistant.";
        chatHistory.Add(new OpenAIMessage { role = "system", content = basePrompt });
    }

    public void TriggerAIResponseByFlag()
    {
        if (isRequestInProgress) return;
        string actionText = "(플레이어가 나에게 음식을 주었다.)"; // 이 텍스트도 변수로 빼거나 별도 파일로 관리할 수 있습니다.
        chatHistory.Add(new OpenAIMessage { role = "user", content = actionText });
        StartCoroutine(GetGPTResponse());
        Debug.Log("TriggerAIResponseByFlag called, initiating GPT response.");
    }
   
    
    private IEnumerator GetGPTResponse()
    {
        isRequestInProgress = true;
        Say("...", null); // GPT 응답 대기 중임을 표시

        string finalSystemPrompt = chatHistory[0].content; // 기존 시스템 프롬프트

        if (flowchart != null)
        {
            bool WindowClicked = flowchart.GetBooleanVariable("WindowClicked");
            if (WindowClicked)
            {
                // Resources 폴더에서 "TutorRoomPrompt.txt" 파일을 로드합니다.
                TextAsset tutorRoomPromptAsset = Resources.Load<TextAsset>("TutorRoomPrompt");
                if (tutorRoomPromptAsset != null)
                {
                    finalSystemPrompt += "\n\n" + tutorRoomPromptAsset.text; // 파일 내용을 추가
                    Debug.Log("TutorRoomPrompt loaded and added to system prompt.");
                }
                else
                {
                    Debug.LogError("TutorRoomPrompt.txt 파일을 찾을 수 없습니다! Assets/Resources/TutorRoomPrompt.txt 경로를 확인해주세요.");
                    // 파일이 없을 경우 기존 하드코딩된 프롬프트를 유지하거나, 대체 프롬프트 사용
                    finalSystemPrompt += "\n\n[중요 지시] 플레이어가 열쇠를 발견했습니다. 누구나 알만한 성경에 대한 수수께끼를 5번 주고 받으싶시오.";
                }
            }
        }

        List<OpenAIMessage> requestMessages = new List<OpenAIMessage>(chatHistory);
        // 최종적으로 구성된 시스템 프롬프트를 첫 번째 메시지로 설정
        requestMessages[0] = new OpenAIMessage { role = "system", content = finalSystemPrompt };

        OpenAIPayload payload = new OpenAIPayload { model = this.modelName, messages = requestMessages };
        string payloadJson = JsonConvert.SerializeObject(payload);

        using (UnityWebRequest request = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(payloadJson);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + API_KEY);

            yield return request.SendWebRequest();

            string chatbotResponse;
            if (request.result != UnityWebRequest.Result.Success)
            {
                chatbotResponse = "오류: " + request.error;
                Debug.LogError("Error: " + request.error + " | Response: " + request.downloadHandler.text);
            }
            else
            {
                chatbotResponse = ParseGPTResponse(request.downloadHandler.text);
                chatHistory.Add(new OpenAIMessage { role = "assistant", content = chatbotResponse });
            }

            isRequestInProgress = false;
            yield return StartCoroutine(DisplayMessageChunks(chatbotResponse));
        }
    }

    private string ParseGPTResponse(string json)
    {
        try
        {
            OpenAIResponse response = JsonConvert.DeserializeObject<OpenAIResponse>(json);
            return response.choices[0].message.content;
        }
        catch (Exception e)
        {
            Debug.LogError("JSON 파싱 오류: " + e.Message);
            return "답변 파싱 실패";
        }
    }

    public void AddPlayerMessageAndGetResponse(string playerMessage)
{
    if (isRequestInProgress)
    {
        Debug.LogWarning("이미 AI 응답 요청이 진행 중입니다. 새로운 요청을 무시합니다.");
        return;
    }

    chatHistory.Add(new OpenAIMessage { role = "user", content = playerMessage });
    StartCoroutine(GetGPTResponse());
    Debug.Log($"플레이어 답변 전송 및 GPT 응답 요청: {playerMessage}");
}

    private IEnumerator DisplayMessageChunks(string message)
    {
        bool isComplete = false;
        Say(message, () => isComplete = true);
        yield return new WaitUntil(() => isComplete);

        if (message.Contains("정답") || message.Contains("맞았") || message.Contains("correct")) // 필요한 키워드 추가
    {
        // 만약 '퀴즈 종료' 메시지에도 '정답'이 포함될 수 있다면, 이를 필터링하는 로직이 필요합니다.
        // 예를 들어, 메시지에 '미션 완료', '종료' 같은 키워드가 있으면 카운트하지 않도록.
        if (!message.Contains("미션을 성공적으로 완료") && !message.Contains("종료"))
        {
             IncrementCorrectAnswerCount();
        }
    }

        if (chatSayDialog != null)
        {
            chatSayDialog.gameObject.SetActive(false);
        }
    }
   
    private void Say(string message, System.Action onComplete = null)
    {
        if (chatSayDialog != null)
        {
            if (!chatSayDialog.gameObject.activeInHierarchy) chatSayDialog.gameObject.SetActive(true);
            chatSayDialog.Say(message, true, true, false, true, true, null, onComplete);
        }
    }
}