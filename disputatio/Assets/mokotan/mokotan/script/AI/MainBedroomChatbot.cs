using UnityEngine;
using System.Collections;
using Fungus;

public class MainBedroomChatbot : BaseChatbot
{
    [Header("Main Bedroom Puzzle Settings")]
    [SerializeField] public Flowchart mainFlowchart;

    protected override string BuildFinalSystemPrompt()
    {
        string finalSystemPrompt = chatHistory[0].content; // 앵무새 기본 자아
        
        TextAsset promptAsset = Resources.Load<TextAsset>("MainBedroomPrompt");
        if (promptAsset != null) finalSystemPrompt += "\n\n" + promptAsset.text;

        if (mainFlowchart != null)
        {
            // [행동 체크] 일기장 읽기 여부 및 금고 해결 상태 연동
            bool diaryRead = mainFlowchart.GetBooleanVariable("DiaryRead");
            bool safeSolved = mainFlowchart.GetBooleanVariable("SafeSolved");

            if (!diaryRead)
            {
                finalSystemPrompt += "\n\n[현재 목표] 플레이어가 아직 일기장을 읽지 않았습니다. 침대 조사를 강하게 유도하세요.";
            }
            else if (diaryRead && !safeSolved)
            {
                finalSystemPrompt += "\n\n[현재 목표] 일기장은 읽었으나 금고를 못 열었습니다. 창문(마리아), 포스터(라자루스), 그림(마르타) 사이의 십자가를 '더하기(+)'로 인식시켜 숫자를 합산하게 하세요.";
            }
            else if (safeSolved)
            {
                finalSystemPrompt += "\n\n[현재 목표] 금고가 열렸습니다. 성배와 열쇠를 챙겨 지하로 내려가라고 조롱하며 지시하세요.";
            }
        }
        return finalSystemPrompt;
    }

    protected override IEnumerator HandleChatbotResponse(string responseMessage)
    {
        bool isComplete = false;
        Say(responseMessage, () => isComplete = true);
        yield return new WaitUntil(() => isComplete);
    }
}