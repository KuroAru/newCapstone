using UnityEngine;
using System.Collections;
using Fungus;

public class SonRoomChatbot : BaseChatbot
{
    [Header("Son's Room Puzzle Settings")]
    [SerializeField] public Flowchart sonFlowchart;

    protected override string BuildFinalSystemPrompt()
    {
        string finalSystemPrompt = chatHistory[0].content; // 앵무새 기본 자아
        
        TextAsset promptAsset = Resources.Load<TextAsset>("SonRoomPrompt");
        if (promptAsset != null) finalSystemPrompt += "\n\n" + promptAsset.text;

        if (sonFlowchart != null)
        {
            // [행동 체크] 플레이어의 퍼즐 진행 상황에 따른 동적 지침 추가
            bool hasBible = sonFlowchart.GetBooleanVariable("HasBible");
            int horsesPlaced = sonFlowchart.GetIntegerVariable("HorsesPlacedCount");

            if (!hasBible)
            {
                finalSystemPrompt += "\n\n[현재 목표] 플레이어가 아직 성경책을 못 찾았습니다. 책장을 조사하도록 유도하세요.";
            }
            else if (hasBible && horsesPlaced < 4)
            {
                finalSystemPrompt += "\n\n[현재 목표] 성경책은 찾았으나 목마 배치가 미완성입니다. 로마 숫자 I~IV 위치와 목마 상징물을 연결하라고 지시하세요.";
            }
            else if (horsesPlaced >= 4)
            {
                finalSystemPrompt += "\n\n[현재 목표] 퍼즐이 풀렸습니다. 침대 밑이나 벽에서 '감옥 열쇠'를 찾으라고 조롱하듯 말하세요.";
            }
        }
        return finalSystemPrompt;
    }

    protected override IEnumerator HandleChatbotResponse(string responseMessage)
    {
        bool isComplete = false;
        Say(responseMessage, () => isComplete = true); // 체셔의 수수께끼 출력
        yield return new WaitUntil(() => isComplete);
    }
}