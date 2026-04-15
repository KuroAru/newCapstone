using System.Text;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fungus;

public class KitchenChatbot : BaseChatbot
{
    [Header("KitchenBot Settings")]
    [SerializeField] public Flowchart kitchenFlowchart;

    [Header("Kitchen puzzle — 요리책 카레 (시스템 주입, 플레이어 비표시)")]
    [Tooltip("요리책에서 카레 레시피가 나오는 첫 페이지 번호.")]
    [SerializeField] private int curryRecipePageStart = 18;
    [Tooltip("이어지는 다음 페이지(연속 장).")]
    [SerializeField] private int curryRecipePageEnd = 19;

    public void TriggerAIResponseByFlag()
    {
        if (isRequestInProgress) return;

        string actionText = "(플레이어가 나에게 음식을 주었다.)";
        StartCoroutine(GetGPTResponse(actionText));
        GameLog.Log("call");
    }

    protected override string BuildFinalSystemPrompt()
    {
        string finalSystemPrompt = chatHistory[0].content;

        TextAsset promptAsset = Resources.Load<TextAsset>("KitchenPrompt");
        if (promptAsset != null)
            finalSystemPrompt += "\n\n" + promptAsset.text;

        if (kitchenFlowchart != null)
        {
            bool giveFood = kitchenFlowchart.GetBooleanVariable("giveFood");
            if (giveFood)
            {
                finalSystemPrompt += BuildGiveFoodSecretDesignBlock(curryRecipePageStart, curryRecipePageEnd);
                finalSystemPrompt +=
                    "\n\n[중요 지시 — 먹이 직후 한 번의 응답] "
                    + "플레이어가 먹이를 주었다. 위 [설계자 전용] 사실을 근거로, "
                    + "KitchenPrompt의 카레·페이지 메타 힌트·말투 규칙을 모두 지켜 **짧은 한 마디**로 응답하라. "
                    + "ChesterVoiceCommon의 길이·문장 수·말끝 규칙을 반드시 따른다.";
            }
        }
        return finalSystemPrompt;
    }

    /// <summary>플레이어에게 노출되지 않는 퍼즐 진실. LLM만 읽는다.</summary>
    private static string BuildGiveFoodSecretDesignBlock(int pageA, int pageB)
    {
        var sb = new StringBuilder(800);
        sb.Append("\n\n[설계자 전용 — 플레이어에게 출력·인용 금지, 내부 사실만]\n");
        sb.Append("- 이 방에서 플레이어가 맞춰야 할 단서 축은 **요리책의 페이지**(연속한 두 장)와 연결된다.\n");
        sb.Append("- 카레(황금 국물·재료)은 그 **연속 두 페이지**에 걸쳐 있다는 설정이다.\n");
        sb.Append("- 두 쪽의 번호는 앞장 ");
        sb.Append(pageA);
        sb.Append(", 바로 이어지는 다음 장 ");
        sb.Append(pageB);
        sb.Append(" 이다.\n");
        sb.Append("- 대사에는 **아라비아 숫자**, **‘N쪽’ ‘N페이지’**, **한글 수사로 쪽수를 직접 말하기**(예: ‘열여덟 쪽’)를 **쓰지 마라**. 스포일이다.\n");
        sb.Append("- **카레·요리** 쪽 힌트와 **책장·넘김·연속 두 장·양면·이웃한 쪽**이 중요하다는 **느낌**만, 수수께끼·비꼼으로 섞어 전달하라.\n");
        sb.Append("- 건방진 주방 체셔 말투로, ChesterVoiceCommon 한도 안에서만.\n");
        return sb.ToString();
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

        if (chatSayDialog != null)
        {
            chatSayDialog.gameObject.SetActive(false);
        }
    }
}
