using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fungus;
using TMPro;

public class SonRoomChatbot : BaseChatbot
{
    [Header("Son's Room Puzzle Settings")]
    [SerializeField] public Flowchart sonFlowchart;

    [Header("SonRoom UI")]
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

        TextAsset promptAsset = Resources.Load<TextAsset>("SonRoomPrompt");
        if (promptAsset != null) finalSystemPrompt += "\n\n" + promptAsset.text;

        if (sonFlowchart != null)
        {
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
