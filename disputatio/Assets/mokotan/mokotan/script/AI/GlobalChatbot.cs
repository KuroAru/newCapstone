using UnityEngine;
using System.Collections;
using Fungus;
using TMPro;

public class GlobalChatbot : BaseChatbot
{
    [Header("GlobalBot UI")]
    [SerializeField] public Flowchart globalFlowchart;
    [SerializeField] private TMP_InputField userInputField;

    protected override void Start()
    {
        base.Start(); // 부모의 기록 초기화 실행
        if (userInputField != null) userInputField.onSubmit.AddListener(OnSubmit);
    }

    private void OnSubmit(string text)
    {
        if (!string.IsNullOrEmpty(text)) OnSendButtonClick();
    }

    public void OnSendButtonClick()
{
    // 🚨 이 로그가 찍히는지 보세요!
    Debug.Log("🚀 전송 버튼 클릭됨! 입력값: " + userInputField.text);

    string message = userInputField.text;

    if (!string.IsNullOrEmpty(message))
    {
        if (isRequestInProgress)
        {
            Debug.LogWarning("⚠️ 현재 이미 요청 중입니다 (isRequestInProgress가 true임)");
            return;
        }

        Debug.Log("📡 서버로 요청을 시작합니다...");
        StartCoroutine(GetGPTResponse(message)); 
        userInputField.text = ""; 
    }
    else
    {
        Debug.LogWarning("❌ 입력값이 비어있습니다.");
    }
}
    

    protected override string BuildFinalSystemPrompt()
{
    // 1. Resources 폴더에서 "introPrompt" 파일을 로드합니다.
    // 주의: 파일이 Assets/Resources/introPrompt.txt 경로에 있어야 합니다.
    TextAsset promptAsset = Resources.Load<TextAsset>("introPrompt");
    
    // 파일이 있으면 그 내용을 사용하고, 없으면 기본 문구를 사용합니다.
    string basePrompt = (promptAsset != null) ? promptAsset.text : "당신은 저택의 도우미입니다.";

    string finalSystemPrompt = basePrompt;

    // 2. Fungus Flowchart의 변수 상태에 따른 동적 지시 사항 추가
    if (globalFlowchart != null)
    {
        // "GetBottle" 불리언 변수 값을 체크합니다.
        bool hasBottleFlag = globalFlowchart.GetBooleanVariable("GetBottle");
        
        if (chatHistory.Count > 0)
        {
            // 가장 최근 플레이어의 메시지를 확인합니다.
            string lastUserMessage = chatHistory[chatHistory.Count - 1].content;
            
            // 물병 단서를 가졌고, 플레이어가 물병에 대해 물어본다면 수수께끼 지시를 추가합니다.
            if (hasBottleFlag && (lastUserMessage.Contains("물병") || lastUserMessage.Contains("병")))
            {
                finalSystemPrompt += "\n\n[중요 지시] 플레이어는 물병 단서를 가졌습니다. 부력에 대해 수수께끼로 답변하세요.";
            }
        }
    }

    return finalSystemPrompt;
}

    protected override IEnumerator HandleChatbotResponse(string responseMessage)
    {
        bool isComplete = false;
        // ✅ 이제 에러 없이 부모의 Say 함수를 호출합니다.
        Say(responseMessage, () => isComplete = true);
        yield return new WaitUntil(() => isComplete);
    }

    private void OnDestroy()
    {
        if (userInputField != null) userInputField.onSubmit.RemoveListener(OnSubmit);
    }
}