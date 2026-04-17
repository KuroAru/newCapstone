using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class FlameFlicker : MonoBehaviour
{
    private Light2D targetLight;
    
    [Header("불꽃 밝기 설정")]
    [Tooltip("불꽃이 가장 어두울 때의 밝기")]
    public float minIntensity = 0.8f;
    [Tooltip("불꽃이 가장 밝을 때의 밝기")]
    public float maxIntensity = 1.2f;

    [Header("일렁임 속도")]
    [Tooltip("값이 클수록 불꽃이 빠르게 일렁입니다.")]
    public float flickerSpeed = 1.5f;

    // 여러 개의 불꽃이 씬에 있을 때 모두 똑같이 일렁이는 것을 방지하기 위한 오프셋 변수
    private float randomOffset;

    private void Awake()
    {
        targetLight = GetComponent<Light2D>();
        
        // 스크립트가 실행될 때마다 노이즈 그래프의 시작 위치를 무작위로 설정합니다.
        randomOffset = Random.Range(0f, 100f); 
    }

    private void Update()
    {
        // Time.time을 이용해 시간이 흐름에 따라 변하는 x 좌표를 만듭니다.
        // Mathf.PerlinNoise는 항상 0.0 ~ 1.0 사이의 부드러운 난수 값을 반환합니다.
        float noiseValue = Mathf.PerlinNoise(Time.time * flickerSpeed + randomOffset, 0f);
        
        // 반환된 0.0 ~ 1.0 사이의 값을 minIntensity와 maxIntensity 사이의 값으로 변환하여 적용합니다.
        targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noiseValue);
    }
}