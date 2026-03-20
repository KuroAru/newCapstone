using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fungus;

public class KitchenChatbot : BaseChatbot
{
    [Header("KitchenBot Settings")]
    [SerializeField] public Flowchart kitchenFlowchart;

    public void TriggerAIResponseByFlag()
    {
        if (isRequestInProgress) return;

        string actionText = "(플레이어가 나에게 음식을 주었다.)";
        StartCoroutine(GetGPTResponse(actionText));
        Debug.Log("call");
    }

    protected override string BuildFinalSystemPrompt()
    {
        string finalSystemPrompt = chatHistory[0].content;

        if (kitchenFlowchart != null)
        {
            bool giveFood = kitchenFlowchart.GetBooleanVariable("giveFood");
            if (giveFood)
            {
                finalSystemPrompt += "\n\n[중요 지시] 플레이어는 당신에게 먹이를 주었습니다. 그에 대한 감사로 '카레'에 대한 힌트를 수수께끼로 내주세요. 수수께끼는 내지만 음식이라는 것은 알 수 있도록 내주세요.";
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
                    Debug.Log($"[{fc.name}] {JsonUtility.ToJson(fc)}");
            }
        }

        if (chatSayDialog != null)
        {
            chatSayDialog.gameObject.SetActive(false);
        }
    }
}
