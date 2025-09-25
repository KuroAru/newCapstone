using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SettingSceneManager : MonoBehaviour
{
    [Header("UI Components")]
    public AudioMixer audioMixer;
    public Slider bgmSlider;
    public Slider sfxSlider;
    public TextMeshProUGUI resolutionText;
    public Toggle fullscreenToggle;

    [Header("Scene Navigation")]
    public string mainMenuSceneName = "MainMenuScene";

    private List<Resolution> resolutions;
    private int currentResolutionIndex = 0;

    void Start()
    {
        // UI 요소들에 저장된 PlayerPrefs 값을 불러와 적용
        LoadSettings();
        // 리스너(버튼 클릭, 슬라이더 조작 등의 이벤트) 연결
        AssignListeners();
        // 해상도 관련 UI 초기화
        InitializeResolution();
    }

    private void LoadSettings()
    {
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.75f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        SetBgmVolume(bgmSlider.value);
        SetSfxVolume(sfxSlider.value);
    }

    private void AssignListeners()
    {
        bgmSlider.onValueChanged.AddListener(SetBgmVolume);
        sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    // --- 설정값 적용 함수 ---
    public void SetBgmVolume(float volume)
    {
        audioMixer.SetFloat("BGMVolume", volume == 0 ? -80 : Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("BGMVolume", volume);
    }

    public void SetSfxVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume == 0 ? -80 : Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    // --- 해상도 관련 함수 ---
    private void InitializeResolution()
    {
        resolutions = new List<Resolution>(Screen.resolutions);
        resolutions.RemoveAll(res => res.refreshRateRatio.value < 60);
        resolutions.Sort((a, b) => (a.width.CompareTo(b.width) * 1000) + a.height.CompareTo(b.height));

        currentResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", resolutions.Count - 1);
        if (currentResolutionIndex >= resolutions.Count)
        {
            currentResolutionIndex = resolutions.Count - 1;
        }
        
        SetResolution(currentResolutionIndex);
    }

    public void CycleResolutionForward()
    {
        CycleResolution(1);
    }

    public void CycleResolutionBackward()
    {
        CycleResolution(-1);
    }

    private void CycleResolution(int direction)
    {
        currentResolutionIndex += direction;
        if (currentResolutionIndex >= resolutions.Count) currentResolutionIndex = 0;
        if (currentResolutionIndex < 0) currentResolutionIndex = resolutions.Count - 1;
        SetResolution(currentResolutionIndex);
    }

    private void SetResolution(int index)
    {
        currentResolutionIndex = index;
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", currentResolutionIndex);
        UpdateResolutionText();
    }

    private void UpdateResolutionText()
    {
        if (resolutionText != null)
            resolutionText.text = resolutions[currentResolutionIndex].width + " x " + resolutions[currentResolutionIndex].height;
    }

    // --- 씬 이동 ---
    public void BackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}