using System.Collections;
using UnityEngine;
using Fungus;

namespace Godlotto.FungusIntegration
{
    /// <summary>
    /// 대기 프레임 코루틴이 중복으로 쌓이거나 블록이 이미 실행 중일 때 재실행을 막습니다.
    /// </summary>
    [EventHandlerInfo("Sprite",
        "Object Clicked (Guarded)",
        "Object Clicked와 동일하되, 대기 중 재클릭·이미 실행 중인 블록 재진입을 막습니다.")]
    [AddComponentMenu("")]
    public class GuardedObjectClicked : ObjectClicked
    {
        bool pendingClick;

        public override void OnObjectClicked(Clickable2D clicked)
        {
            if (clicked != clickableObject)
                return;

            if (pendingClick)
                return;

            if (ParentBlock != null && ParentBlock.IsExecuting())
                return;

            pendingClick = true;
            StartCoroutine(ExecuteBlockThenRelease());
        }

        IEnumerator ExecuteBlockThenRelease()
        {
            yield return StartCoroutine(DoExecuteBlock(waitFrames));
            pendingClick = false;
        }
    }
}
