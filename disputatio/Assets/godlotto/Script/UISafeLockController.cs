using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Fungus;

public class UISafeLockController : MonoBehaviour
{
    [Header("다이얼 (1개)")]
    public UIDialRotator dial;

    [Header("정답 순서 — 왼쪽부터 차례대로 입력")]
    public int[] targets = { 3, 0, 5 };

    [Header("단계 표시 UI (선택)")]
    public TMP_Text stepIndicatorText;
    public TMP_Text feedbackText;

    [Header("Fungus 연동 (선택)")]
    public Flowchart flowchart;
    public string allVar = "safe_all_ok";

    [Header("UI 토글 이미지")]
    public GameObject closedImage;
    public GameObject openImage;

    [Header("이벤트")]
    public UnityEvent onUnlock;
    public UnityEvent onStepCorrect;
    public UnityEvent onWrong;

    private int currentStep;
    private int currentDialValue;
    private bool unlocked;

    private const string UnlockKey = "SafeLock_Unlocked";

    private void Start()
    {
        unlocked = PlayerPrefs.GetInt(UnlockKey, 0) == 1;
        currentStep = 0;
        RefreshStepUI();

        if (unlocked)
            GameLog.Log("[Safe] 이미 열린 상태");
    }

    // UIDialRotator.onDigitChanged 에 인스펙터에서 연결
    public void OnDialChanged(int val)
    {
        currentDialValue = val;
    }

    // 확인 버튼 onClick 에 연결
    public void Confirm()
    {
        if (unlocked) return;

        if (currentDialValue == targets[currentStep])
        {
            currentStep++;
            onStepCorrect?.Invoke();
            GameLog.Log($"[Safe] {currentStep}/{targets.Length} 단계 성공");

            if (currentStep >= targets.Length)
                Unlock();
            else
                RefreshStepUI();
        }
        else
        {
            GameLog.Log($"[Safe] 틀림 — {currentDialValue} (정답 {targets[currentStep]}), 처음부터 다시");
            currentStep = 0;
            onWrong?.Invoke();
            RefreshStepUI();
        }
    }

    private void Unlock()
    {
        unlocked = true;
        PlayerPrefs.SetInt(UnlockKey, 1);
        PlayerPrefs.Save();

        if (flowchart != null)
            flowchart.SetBooleanVariable(allVar, true);

        if (closedImage != null) closedImage.SetActive(false);
        if (openImage != null)   openImage.SetActive(true);

        onUnlock?.Invoke();
        GameLog.Log("[Safe] 금고 열림!");
    }

    private void RefreshStepUI()
    {
        if (stepIndicatorText != null)
            stepIndicatorText.text = $"{currentStep + 1}  /  {targets.Length}";

        if (feedbackText != null)
            feedbackText.text = currentStep == 0
                ? ""
                : $"✓  {currentStep}번 맞음";
    }

    // 에디터에서 금고 상태 초기화용
    public void EditorResetLock()
    {
        PlayerPrefs.DeleteKey(UnlockKey);
        PlayerPrefs.Save();
        unlocked = false;
        currentStep = 0;
        RefreshStepUI();
        GameLog.Log("[Safe] 금고 초기화됨");
    }
}
