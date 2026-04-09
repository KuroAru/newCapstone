using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fungus;
using System;
using UnityEngine.Events;

public class TutorChatbot : BaseChatbot
{
    [Header("TutorBot Settings")]
    [SerializeField] public Flowchart flowchart;

    [Header("Events")]
    public UnityEvent OnAIReponseComplete;
    public UnityEvent OnQuizCompletedEvent;

    public void AddPlayerMessageAndGetResponse(string playerMessage)
    {
        if (isRequestInProgress)
        {
            Debug.LogWarning("이미 AI 응답 요청이 진행 중입니다. 새로운 요청을 무시합니다.");
            return;
        }

        StartCoroutine(GetGPTResponse(playerMessage));
        Debug.Log($"플레이어 답변 전송 및 GPT 응답 요청: {playerMessage}");
    }

    protected override string BuildFinalSystemPrompt()
    {
        string finalSystemPrompt = chatHistory[0].content;

        if (flowchart != null)
        {
            bool windowClicked = flowchart.GetBooleanVariable("WindowClicked");
            if (windowClicked)
            {
                TextAsset tutorRoomPromptAsset = Resources.Load<TextAsset>("TutorRoomPrompt");
                if (tutorRoomPromptAsset != null)
                {
                    int currentCorrectCount = flowchart.GetIntegerVariable("CorrectAnswerCount");
                    finalSystemPrompt += $"\n\n[현재 진행 상황] 플레이어는 현재까지 {currentCorrectCount}/5번 정답을 맞췄습니다.\n\n" + tutorRoomPromptAsset.text;
                }
                else
                {
                    Debug.LogError("TutorRoomPrompt.txt 파일을 찾을 수 없습니다!");
                    finalSystemPrompt += "\n\n[중요 지시]... (TutorRoomPrompt 내용)...";
                }
            }
        }
        return finalSystemPrompt;
    }

    protected override HeuristicSignalInput BuildHeuristicSignalInput(string userMessage)
    {
        var signal = base.BuildHeuristicSignalInput(userMessage);
        signal.roomName = nameof(TutorChatbot);

        if (flowchart != null)
        {
            int currentCorrectCount = Mathf.Clamp(flowchart.GetIntegerVariable("CorrectAnswerCount"), 0, 5);
            signal.progressScore = currentCorrectCount / 5f;
            signal.accuracyScore = currentCorrectCount / 5f;
        }

        return signal;
    }

    protected override IEnumerator HandleChatbotResponse(string responseMessage, List<FunctionCallData> functionCalls)
    {
        bool isComplete = false;
        Say(responseMessage, () => isComplete = true);
        yield return new WaitUntil(() => isComplete);

        ProcessFunctionCalls(functionCalls);

        OnAIReponseComplete?.Invoke();
    }

    private void ProcessFunctionCalls(List<FunctionCallData> functionCalls)
    {
        if (functionCalls == null) return;

        foreach (var fc in functionCalls)
        {
            switch (fc.name)
            {
                case "update_quiz":
                    HandleUpdateQuiz(fc.arguments);
                    break;
                case "emote":
                    HandleEmote(fc.arguments);
                    break;
                default:
                    Debug.Log($"Unhandled function call: {fc.name}");
                    break;
            }
        }
    }

    private void HandleUpdateQuiz(Dictionary<string, object> args)
    {
        if (args == null) return;

        bool isCorrect = args.ContainsKey("is_correct") && Convert.ToBoolean(args["is_correct"]);
        bool quizComplete = args.ContainsKey("quiz_complete") && Convert.ToBoolean(args["quiz_complete"]);

        if (isCorrect)
        {
            IncrementCorrectAnswerCount();
        }
        if (quizComplete)
        {
            OnQuizCompletedEvent?.Invoke();
        }
    }

    private void HandleEmote(Dictionary<string, object> args)
    {
        if (args == null || !args.ContainsKey("emotion")) return;
        string emotion = args["emotion"].ToString();
        Debug.Log($"Chester emote: {emotion}");
        // TODO: wire to animation controller when Chester animations are ready
    }

    public void IncrementCorrectAnswerCount()
    {
        if (flowchart != null)
        {
            int currentCount = flowchart.GetIntegerVariable("CorrectAnswerCount");
            flowchart.SetIntegerVariable("CorrectAnswerCount", currentCount + 1);
            Debug.Log($"CorrectAnswerCount increased to: {currentCount + 1}");
        }
    }
}
