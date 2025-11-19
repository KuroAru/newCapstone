using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostExposureController : MonoBehaviour
{
    [SerializeField] private Volume postProcessVolume;

    private ColorAdjustments colorAdjustments;
    private Vignette vignette;
    private Bloom bloom;

    private Coroutine postExposureFadeCoroutine;
    private Coroutine contrastFadeCoroutine;
    private Coroutine eyeBlinkCoroutine;

    [Tooltip("효과가 원래 값으로 돌아가는 데 걸리는 시간(초)")]
    [SerializeField] private float fadeDuration = 3.0f;

    void Start()
    {
        if (postProcessVolume == null)
        {
            Debug.LogError("Post Process Volume이 Inspector에 연결되지 않았습니다!");
            return;
        }

        // Volume Profile의 컴포넌트들을 가져옵니다.
        if (postProcessVolume.profile.TryGet(out colorAdjustments)) Debug.Log("Color Adjustments 찾음");
        if (postProcessVolume.profile.TryGet(out vignette)) Debug.Log("Vignette 찾음");
        if (postProcessVolume.profile.TryGet(out bloom)) Debug.Log("Bloom 찾음");
    }

    // ---------------------------------------------------------
    // ★ 펑구스(Fungus) 연동용 함수들 (수정됨)
    // ---------------------------------------------------------

    /// <summary>
    /// [눈 감기] Vignette: 1, Exposure: -3 (어둡게)
    /// </summary>
    public void SetEyesClosed()
    {
        if (vignette != null)
        {
            vignette.active = true;
            vignette.intensity.overrideState = true; // 체크박스 강제 활성화
            vignette.intensity.value = 1f;
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.active = true;
            colorAdjustments.postExposure.overrideState = true; // 체크박스 강제 활성화
            colorAdjustments.postExposure.value = -3f; // 눈 감았을 때는 어둡게
        }
        
        Debug.Log("눈을 감았습니다. (Vignette: 1, Exposure: -3)");
    }

    /// <summary>
    /// [눈 뜨기] Vignette: 0.3, Exposure: 2.0 (밝게!)
    /// 사용자 요청 반영: Exposure 값을 2.0으로 상향 조정
    /// </summary>
    public void SetEyesOpened()
    {
        if (vignette != null)
        {
            vignette.active = true;
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.3f;
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.active = true;
            colorAdjustments.postExposure.overrideState = true;
            
            // ★ 수정된 부분: -0.6 -> 2.0 (배경이 잘 보이도록 밝기 올림)
            colorAdjustments.postExposure.value = 2.0f; 
        }

        Debug.Log("눈을 떴습니다. (Vignette: 0.3, Exposure: 2.0)");
    }

    /// <summary>
    /// [부드럽게 눈 뜨기] 서서히 밝아짐 (-3 -> 2.0)
    /// </summary>
    public void SetEyesOpenSmoothly(float duration)
    {
        if (duration <= 0)
        {
            SetEyesOpened();
            return;
        }

        if (eyeBlinkCoroutine != null) StopCoroutine(eyeBlinkCoroutine);
        eyeBlinkCoroutine = StartCoroutine(SmoothEyeOpen(duration));
    }

    private IEnumerator SmoothEyeOpen(float duration)
    {
        float time = 0f;
        
        // 시작 값 (눈 감음)
        float startVig = 1f;
        float startExp = -3f;

        // 목표 값 (눈 뜸) - ★ 여기도 2.0으로 수정
        float endVig = 0.3f;
        float endExp = 2.0f; 

        // 코루틴 시작 전 overrideState 확실히 켜기
        if (vignette != null)
        {
            vignette.active = true;
            vignette.intensity.overrideState = true; 
            vignette.intensity.value = startVig;
        }
        if (colorAdjustments != null)
        {
            colorAdjustments.active = true;
            colorAdjustments.postExposure.overrideState = true;
            colorAdjustments.postExposure.value = startExp;
        }

        Debug.Log($"SmoothEyeOpen 시작: {duration}초 동안 Exposure {startExp} -> {endExp}로 변경");

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float tSmooth = Mathf.SmoothStep(0f, 1f, t); // 부드러운 가속

            if (vignette != null) 
                vignette.intensity.value = Mathf.Lerp(startVig, endVig, tSmooth);
            
            if (colorAdjustments != null) 
                colorAdjustments.postExposure.value = Mathf.Lerp(startExp, endExp, tSmooth);

            yield return null;
        }

        // 최종 값 확정
        SetEyesOpened();
    }

    // ---------------------------------------------------------
    // ▼ 기존 레거시 함수들 (유지)
    // ---------------------------------------------------------

    public void TriggerFlashEffect(float peakExposure = 6f, float targetExposure = 0f)
    {
        if (colorAdjustments == null) return;
        if (postExposureFadeCoroutine != null) StopCoroutine(postExposureFadeCoroutine);
        postExposureFadeCoroutine = StartCoroutine(FadeEffect(
            (value) => { 
                colorAdjustments.postExposure.overrideState = true;
                colorAdjustments.postExposure.value = value; 
            },
            peakExposure, targetExposure, fadeDuration
        ));
    }

    public void TriggerBlinkEffect(float peakContrast = 50f, float targetContrast = 0f)
    {
        if (colorAdjustments == null) return;
        if (contrastFadeCoroutine != null) StopCoroutine(contrastFadeCoroutine);
        contrastFadeCoroutine = StartCoroutine(FadeEffect(
            (value) => { 
                colorAdjustments.contrast.overrideState = true;
                colorAdjustments.contrast.value = value; 
            },
            peakContrast, targetContrast, fadeDuration
        ));
    }
    
    public void SetVignetteToZero(float value)
    {
        if (vignette != null)
        {
            vignette.intensity.overrideState = true;
            vignette.intensity.value = value;
        }
    }

    public void SetBloomToZero(float value)
    {
        if (bloom != null)
        {
            bloom.intensity.overrideState = true;
            bloom.intensity.value = value;
        }
    }

    private IEnumerator FadeEffect(System.Action<float> setter, float startValue, float endValue, float duration)
    {
        setter(startValue);
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float currentValue = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
            setter(currentValue);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        setter(endValue);
    }
}