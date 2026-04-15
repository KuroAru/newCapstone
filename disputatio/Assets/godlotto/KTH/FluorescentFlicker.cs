using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class FluorescentFlicker : MonoBehaviour
{
    private Light2D targetLight;
    private Coroutine flickerCoroutine;
    
    [Header("조명 강도 설정")]
    [Tooltip("적이 없을 때(평상시)의 밝기")]
    public float intensityWithoutEnemy = 2.0f;
    [Tooltip("적이 있을 때(점멸 사이)의 기본 밝기")]
    public float intensityWithEnemy = 1.0f;
    [Tooltip("깜빡일 때 덜 밝아지거나 꺼지는 최소 밝기")]
    public float flickerMinIntensity = 0.0f;
    [Tooltip("깜빡일 때 최대 밝기")]
    public float flickerMaxIntensity = 0.5f;

    [Header("시간 설정")]
    [Tooltip("깜빡임 효과가 발생하는 간격 (최소, 최대 초)")]
    public Vector2 timeBetweenFlickers = new Vector2(2.0f, 6.0f);
    [Tooltip("한 번의 깜빡임이 지속되는 시간 (최소, 최대 초)")]
    public Vector2 flickerDuration = new Vector2(0.05f, 0.15f);
    [Tooltip("한 번의 주기 동안 연속으로 깜빡이는 횟수 (최소, 최대)")]
    public Vector2Int flickerCount = new Vector2Int(2, 6);

    private void Awake()
    {
        targetLight = GetComponent<Light2D>();
    }

    private void OnEnable()
    {
        // 일반 점프스케어 매니저 이벤트 구독
        JumpscareManager.OnEnemyAppeared += StartFlicker;
        JumpscareManager.OnJumpscareReset += StopFlicker;
        
        // 스페셜 점프스케어 매니저 이벤트 구독
        SpecialJumpscareManager.OnEnemyAppeared += StartFlicker;
        SpecialJumpscareManager.OnJumpscareReset += StopFlicker;
    }

    private void OnDisable()
    {
        // 구독 해제
        JumpscareManager.OnEnemyAppeared -= StartFlicker;
        JumpscareManager.OnJumpscareReset -= StopFlicker;
        
        SpecialJumpscareManager.OnEnemyAppeared -= StartFlicker;
        SpecialJumpscareManager.OnJumpscareReset -= StopFlicker;
    }

    private void Start()
    {
        // 시작 시 적이 없는 상태이므로 평상시 밝기(2.0)로 설정
        targetLight.intensity = intensityWithoutEnemy;
    }

    private void StartFlicker()
    {
        if (flickerCoroutine == null)
        {
            // 적이 등장했으므로 기본 밝기를 1.0으로 낮춤
            targetLight.intensity = intensityWithEnemy;
            flickerCoroutine = StartCoroutine(FlickerRoutine());
        }
    }

    private void StopFlicker()
    {
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }
        // 적이 사라지고 초기화되었으므로 평상시 밝기(2.0)로 강제 복구
        targetLight.intensity = intensityWithoutEnemy;
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // 1. 형광등 깜빡임 연출 즉시 시작
            int currentFlickerCount = UnityEngine.Random.Range(flickerCount.x, flickerCount.y + 1);
            
            for (int i = 0; i < currentFlickerCount; i++)
            {
                // 어두워짐
                targetLight.intensity = UnityEngine.Random.Range(flickerMinIntensity, flickerMaxIntensity);
                yield return new WaitForSeconds(UnityEngine.Random.Range(flickerDuration.x, flickerDuration.y));

                // 다시 켜질 때, 평상시 밝기(2.0)가 아닌 적 등장 시 기본 밝기(1.0)로 켜짐
                targetLight.intensity = intensityWithEnemy;
                yield return new WaitForSeconds(UnityEngine.Random.Range(flickerDuration.x, flickerDuration.y));
            }
            
            // 2. 깜빡임 종료 후 다음 깜빡임까지 대기 구간도 적 등장 시 기본 밝기 유지
            float waitTime = UnityEngine.Random.Range(timeBetweenFlickers.x, timeBetweenFlickers.y);
            targetLight.intensity = intensityWithEnemy;
            yield return new WaitForSeconds(waitTime);
        }
    }
}