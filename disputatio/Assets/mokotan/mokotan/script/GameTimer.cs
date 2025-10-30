using UnityEngine;
using UnityEngine.Events; // UnityEvent를 사용하기 위해 추가

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; } // 싱글톤 인스턴스

    [Header("Timer Settings")]
    [Tooltip("타이머가 시작할 시간 (초)")]
    [SerializeField] private float initialTime = 60f;
    [Tooltip("타이머가 0이 되었을 때 호출되는 이벤트")]
    public UnityEvent OnTimerExpired;

    private float currentTime;
    private bool isRunning = false;

    public float CurrentTime => currentTime; // 현재 시간을 외부에서 읽을 수 있도록

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // 필요하다면 씬 전환 시 파괴되지 않도록 설정
    }

    void Start()
    {
        currentTime = initialTime;
    }

    void Update()
    {
        if (isRunning)
        {
            currentTime -= Time.deltaTime; // 매 프레임마다 시간 감소

            if (currentTime <= 0f)
            {
                currentTime = 0f; // 0 이하로 내려가지 않도록
                isRunning = false; // 타이머 중지
                Debug.Log("Timer Expired!");
                OnTimerExpired?.Invoke(); // 타이머 만료 이벤트 호출
            }
            // Debug.Log($"Current Time: {Mathf.CeilToInt(currentTime)}"); // 실시간 디버그 확인
        }
    }

    /// <summary>
    /// 타이머를 시작합니다.
    /// </summary>
    public void StartTimer()
    {
        isRunning = true;
        Debug.Log("Timer Started!");
    }

    /// <summary>
    /// 타이머를 중지합니다.
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
        Debug.Log("Timer Stopped!");
    }

    /// <summary>
    /// 타이머를 초기 시간으로 리셋하고, 필요하면 시작합니다.
    /// </summary>
    /// <param name="andStart">리셋 후 바로 시작할지 여부</param>
    public void ResetTimer(bool andStart = false)
    {
        currentTime = initialTime;
        isRunning = andStart;
        Debug.Log($"Timer Reset to {initialTime}s. Started: {andStart}");
    }

    /// <summary>
    /// 타이머의 초기 시간을 변경합니다.
    /// </summary>
    /// <param name="newTime">새로운 초기 시간 (초)</param>
    public void SetInitialTime(float newTime)
    {
        initialTime = newTime;
        Debug.Log($"Initial Timer Time set to: {newTime}s");
    }
}