using UnityEngine;

/// <summary>
/// Fungus SayDialog UI가 활성화되면 Animator의 TalkTrigger를 발동시키고,
/// 비활성화되면 Idle로 돌아갑니다.
/// </summary>
public class SayDialogAnimController : MonoBehaviour
{
    [Header("감시 대상")]
    [Tooltip("Fungus SayDialog UI 오브젝트")]
    public GameObject sayDialogObject;

    [Header("제어 대상")]
    [Tooltip("SayDialog가 켜져있을 때 애니메이션을 재생할 Animator")]
    public Animator targetAnimator;

    private bool wasDialogActive = false;

    private void Update()
    {
        if (sayDialogObject == null || targetAnimator == null) return;

        bool dialogActive = sayDialogObject.activeInHierarchy;

        // SayDialog가 켜지는 순간 → TalkTrigger 발동
        if (dialogActive && !wasDialogActive)
        {
            targetAnimator.SetTrigger("TalkTrigger");
        }

        wasDialogActive = dialogActive;
    }
}