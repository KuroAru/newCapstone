using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 슬라이더 등 0~1 선형 볼륨을 AudioMixer Exposed Parameter(dB)로 변환합니다.
/// </summary>
public static class AudioMixerVolumeUtility
{
    private const float SilentDecibels = -80f;
    private const float DecibelsPerDecade = 20f;

    public static float Linear01ToDecibels(float linear01)
    {
        if (linear01 <= 0f)
            return SilentDecibels;
        return Mathf.Log10(linear01) * DecibelsPerDecade;
    }

    public static void SetExposedVolume(AudioMixer mixer, string exposedParameterName, float linear01)
    {
        if (mixer == null || string.IsNullOrEmpty(exposedParameterName))
            return;
        mixer.SetFloat(exposedParameterName, Linear01ToDecibels(linear01));
    }
}
