using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Plain C# class that owns the chat history list and prompt-composition helpers.
/// Created once per <see cref="BaseChatbot"/> instance in <c>Start()</c>.
/// </summary>
public sealed class ChatHistoryManager
{
    private const string ChesterVoiceCommonResource = "ChesterVoiceCommon";
    private const string DefaultSystemMessage =
        "당신은 저택의 앵무새 체셔입니다. 방별 지침과 공통 말투 규칙(시스템 끝단)을 모두 따릅니다.";

    private readonly List<OpenAIMessage> _history = new List<OpenAIMessage>();
    private readonly bool _appendCommonVoice;

    /// <summary>Direct reference so subclasses can index/enumerate freely.</summary>
    public List<OpenAIMessage> History => _history;

    public ChatHistoryManager(bool appendCommonVoice)
    {
        _appendCommonVoice = appendCommonVoice;
    }

    public void Initialize()
    {
        _history.Clear();
        _history.Add(new OpenAIMessage
        {
            role = "system",
            content = DefaultSystemMessage
        });
    }

    public void AddMessage(string role, string content)
    {
        _history.Add(new OpenAIMessage { role = role, content = content });
    }

    /// <summary>
    /// 모든 /chat·/chat/stream 요청에 공통으로 붙는 말투 블록(Resources/ChesterVoiceCommon.txt).
    /// </summary>
    public string ComposeSystemPromptWithCommonRules(string roomSpecificPrompt)
    {
        if (!_appendCommonVoice || string.IsNullOrEmpty(roomSpecificPrompt))
            return roomSpecificPrompt;

        TextAsset common = Resources.Load<TextAsset>(ChesterVoiceCommonResource);
        if (common == null)
        {
            GameLog.LogWarning(
                $"[ChatHistoryManager] Resources/{ChesterVoiceCommonResource}.txt 없음 — 공통 말투 생략");
            return roomSpecificPrompt;
        }

        return roomSpecificPrompt + "\n\n" + common.text;
    }

    /// <summary>
    /// Applies heuristic info-budget signals (revisit tracking, scene context) to the prompt.
    /// The caller is responsible for building the <see cref="HeuristicSignalInput"/>
    /// (via the virtual <c>BuildHeuristicSignalInput</c> on <see cref="BaseChatbot"/>).
    /// </summary>
    public string ComposeSystemPromptWithHeuristics(string basePrompt, HeuristicSignalInput signal)
    {
        signal = SceneRevisitTracker.Instance.FillRevisitSignals(
            signal, SceneManager.GetActiveScene().name);
        return PromptInfoBudgetComposer.Compose(basePrompt, signal);
    }
}
