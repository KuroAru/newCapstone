using System.Collections;
using Fungus;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[CommandInfo("Post Processing",
             "Set Bloom",
             "URP Volume의 Bloom 효과 강도를 설정합니다. Duration이 0이면 즉시 적용, 양수면 SmoothStep으로 부드럽게 변환합니다.")]
[AddComponentMenu("")]
public class SetBloom : Command
{
    [Tooltip("대상 URP Volume. 비워 두면 씬에서 자동으로 첫 번째 Volume을 찾습니다.")]
    [SerializeField] private Volume targetVolume;

    [Tooltip("블룸 강도. 0이면 효과 없음, 값이 클수록 밝은 영역의 빛 번짐이 강해짐. 몽환적·신성한 분위기 연출에 적합.")]
    [Min(0f)]
    [SerializeField] private float intensity = 1f;

    [Header("Timing")]
    [Tooltip("변환에 걸리는 시간(초). 0이면 즉시 적용, 양수면 SmoothStep으로 부드럽게 변환.")]
    [SerializeField] private float duration = 0f;

    [Tooltip("체크 시 Duration이 끝날 때까지 플로우차트 진행을 멈춤. 미체크 시 변환 도중에도 다음 커맨드 실행.")]
    [SerializeField] private bool waitUntilFinished = true;

    private Coroutine fadeCoroutine;

    public override void OnEnter()
    {
        Volume volume = targetVolume != null ? targetVolume : Object.FindFirstObjectByType<Volume>();
        if (volume == null || !volume.profile.TryGet(out Bloom bloom))
        {
            GameLog.LogWarning("[SetBloom] 씬에서 Bloom 효과를 찾을 수 없습니다.");
            Continue();
            return;
        }

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        if (duration <= 0f)
        {
            Apply(bloom, intensity);
            Continue();
        }
        else
        {
            fadeCoroutine = StartCoroutine(Fade(bloom, duration));
            if (!waitUntilFinished) Continue();
        }
    }

    public override void OnStopExecuting()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
    }

    private void Apply(Bloom bloom, float targetIntensity)
    {
        bloom.intensity.overrideState = true;
        bloom.intensity.value = targetIntensity;
    }

    private IEnumerator Fade(Bloom bloom, float fadeDuration)
    {
        float startIntensity = bloom.intensity.value;
        bloom.intensity.overrideState = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, t / fadeDuration);
            bloom.intensity.value = Mathf.Lerp(startIntensity, intensity, s);
            yield return null;
        }

        Apply(bloom, intensity);
        fadeCoroutine = null;
        if (waitUntilFinished) Continue();
    }

    public override string GetSummary()
    {
        string durationStr = duration <= 0f ? "즉시" : $"{duration}s";
        return $"Intensity: {intensity}, Duration: {durationStr}";
    }

    public override Color GetButtonColor()
    {
        return new Color32(181, 166, 106, 255);
    }
}
