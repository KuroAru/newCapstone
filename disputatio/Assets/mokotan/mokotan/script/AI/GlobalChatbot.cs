using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fungus;

public class GlobalChatbot : BaseChatbot
{
    [Header("GlobalBot UI")]
    [SerializeField] public Flowchart globalFlowchart;

    private const int BottleItemId = 1;
    private const string FallbackSystemPrompt = "당신은 저택의 도우미입니다.";

    protected override string BuildFinalSystemPrompt()
    {
        TextAsset promptAsset = Resources.Load<TextAsset>("introPrompt");
        string finalSystemPrompt = promptAsset != null ? promptAsset.text : FallbackSystemPrompt;

        if (globalFlowchart == null)
            return finalSystemPrompt;

        finalSystemPrompt += ItemAcquisitionTracker.BuildPromptSection(globalFlowchart);
        finalSystemPrompt += BuildBottleHintIfNeeded();

        return finalSystemPrompt;
    }

    private string BuildBottleHintIfNeeded()
    {
        if (chatHistory.Count == 0)
            return string.Empty;

        string lastMessage = chatHistory[chatHistory.Count - 1].content;
        bool mentionsBottle = lastMessage.Contains("물병") || lastMessage.Contains("병");

        if (mentionsBottle && ItemAcquisitionTracker.IsAcquired(globalFlowchart, BottleItemId))
            return "\n\n[중요 지시] 플레이어는 물병 단서를 가졌습니다. 부력에 대해 수수께끼로 답변하세요.";

        return string.Empty;
    }

    protected override IEnumerator HandleChatbotResponse(string responseMessage, List<FunctionCallData> functionCalls)
    {
        bool isComplete = false;
        Say(responseMessage, () => isComplete = true);
        yield return new WaitUntil(() => isComplete);

        ProcessCommonFunctionCalls(functionCalls);
    }

    /// <summary>Dispatch give_hint / emote (and log unknown tools). Subclasses may override to extend.</summary>
    protected virtual void ProcessCommonFunctionCalls(List<FunctionCallData> functionCalls)
    {
        if (functionCalls == null) return;
        foreach (var fc in functionCalls)
        {
            if (string.IsNullOrEmpty(fc.name))
                continue;
            switch (fc.name)
            {
                case "give_hint":
                    ApplyGiveHint(fc.arguments);
                    break;
                case "emote":
                    ApplyEmote(fc.arguments);
                    break;
                default:
                    Debug.LogWarning($"[{GetType().Name}] Unhandled tool call: {fc.name}");
                    break;
            }
        }
    }

    protected virtual void ApplyGiveHint(Dictionary<string, object> args)
    {
        if (args == null) return;
        string level = ChatbotToolArgs.GetString(args, "hint_level", "subtle");
        string target = ChatbotToolArgs.GetString(args, "target_object");
        string category = ChatbotToolArgs.GetString(args, "hint_category");
        Debug.Log($"[Hint] level={level}, target={target}, category={category}");
    }

    protected virtual void ApplyEmote(Dictionary<string, object> args)
    {
        if (args == null) return;
        string emotion = ChatbotToolArgs.GetString(args, "emotion");
        if (string.IsNullOrEmpty(emotion)) return;
        Debug.Log($"Chester emote: {emotion}");
    }
}
