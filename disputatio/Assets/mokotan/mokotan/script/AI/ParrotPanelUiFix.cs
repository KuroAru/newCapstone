using UnityEngine;
using Fungus;

/// <summary>
/// Hall 등에서 Parret 패널이 켜진 뒤에도 Fungus SayDialog(Overlay)의 CanvasGroup이
/// blocksRaycasts=true로 남아 있으면 그 아래 Screen Space Camera UI(입력 필드)가 클릭을 받지 못합니다.
/// 패널이 열릴 때 한 번 레이캐스트 차단을 끕니다.
/// </summary>
[DisallowMultipleComponent]
public class ParrotPanelUiFix : MonoBehaviour
{
    [SerializeField] private SayDialog targetSayDialog;
    [SerializeField] private CanvasGroup sayDialogCanvasGroup;

    private void Awake()
    {
        if (sayDialogCanvasGroup == null && targetSayDialog != null)
            sayDialogCanvasGroup = targetSayDialog.GetComponent<CanvasGroup>();
        if (sayDialogCanvasGroup == null)
        {
            var sd = FindFirstObjectByType<SayDialog>();
            if (sd != null)
                sayDialogCanvasGroup = sd.GetComponent<CanvasGroup>();
        }
    }

    private void OnEnable()
    {
        if (sayDialogCanvasGroup == null)
            return;
        sayDialogCanvasGroup.blocksRaycasts = false;
        sayDialogCanvasGroup.interactable = false;
    }
}
