using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fungus;
using TMPro;

public class MainBedroomChatbot : BaseChatbot
{
    [Header("Main Bedroom Puzzle Settings")]
    [SerializeField] public Flowchart mainFlowchart;

    [Header("MainBedRoom UI")]
    [SerializeField] private TMP_InputField userInputField;

    protected override void Start()
    {
        base.Start();
        if (userInputField != null)
            userInputField.onSubmit.AddListener(OnSubmit);
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
        if (userInputField == null)
        {
            Debug.LogWarning("WifeRoomChatbot: userInputField가 연결되지 않았습니다.");
            return;
        }

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

    private void OnDestroy()
    {
        if (userInputField != null)
            userInputField.onSubmit.RemoveListener(OnSubmit);
    }

    protected override string BuildFinalSystemPrompt()
    {
        string finalSystemPrompt = chatHistory[0].content;

        TextAsset promptAsset = Resources.Load<TextAsset>("MainBedroomPrompt");
        if (promptAsset != null) finalSystemPrompt += "\n\n" + promptAsset.text;

        if (mainFlowchart != null)
        {
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
    }
}
