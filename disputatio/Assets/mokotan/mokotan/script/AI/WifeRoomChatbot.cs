using UnityEngine;
using System.Collections;
using Fungus;

public class WifeRoomChatbot : BaseChatbot
{
    [Header("WifeRoom Settings")]
    [SerializeField] public Flowchart wifeFlowchart;

    protected override string BuildFinalSystemPrompt()
    {
        string finalSystemPrompt = chatHistory[0].content;
        
        // 아내의 방 전용 프롬프트 로드
        TextAsset promptAsset = Resources.Load<TextAsset>("WifeRoomPrompt");
        if (promptAsset != null)
        {
            finalSystemPrompt += "\n\n" + promptAsset.text;
        }

        if (wifeFlowchart != null)
        {
            // 거울을 조사했는지 등의 플래그 연동 가능
            if (wifeFlowchart.GetBooleanVariable("CheckedMirror"))
            {
                finalSystemPrompt += "\n\n[상황] 플레이어가 거울 속 이면을 발견했습니다. 진실에 다가가는 수수께끼를 내세요.";
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