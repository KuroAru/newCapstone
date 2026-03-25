using UnityEngine;

public class SpecialJumpscareEventBridge : MonoBehaviour
{
    /// <summary>
    /// 애니메이션 마지막 프레임 이벤트에서 호출됩니다.
    /// </summary>
    public void OnJumpscareFinished()
    {
        if (SpecialJumpscareManager.Instance != null)
        {
            SpecialJumpscareManager.Instance.OnJumpscareFinished();
        }
    }

    /// <summary>
    /// 2컷, 3컷, 4컷 시작 키프레임의 Animation Event에서 호출됩니다.
    /// 프레임 전환 시 눈깜빡임 효과를 발동합니다.
    /// </summary>
    public void OnFrameTransition()
    {
        if (SpecialJumpscareManager.Instance != null)
        {
            SpecialJumpscareManager.Instance.OnFrameTransition();
        }
    }
}