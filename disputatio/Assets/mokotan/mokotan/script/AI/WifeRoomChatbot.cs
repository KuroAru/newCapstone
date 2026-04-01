using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fungus;
using TMPro;

public class WifeRoomChatbot : BaseChatbot
{
    [Header("WifeRoom Settings")]
    [SerializeField] public Flowchart wifeFlowchart;

    [Header("WifeRoom UI")]
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
                    Debug.Log($"[{fc.name}] {JsonUtility.ToJson(fc)}");
            }
        }
    }
}
