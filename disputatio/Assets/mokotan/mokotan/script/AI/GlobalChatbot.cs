using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fungus;
using TMPro;

public class GlobalChatbot : BaseChatbot
{
    [Header("GlobalBot UI")]
    [SerializeField] public Flowchart globalFlowchart;
    [SerializeField] private TMP_InputField userInputField;

    protected override void Start()
    {
        base.Start();
        if (userInputField != null) userInputField.onSubmit.AddListener(OnSubmit);
    }

    /// <summary>
    /// TMP onSubmit may pass an empty string while IME is active or for some line types;
    /// always read the live field text in OnSendButtonClick.
    /// </summary>
    private void OnSubmit(string _)
    {
        OnSendButtonClick();
    }

    public void OnSendButtonClick()
    {
        string message = userInputField.text;

        if (string.IsNullOrEmpty(message))
        {
            Debug.LogWarning("입력값이 비어있습니다.");
            return;
        }

        if (isRequestInProgress)
        {
            Debug.LogWarning("현재 이미 요청 중입니다.");
            return;
        }

        StartCoroutine(GetGPTResponse(message));
        userInputField.text = "";
    }

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

    private void ProcessCommonFunctionCalls(List<FunctionCallData> functionCalls)
    {
        if (functionCalls == null) return;
        foreach (var fc in functionCalls)
        {
            switch (fc.name)
            {
                case "give_hint":
                    HandleGiveHint(fc.arguments);
                    break;
                case "emote":
                    HandleEmote(fc.arguments);
                    break;
            }
        }
    }

    private void HandleGiveHint(Dictionary<string, object> args)
    {
        if (args == null) return;
        string level = args.ContainsKey("hint_level") ? args["hint_level"].ToString() : "subtle";
        string target = args.ContainsKey("target_object") ? args["target_object"].ToString() : "";
        string category = args.ContainsKey("hint_category") ? args["hint_category"].ToString() : "";
        Debug.Log($"[Hint] level={level}, target={target}, category={category}");
    }

    private void HandleEmote(Dictionary<string, object> args)
    {
        if (args == null || !args.ContainsKey("emotion")) return;
        Debug.Log($"Chester emote: {args["emotion"]}");
    }

    private void OnDestroy()
    {
        if (userInputField != null) userInputField.onSubmit.RemoveListener(OnSubmit);
    }
}
