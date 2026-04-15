using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fungus;

public class WifeRoomChatbot : BaseChatbot
{
    [Header("WifeRoom Settings")]
    [SerializeField] public Flowchart wifeFlowchart;

    protected override string BuildFinalSystemPrompt()
    {
        string finalSystemPrompt = chatHistory[0].content;

        TextAsset promptAsset = Resources.Load<TextAsset>("WifeRoomPrompt");
        if (promptAsset != null)
        {
            finalSystemPrompt += "\n\n" + promptAsset.text;
        }

        if (wifeFlowchart != null)
        {
            if (wifeFlowchart.GetBooleanVariable("CheckedMirror"))
            {
                finalSystemPrompt += "\n\n[상황] 플레이어가 거울 속 이면을 발견했습니다. 진실에 다가가는 수수께끼를 내세요.";
            }
        }
        return finalSystemPrompt;
    }

    protected override IEnumerator HandleChatbotResponse(string responseMessage, List<FunctionCallData> functionCalls)
    {
        bool isComplete = false;
        Say(responseMessage, () => isComplete = true);
        yield return new WaitUntil(() => isComplete);

        if (functionCalls != null)
        {
            foreach (var fc in functionCalls)
            {
                if (fc.name == "give_hint" || fc.name == "emote")
                    GameLog.Log($"[{fc.name}] {JsonUtility.ToJson(fc)}");
            }
        }
    }
}
