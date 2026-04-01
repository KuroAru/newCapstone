using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fungus;

public class SonRoomChatbot : BaseChatbot
{
    /// <summary><see cref="ItemAcquisitionTracker"/> 레거시 매핑(HasBible)과 동일한 성경 아이템 ID.</summary>
    private const int IllustratedBibleItemId = 19;

    [Header("Son's Room Puzzle Settings")]
    [SerializeField] public Flowchart sonFlowchart;

    protected override string BuildFinalSystemPrompt()
    {
        string finalSystemPrompt = chatHistory[0].content;

        TextAsset promptAsset = Resources.Load<TextAsset>("SonRoomPrompt");
        if (promptAsset != null) finalSystemPrompt += "\n\n" + promptAsset.text;

        if (sonFlowchart != null)
        {
            // 성경: Variablemanager(AcquiredItemsMask)에 기록됨. ChildRoom 플로우차트에는 HasBible이 없을 수 있음.
            Flowchart globalFc = FlowchartLocator.Find();
            bool hasBible =
                (globalFc != null && ItemAcquisitionTracker.IsAcquired(globalFc, IllustratedBibleItemId))
                || sonFlowchart.GetBooleanVariable("HasBible");

            // 실제 씬 퍼즐은 seal1~7 + SealManager → allSealsComplete (HorsesPlacedCount는 미사용·미갱신이었음).
            bool sealsComplete = sonFlowchart.GetBooleanVariable("allSealsComplete");

            if (!hasBible)
            {
                finalSystemPrompt += "\n\n[현재 목표] 플레이어가 아직 일러스트가 들어간 성경 단서를 못 찾았습니다. 서재 책장 등을 조사하도록 유도하세요.";
            }
            else if (!sealsComplete)
            {
                finalSystemPrompt += "\n\n[현재 목표] 성경 단서는 확보했으나 칠각형 인장 퍼즐이 미완성입니다. 로마 숫자·인장(봉인) 순서와 씬 안 표식을 맞추라고 짧게 지시하세요.";
            }
            else
            {
                finalSystemPrompt += "\n\n[현재 목표] 퍼즐이 풀렸습니다. 침대 밑·벽 등에서 감옥 열쇠와 나무 조각을 찾으라고 조롱하듯 말하세요.";
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
