using System.Collections;
using Fungus;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[CommandInfo("Post Processing",
             "Set Vignette",
             "URP Volume의 Vignette 효과를 설정합니다. Duration이 0이면 즉시 적용, 양수면 SmoothStep으로 부드럽게 변환합니다.")]
[AddComponentMenu("")]
public class SetVignette : Command
{
    [Tooltip("비네트 강도. 0이면 효과 없음, 1이면 화면 가장자리가 완전히 검게 됨. 눈 깜빡임, 의식 흐림 연출에 사용.")]
    [Range(0f, 1f)]
    [SerializeField] private float intensity = 0.5f;

    [Tooltip("비네트 경계 부드러움. 0에 가까울수록 경계가 선명하고, 1에 가까울수록 경계가 부드럽게 퍼짐.")]
    [Range(0f, 1f)]
    [SerializeField] private float smoothness = 0.5f;

    [Tooltip("체크 시 Color 필드를 적용. 미체크 시 색상은 현재 값을 유지한 채 Intensity/Smoothness만 변경.")]
    [SerializeField] private bool overrideColor = false;

    [Tooltip("비네트 색상. 기본값 검정(공포/긴장), 붉은색(공포/위협), 흰색(희망/빛) 등 연출에 따라 변경.")]
    [SerializeField] private Color color = Color.black;

    [Tooltip("변환에 걸리는 시간(초). 0이면 즉시 적용, 양수면 SmoothStep으로 부드럽게 변환.")]
    [SerializeField] private float duration = 0f;

    [Tooltip("체크 시 Duration이 끝날 때까지 플로우차트 진행을 멈춤. 미체크 시 변환 도중에도 다음 커맨드 실행.")]
    [SerializeField] private bool waitUntilFinished = false;

    private Coroutine fadeCoroutine;

    public override void OnEnter()
    {
        Volume volume = Object.FindFirstObjectByType<Volume>();
        if (volume == null || !volume.profile.TryGet(out Vignette vignette))
        {
            Debug.LogWarning("[SetVignette] 씬에서 Vignette 효과를 찾을 수 없습니다.");
            Continue();
            return;
        }

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        if (duration <= 0f)
        {
            Apply(vignette, intensity, smoothness);
            Continue();
        }
        else
        {
            fadeCoroutine = StartCoroutine(Fade(vignette, duration));
            if (!waitUntilFinished) Continue();
        }
    }

    private void Apply(Vignette vignette, float targetIntensity, float targetSmoothness)
    {
        vignette.intensity.overrideState = true;
        vignette.intensity.value = targetIntensity;
        vignette.smoothness.overrideState = true;
        vignette.smoothness.value = targetSmoothness;
        if (overrideColor)
        {
            vignette.color.overrideState = true;
            vignette.color.value = color;
        }
    }

    private IEnumerator Fade(Vignette vignette, float fadeDuration)
    {
        float startIntensity = vignette.intensity.value;
        float startSmoothness = vignette.smoothness.value;
        Color startColor = vignette.color.value;

        vignette.intensity.overrideState = true;
        vignette.smoothness.overrideState = true;
        if (overrideColor) vignette.color.overrideState = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, t / fadeDuration);
            vignette.intensity.value = Mathf.Lerp(startIntensity, intensity, s);
            vignette.smoothness.value = Mathf.Lerp(startSmoothness, smoothness, s);
            if (overrideColor) vignette.color.value = Color.Lerp(startColor, color, s);
            yield return null;
        }

        Apply(vignette, intensity, smoothness);
        fadeCoroutine = null;
        if (waitUntilFinished) Continue();
    }

    public override string GetSummary()
    {
        string durationStr = duration <= 0f ? "즉시" : $"{duration}s";
        return $"Intensity: {intensity}, Smoothness: {smoothness}, Duration: {durationStr}";
    }

    public override Color GetButtonColor()
    {
        return new Color32(106, 159, 181, 255);
    }
}
