using UnityEngine;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class UIDialRotator : MonoBehaviour
{
    [Header("한 칸 회전 각도 (기본 36° = 10단계)")]
    public float stepDegrees = 36f;

    [Header("감도 (값이 높을수록 빠름)")]
    [Range(0.1f, 5f)] public float sensitivity = 1.8f;

    [Header("숫자 변경 완료 시 호출되는 이벤트 (마우스 뗐을 때)")]
    public UnityEvent<int> onDigitChanged;

    [Header("UI 숫자 표시용 (선택사항)")]
    public TMP_Text dialText;

    private RectTransform rect;
    private bool dragging;
    private Vector2 centerScreenPos;
    private float lastAngle;
    private float totalRotation;
    private int currentDigit;
    private int finalDigit;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        // 자동으로 UISafeLockController 연결
        var controller = FindObjectOfType<UISafeLockController>();
        if (controller != null)
        {
            if (gameObject.name.Contains("L"))
                onDigitChanged.AddListener(controller.OnLeftChanged);
            else if (gameObject.name.Contains("M"))
                onDigitChanged.AddListener(controller.OnMiddleChanged);
            else if (gameObject.name.Contains("R"))
                onDigitChanged.AddListener(controller.OnRightChanged);
        }
    }

    private void Update()
    {
        // 🔹 마우스 클릭 시작
        if (Input.GetMouseButtonDown(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition))
            {
                dragging = true;
                centerScreenPos = RectTransformUtility.WorldToScreenPoint(null, rect.position);
                Vector2 dir = (Vector2)Input.mousePosition - centerScreenPos;
                lastAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            }
        }

        // 🔹 드래그 중 회전
        if (dragging)
        {
            Vector2 dir = (Vector2)Input.mousePosition - centerScreenPos;
            float newAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float deltaAngle = Mathf.DeltaAngle(lastAngle, newAngle) * sensitivity;

            // 시계 방향 회전 시 숫자 증가
            totalRotation -= deltaAngle;
            rect.localEulerAngles = new Vector3(0, 0, totalRotation);

            int newDigit = Mathf.FloorToInt(((totalRotation / stepDegrees) % 10f + 10f) % 10f);

            // 🔸 숫자 실시간 갱신 (UI만)
            if (newDigit != currentDigit)
            {
                currentDigit = newDigit;
                if (dialText != null)
                {
                    dialText.text = currentDigit.ToString();
                    dialText.ForceMeshUpdate();
                }
            }

            lastAngle = newAngle;
        }

        // 🔹 마우스 뗐을 때 — 여기서만 정답 체크 실행
        if (Input.GetMouseButtonUp(0))
        {
            if (dragging)
            {
                dragging = false;
                finalDigit = currentDigit;
                Debug.Log($"🟢 [UIDialRotator] {gameObject.name} 최종 숫자: {finalDigit}");

                // ✅ UISafeLockController에 알림 (한 번만)
                onDigitChanged?.Invoke(finalDigit);
            }
        }
    }
}
