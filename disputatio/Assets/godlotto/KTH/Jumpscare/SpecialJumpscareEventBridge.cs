using UnityEngine;

public class SpecialJumpscareEventBridge : MonoBehaviour
{
    // 애니메이션 이벤트에서 이 함수를 선택하게 됩니다.
    public void OnJumpscareFinished()
    {
        // 매니저를 찾아서 함수 실행
        if (SpecialJumpscareManager.Instance != null)
        {
            SpecialJumpscareManager.Instance.OnJumpscareFinished();
        }
    }
}