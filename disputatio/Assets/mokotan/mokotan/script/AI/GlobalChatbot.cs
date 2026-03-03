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
    

    protected override string BuildFinalSystemPrompt()
    {
        string finalSystemPrompt = chatHistory.Count > 0 ? chatHistory[0].content : "당신은 저택의 도우미입니다.";

        if (globalFlowchart != null)
        {
            bool hasBottleFlag = globalFlowchart.GetBooleanVariable("GetBottle");
            if (chatHistory.Count > 0)
            {
                string lastUserMessage = chatHistory[chatHistory.Count - 1].content;
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