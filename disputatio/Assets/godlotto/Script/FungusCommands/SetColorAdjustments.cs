using System.Collections;
using Fungus;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[CommandInfo("Post Processing",
             "Set Color Adjustments",
             "URP Volume의 Color Adjustments 효과(노출, 명암)를 설정합니다. Duration이 0이면 즉시 적용, 양수면 SmoothStep으로 부드럽게 변환합니다.")]
[AddComponentMenu("")]
public class SetColorAdjustments : Command
{
    [Tooltip("대상 URP Volume. 비워 두면 씬에서 자동으로 첫 번째 Volume을 찾습니다.")]
    [SerializeField] private Volume targetVolume;

    [Header("Post Exposure")]
    [Tooltip("체크 시 Post Exposure 필드를 적용. 미체크 시 현재 노출값 유지.")]
    [SerializeField] private bool overridePostExposure = true;

    [Tooltip("노출(밝기) 보정값(EV). 양수면 화면이 밝아지고, 음수면 어두워짐. -3 이하는 거의 완전 암흑, 6 이상은 섬광 효과.")]
    [SerializeField] private float postExposure = 0f;

    [Header("Contrast")]
    [Tooltip("체크 시 Contrast 필드를 적용. 미체크 시 현재 명암값 유지.")]
    [SerializeField] private bool overrideContrast = false;

    [Tooltip("명암 대비. 양수면 밝고 어두운 부분의 차이가 강해지고, 음수면 화면이 회색빛으로 평탄해짐.")]
    [Range(-100f, 100f)]
    [SerializeField] private float contrast = 0f;

    [Header("Timing")]
    [Tooltip("변환에 걸리는 시간(초). 0이면 즉시 적용, 양수면 SmoothStep으로 부드럽게 변환.")]
    [SerializeField] private float duration = 0f;

    [Tooltip("체크 시 Duration이 끝날 때까지 플로우차트 진행을 멈춤. 미체크 시 변환 도중에도 다음 커맨드 실행.")]
    [SerializeField] private bool waitUntilFinished = true;

    private Coroutine fadeCoroutine;
    private ColorAdjustments cachedColorAdjustments;

    public override void OnEnter()
    {
        if (!overridePostExposure && !overrideContrast)
        {
            GameLog.LogWarning("[SetColorAdjustments] overridePostExposure와 overrideContrast가 모두 꺼져 있습니다. 아무 효과도 적용되지 않습니다.");
            Continue();
            return;
        }

        Volume volume = targetVolume != null ? targetVolume : Object.FindFirstObjectByType<Volume>();
        if (volume == null || !volume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            GameLog.LogWarning("[SetColorAdjustments] 씬에서 Color Adjustments 효과를 찾을 수 없습니다.");
            Continue();
            return;
        }

        cachedColorAdjustments = colorAdjustments;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        if (duration <= 0f)
        {
            Apply(colorAdjustments, postExposure, contrast);
            cachedColorAdjustments = null;
            Continue();
        }
        else
        {
            fadeCoroutine = StartCoroutine(Fade(colorAdjustments, duration));
            if (!waitUntilFinished) Continue();
        }
    }

    public override void OnStopExecuting()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
            if (cachedColorAdjustments != null)
            {
                Apply(cachedColorAdjustments, postExposure, contrast);
                cachedColorAdjustments = null;
            }
        }
    }

    private void Apply(ColorAdjustments ca, float targetExposure, float targetContrast)
    {
        if (overridePostExposure)
        {
            ca.postExposure.overrideState = true;
            ca.postExposure.value = targetExposure;
        }
        if (overrideContrast)
        {
            ca.contrast.overrideState = true;
            ca.contrast.value = targetContrast;
        }
    }

    private IEnumerator Fade(ColorAdjustments ca, float fadeDuration)
    {
        float startExposure = 0f;
        float startContrast = 0f;

        if (overridePostExposure)
        {
            startExposure = ca.postExposure.value;
            ca.postExposure.overrideState = true;
        }
        if (overrideContrast)
        {
            startContrast = ca.contrast.value;
            ca.contrast.overrideState = true;
        }

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, t / fadeDuration);
            if (overridePostExposure) ca.postExposure.value = Mathf.Lerp(startExposure, postExposure, s);
            if (overrideContrast) ca.contrast.value = Mathf.Lerp(startContrast, contrast, s);
            yield return null;
        }

        Apply(ca, postExposure, contrast);
        cachedColorAdjustments = null;
        fadeCoroutine = null;
        if (waitUntilFinished) Continue();
    }

    public override string GetSummary()
    {
        string durationStr = duration <= 0f ? "즉시" : $"{duration}s";
        string exposurePart = overridePostExposure ? $"PostExposure: {postExposure}, " : "";
        string contrastPart = overrideContrast ? $"Contrast: {contrast}, " : "";
        return $"{exposurePart}{contrastPart}Duration: {durationStr}";
    }

    public override Color GetButtonColor()
    {
        return new Color32(154, 106, 181, 255);
    }
}
