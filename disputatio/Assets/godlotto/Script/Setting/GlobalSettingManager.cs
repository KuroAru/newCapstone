using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class GlobalSettingManager : MonoBehaviour
{
    public static GlobalSettingManager Instance;

    [Header("Core Components")]
    [Tooltip("Exposed BGM/SFX 파라미터가 있는 AudioMixer")]
    public AudioMixer audioMixer;

    // 현재 설정값들을 메모리에 들고 있음 (씬이 바뀌어도 유지됨)
    public float bgmVolume { get; private set; }
    public float sfxVolume { get; private set; }
    public int currentResolutionIndex { get; private set; }
    public bool isFullscreen { get; private set; }

    // 해상도 목록 캐싱
    public List<Resolution> availableResolutions { get; private set; }
    private List<string> resolutionOptions; // UI 표시용 문자열

    void Awake()
    {
        // 싱글톤 패턴: 나 자신을 보존하고 중복된 놈은 파괴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSettings(); // 최초 1회 초기화
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSettings()
    {
        // 1. 해상도 목록 생성
        GenerateResolutionList();

        // 2. 저장된 값 불러오기 (없으면 기본값)
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.75f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        
        // 해상도 인덱스는 저장된 값이 유효한지 체크
        int savedResIndex = PlayerPrefs.GetInt("ResolutionIndex", -1);
        
        // 저장된게 없으면 현재 화면 해상도를 찾음
        if (savedResIndex == -1)
        {
            currentResolutionIndex = FindCurrentResolutionIndex();
        }
        else
        {
            currentResolutionIndex = Mathf.Clamp(savedResIndex, 0, availableResolutions.Count - 1);
        }

        // 3. 실제 적용 (AudioMixer, Screen)
        ApplyAudioSettings();
        ApplyGraphicSettings();
    }

    private void GenerateResolutionList()
    {
        availableResolutions = ResolutionListUtility.BuildPreferredResolutionList();
        resolutionOptions = ResolutionListUtility.BuildLabels(availableResolutions);
    }

    // 현재 화면과 일치하는 해상도 인덱스 찾기
    public int FindCurrentResolutionIndex()
    {
        for (int i = 0; i < availableResolutions.Count; i++)
        {
            if (availableResolutions[i].width == Screen.width && 
                availableResolutions[i].height == Screen.height)
                return i;
        }
        return 0; // 못 찾으면 0번
    }

    public List<string> GetResolutionOptions() => resolutionOptions;

    // --- 설정 변경 및 적용 메서드 ---

    public void SetBGM(float value)
    {
        bgmVolume = value;
        ApplyAudioSettings();
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
    }

    public void SetSFX(float value)
    {
        sfxVolume = value;
        ApplyAudioSettings();
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    public void SetResolutionIndex(int index)
    {
        if (index < 0 || index >= availableResolutions.Count) return;
        currentResolutionIndex = index;
        ApplyGraphicSettings();
        PlayerPrefs.SetInt("ResolutionIndex", currentResolutionIndex);
    }

    public void SetFullscreen(bool isFull)
    {
        isFullscreen = isFull;
        ApplyGraphicSettings();
        PlayerPrefs.SetInt("Fullscreen", isFull ? 1 : 0);
    }

    private void ApplyAudioSettings()
    {
        AudioMixerVolumeUtility.SetExposedVolume(audioMixer, "BGMVolume", bgmVolume);
        AudioMixerVolumeUtility.SetExposedVolume(audioMixer, "SFXVolume", sfxVolume);
    }

    private void ApplyGraphicSettings()
    {
        Resolution r = availableResolutions[currentResolutionIndex];
        // 중복 적용 방지 (화면 깜빡임 최소화)
        if (Screen.width != r.width || Screen.height != r.height || Screen.fullScreen != isFullscreen)
        {
            Screen.SetResolution(r.width, r.height, isFullscreen);
        }
    }
}