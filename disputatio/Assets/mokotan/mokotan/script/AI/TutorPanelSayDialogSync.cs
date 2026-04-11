using UnityEngine;
using Fungus;

/// <summary>
/// 튜터 퀴즈 패널(부모)과 Fungus SayDialog 활성화를 맞춥니다. Fungus Set Active로 패널만 켜고 끌 때 Say가 남거나 반대로 사라지는 문제를 방지합니다.
/// </summary>
[DisallowMultipleComponent]
public sealed class TutorPanelSayDialogSync : MonoBehaviour
{
    private SayDialog _sayDialog;

    public void Initialize(SayDialog sayDialog)
    {
        _sayDialog = sayDialog;
    }

    private void OnEnable()
    {
        if (_sayDialog != null)
            _sayDialog.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (_sayDialog == null)
            return;
        _sayDialog.Stop();
        _sayDialog.gameObject.SetActive(false);
    }
}
