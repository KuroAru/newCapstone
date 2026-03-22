using UnityEngine;

public class JumpscareEventBridge : MonoBehaviour
{
    public void OnJumpscareFinished()
    {
        // 싱글톤인 JumpscareManager의 함수를 대신 호출해줍니다.
        if (JumpscareManager.Instance != null)
        {
            JumpscareManager.Instance.OnJumpscareFinished();
        }
    }
}