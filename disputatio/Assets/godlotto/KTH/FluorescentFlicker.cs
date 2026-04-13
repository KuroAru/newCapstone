using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class FluorescentFlicker : MonoBehaviour
{
    private Light2D targetLight;
    
    [Header("조명 강도 설정")]
    [Tooltip("정상 상태일 때의 밝기")]
    public float normalIntensity = 1.0f;
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
        // 동일한 게임 오브젝트에 있는 Light2D 컴포넌트를 가져옵니다.
        targetLight = GetComponent<Light2D>();
    }

    private void Start()
    {
        // 시작 시 정상 밝기로 설정하고 코루틴을 실행합니다.
        targetLight.intensity = normalIntensity;
        StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // 1. 다음 깜빡임 이벤트까지 대기 (정상 상태 유지)
            float waitTime = Random.Range(timeBetweenFlickers.x, timeBetweenFlickers.y);
            targetLight.intensity = normalIntensity;
            yield return new WaitForSeconds(waitTime);

            // 2. 형광등 깜빡임 연출 시작
            int currentFlickerCount = Random.Range(flickerCount.x, flickerCount.y + 1);
            
            for (int i = 0; i < currentFlickerCount; i++)
            {
                // 어두워짐
                targetLight.intensity = Random.Range(flickerMinIntensity, flickerMaxIntensity);
                yield return new WaitForSeconds(Random.Range(flickerDuration.x, flickerDuration.y));

                // 다시 원래 밝기로 돌아오려는 시도
                targetLight.intensity = normalIntensity;
                yield return new WaitForSeconds(Random.Range(flickerDuration.x, flickerDuration.y));
            }
            
            // 깜빡임 종료 후 다시 정상 상태로 루프
        }
    }
}