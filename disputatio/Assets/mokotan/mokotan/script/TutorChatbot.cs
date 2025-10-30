using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;
using Fungus;
using System;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.Events; // UnityEvent를 사용하기 위해 추가

// --- OpenAI API 데이터 구조 클래스 (TutorChatbot.cs 파일 상단에 위치) ---
// 이 클래스들이 다른 파일에 이미 정의되어 있다면 이 부분은 삭제하세요.
// TutorChatbot.cs 파일에 CS0579 에러가 뜬다면, 이 클래스들을 다른 파일로 옮기거나 중복 정의를 제거해야 합니다.


[System.Serializable]
public class ResponseFormat
{
    public string type;
}



[System.Serializable]
public class Choice
{
    public OpenAIMessage message;
    public string finish_reason;
    public int index;
}


// AI의 응답에서 파싱할 JSON 데이터를 위한 새로운 클래스 정의
[System.Serializable]
public class ChatbotStatus
{
    // JSON 키와 정확히 일치해야 합니다. (is_correct, quiz_complete)
    public bool is_correct;
    public bool quiz_complete;
}

// --- TutorChatbot 메인 클래스 ---
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

    [Header("Events")] 
    public UnityEvent OnAIReponseComplete; // AI 응답 표시 완료 후 호출 (다음 입력 필드 활성화 등)
    public UnityEvent OnQuizCompletedEvent; // 퀴즈가 최종 완료되었을 때 호출

    // ▼▼▼ API 키를 로드하기 위한 Awake() 함수 추가 ▼▼▼
    private void Awake()
    {
        LoadAPIKey();
    }

    private void Start()
    {
        InitializeChatHistory();
    }

    // ▼▼▼ API 키를 파일에서 읽어오는 함수 추가 ▼▼▼
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
        TextAsset introTextAsset = Resources.Load<TextAsset>("introPrompt");
        string basePrompt = introTextAsset != null ? introTextAsset.text : "You are a helpful kitchen assistant.";
        chatHistory.Add(new OpenAIMessage { role = "system", content = basePrompt });
    }

    // QuizInputHandler에서 호출하는 함수
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
                TextAsset tutorRoomPromptAsset = Resources.Load<TextAsset>("TutorRoomPrompt");
                if (tutorRoomPromptAsset != null)
                {
                    // 프롬프트에 현재 정답 카운트를 포함하여 AI에게 알려줍니다.
                    int currentCorrectCount = flowchart.GetIntegerVariable("CorrectAnswerCount");
                    finalSystemPrompt += $"\n\n[현재 진행 상황] 플레이어는 현재까지 {currentCorrectCount}/5번 정답을 맞췄습니다.\n\n" + tutorRoomPromptAsset.text;
                    Debug.Log("TutorRoomPrompt loaded and added to system prompt with current count.");
                }
                else
                {
                    Debug.LogError("TutorRoomPrompt.txt 파일을 찾을 수 없습니다! Assets/Resources/TutorRoomPrompt.txt 경로를 확인해주세요.");
                    // 파일 로드 실패 시 대체 프롬프트 (프롬프트 파일 내용과 일치하도록)
                    finalSystemPrompt += "\n\n[중요 지시]\n플레이어가 열쇠를 발견했습니다. 이제부터 당신은 '튜터' 역할을 수행해야 합니다.\n당신은 플레이어에게 성경에 대한 흥미롭고 교육적인 퀴즈를 출제합니다.\n이 퀴즈 미션은 **플레이어가 정답을 총 5번 맞출 때**까지 계속됩니다.\n\n다음은 퀴즈 진행 방식에 대한 엄격한 규칙입니다:\n\n1.  **시작**: 당신은 즉시 첫 번째 퀴즈를 출제합니다. 퀴즈는 누구나 알 법한 성경 인물, 사건, 이야기, 구절 등에서 출제하세요.\n2.  **규칙 설명**: 각 퀴즈를 내기 전에, 플레이어에게 현재까지 **몇 번의 정답을 맞췄는지** 명확히 알려주십시오. (예: \"현재 0/5번 정답을 맞추셨습니다. 첫 번째 퀴즈 나갑니다!\") 그리고 정답을 맞춰야 다음 퀴즈로 넘어갈 수 있음을 다시 한번 강조합니다.\n3.  **정답 확인**: 플레이어가 답변을 하면, 당신은 그 답변이 정답인지 아닌지를 판단해야 합니다.\n    * **정답인 경우**: 플레이어가 정답을 맞췄다고 칭찬하고, 이제 정답 횟수가 한 번 더 늘었음을 알립니다. 그리고 **바로 다음 퀴즈를 출제합니다.** (예: \"정답입니다! 정말 대단하시네요. 이제 1/5번 정답을 맞추셨어요! 자, 다음 퀴즈 나갑니다!\")\n    * **오답인 경우**: 플레이어가 오답을 제출했음을 알리고, 첫 번째 시도였다면 다시 한번 생각할 기회를 주거나, 힌트를 제공할 수 있습니다. 너무 어렵다면 정답을 알려주고 **동일한 퀴즈를 다시 내거나, 다음 퀴즈로 넘어갈지** (이 경우 정답 카운트는 올라가지 않음) 유연하게 판단하십시오. 중요한 것은 정답을 맞춰야 카운트가 올라간다는 것입니다. (예: \"아쉽지만 정답이 아니에요. 조금 더 생각해볼까요?\" 또는 \"정답은 ~입니다. 하지만 정답 카운트는 올라가지 않아요. 다음 퀴즈 나갑니다!\")\n4.  **카운트**: 당신은 플레이어가 **총 몇 번의 정답을 맞췄는지** 대화 기록을 통해 항상 기억하고 있어야 합니다. 정답을 맞출 때마다 이 카운트를 1씩 증가시키고 플레이어에게 알려줍니다.\n5.  **종료**: 플레이어가 **총 5번의 정답을 맞추는 순간**, 당신은 플레이어의 지식을 칭찬하며 미션을 성공적으로 완료했음을 알리고 대화를 마무리합니다. 더 이상의 퀴즈는 내지 않습니다.\n\n**[기술 지시 - 매우 중요! 모든 응답에 포함!]**\n**당신의 모든 응답의 마지막에는 반드시 다음 JSON 포맷을 포함하십시오. 이 JSON은 다른 어떠한 문자열로도 변질되어서는 안 됩니다:**\n**{\"is_correct\": true/false, \"quiz_complete\": true/false}**\n* **\"is_correct\"**: 플레이어의 방금 전 답변이 정답이었으면 `true`, 오답이었으면 `false`로 설정하십시오.\n* **\"quiz_complete\"**: 현재 답변으로 인해 총 5번의 정답 미션이 완료되었으면 `true`, 아직 미완료 상태이면 `false`로 설정하십시오.\n\n[예시 대화 흐름]\n튜터: 현재 0/5번 정답을 맞추셨습니다. 첫 번째 퀴즈 나갑니다! 다윗이 골리앗을 무엇으로 물리쳤을까요? {\"is_correct\": false, \"quiz_complete\": false}\n플레이어: 돌팔매\n튜터: 정답입니다! 정말 대단하시네요. 이제 1/5번 정답을 맞추셨어요! 자, 다음 퀴즈 나갑니다! {\"is_correct\": true, \"quiz_complete\": false}\n...\n튜터: 마지막 정답입니다! 총 5번의 퀴즈를 모두 맞추셨어요! 정말 훌륭한 지식이네요! 미션을 성공적으로 완료했습니다! {\"is_correct\": true, \"quiz_complete\": true}";
                }
            }
        }

        List<OpenAIMessage> requestMessages = new List<OpenAIMessage>(chatHistory);
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

    private IEnumerator DisplayMessageChunks(string message)
    {
        // 1. 메시지에서 JSON 부분 추출 및 파싱
        ChatbotStatus status = null;
        string cleanedMessage = message; // JSON이 제거된 최종 메시지

        int jsonStartIndex = message.LastIndexOf("{\"is_correct\"");
        if (jsonStartIndex != -1)
        {
            string jsonString = message.Substring(jsonStartIndex);
            try
            {
                status = JsonConvert.DeserializeObject<ChatbotStatus>(jsonString);
                cleanedMessage = message.Substring(0, jsonStartIndex).Trim(); // JSON 부분 제거
                Debug.Log($"Parsed AI Status: is_correct={status.is_correct}, quiz_complete={status.quiz_complete}");
            }
            catch (Exception e)
            {
                Debug.LogError("ChatbotStatus JSON 파싱 오류: " + e.Message + " | JSON: " + jsonString);
                status = null; // 파싱 실패 시 상태 초기화
            }
        }

        // 2. JSON이 제거된 메시지를 SayDialog로 표시
        bool isComplete = false;
        Say(cleanedMessage, () => isComplete = true);
        yield return new WaitUntil(() => isComplete);

        // 3. 파싱된 상태에 따라 Fungus 변수 업데이트 및 이벤트 트리거
        if (status != null)
        {
            if (status.is_correct)
            {
                IncrementCorrectAnswerCount(); // 정답 카운트 증가
            }

            if (status.quiz_complete)
            {
                Debug.Log("AI reported quiz is complete. Triggering OnQuizCompletedEvent.");
                OnQuizCompletedEvent?.Invoke(); // 퀴즈 완료 이벤트 호출
            }
        }

        // 4. AI 응답 표시 완료 후 일반 이벤트 호출 (다음 입력 활성화 등)
        OnAIReponseComplete?.Invoke();
        
        // 중요: chatSayDialog.gameObject.SetActive(false);는 퀴즈 최종 완료 시에만 호출해야 합니다.
        // OnQuizCompletedEvent에 연결된 블록에서 SayDialog를 닫는 것이 좋습니다.
    }
   
    // QuizInputHandler에서 호출하는 IncrementCorrectAnswerCount는 이제 AI의 is_correct 응답에 따라
    // 이 함수를 호출하게 되므로, 직접적인 퀴즈 완료 검사는 DisplayMessageChunks에서 하는 것이 더 좋습니다.
    // 이 함수는 단순히 카운트만 증가시킵니다.
    public void IncrementCorrectAnswerCount()
    {
        if (flowchart != null)
        {
            int currentCount = flowchart.GetIntegerVariable("CorrectAnswerCount");
            flowchart.SetIntegerVariable("CorrectAnswerCount", currentCount + 1);
            Debug.Log($"CorrectAnswerCount increased to: {currentCount + 1}");
            // 퀴즈 완료 검사는 AI의 quiz_complete 상태에 의존하므로 여기서 하지 않습니다.
        }
        else
        {
            Debug.LogError("Flowchart가 TutorChatbot에 연결되지 않았습니다.");
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