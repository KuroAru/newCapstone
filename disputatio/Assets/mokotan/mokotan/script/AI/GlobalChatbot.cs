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

    private void OnSubmit(string text)
    {
        if (!string.IsNullOrEmpty(text)) OnSendButtonClick();
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

    protected override string BuildFinalSystemPrompt()
    {
        TextAsset promptAsset = Resources.Load<TextAsset>("introPrompt");
        string basePrompt = (promptAsset != null) ? promptAsset.text : "당신은 저택의 도우미입니다.";
        string finalSystemPrompt = basePrompt;

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
