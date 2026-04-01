using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.EventSystems;
public class SettingManager : MonoBehaviour
{
    [Header("UI Components")]
    public AudioMixer audioMixer;
    public Slider bgmSlider;
    public Slider sfxSlider;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    [Header("Keyboard Navigation")]
    public Selectable[] navigableElements;
    private int currentIndex = 0;

    [Header("Scene Navigation")]
    public string mainMenuSceneName = "MainMenuScene";

    private List<Resolution> resolutions;
    private int currentResolutionIndex = 0;

    void Start()
    {
        LoadSettings();
        AssignListeners();
        InitializeResolutionDropdown();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SelectUIElement(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BackToMainMenuViaEsc();
            return;
        }

        HandleKeyboardInput();
    }

    private void LoadSettings()
    {
        bgmSlider.value = PlayerPrefs.GetFloat(SettingPlayerPrefsKeys.BgmVolume, SettingPlayerPrefsKeys.DefaultLinearVolume);
        sfxSlider.value = PlayerPrefs.GetFloat(SettingPlayerPrefsKeys.SfxVolume, SettingPlayerPrefsKeys.DefaultLinearVolume);
        fullscreenToggle.isOn = PlayerPrefs.GetInt(SettingPlayerPrefsKeys.Fullscreen, SettingPlayerPrefsKeys.FullscreenDefaultEnabled) == 1;

        SetBgmVolume(bgmSlider.value);
        SetSfxVolume(sfxSlider.value);
    }

    private void AssignListeners()
    {
        bgmSlider.onValueChanged.AddListener(SetBgmVolume);
        sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    public void SetBgmVolume(float volume)
    {
        AudioMixerVolumeUtility.SetExposedVolume(audioMixer, SettingPlayerPrefsKeys.BgmVolume, volume);
        PlayerPrefs.SetFloat(SettingPlayerPrefsKeys.BgmVolume, volume);
    }

    public void SetSfxVolume(float volume)
    {
        AudioMixerVolumeUtility.SetExposedVolume(audioMixer, SettingPlayerPrefsKeys.SfxVolume, volume);
        PlayerPrefs.SetFloat(SettingPlayerPrefsKeys.SfxVolume, volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(SettingPlayerPrefsKeys.Fullscreen, isFullscreen ? 1 : 0);
    }

    private void InitializeResolutionDropdown()
    {
        resolutions = ResolutionListUtility.BuildPreferredResolutionList();
        resolutionDropdown.ClearOptions();
        List<string> options = ResolutionListUtility.BuildLabels(resolutions);
        int defaultResolutionIndex = 0;
        for (int i = 0; i < resolutions.Count; i++)
        {
            var r = resolutions[i];
            if (r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height)
                defaultResolutionIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        currentResolutionIndex = PlayerPrefs.GetInt(SettingPlayerPrefsKeys.ResolutionIndex, defaultResolutionIndex);
        
        // 저장된 값이 유효하지 않을 경우(예: 모니터 변경) 기본값으로 리셋
        if (currentResolutionIndex >= resolutions.Count || currentResolutionIndex < 0)
        {
            currentResolutionIndex = defaultResolutionIndex;
        }

        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownChanged);
        SetResolution(currentResolutionIndex);
    }

    // 드롭다운 값이 변경될 때 호출되는 함수
    private void OnResolutionDropdownChanged(int index)
    {
        SetResolution(index);
    }

    // 해상도를 적용하고 저장하는 함수
    private void SetResolution(int index)
    {
        // 유효한 인덱스인지 확인
        if (index < 0 || index >= resolutions.Count)
        {
            return;
        }

        currentResolutionIndex = index;
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt(SettingPlayerPrefsKeys.ResolutionIndex, currentResolutionIndex);
    }

    public void BackToMainMenu()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void BackToMainMenuViaEsc()
    {
        BackToMainMenu();
    }

    // --- 이하 키보드 조작 처리 ---
    private void HandleKeyboardInput()
    {
        bool isKeyboardInput = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
                                 Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
                                 Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);

        if (isKeyboardInput && EventSystem.current.currentSelectedGameObject == null)
            SelectUIElement(currentIndex);

        if (EventSystem.current.currentSelectedGameObject == null) return;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            HandleNavigation();
        else
            HandleSelectionKeyboardInput();
    }

    private void HandleNavigation()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentIndex++;
            if (currentIndex >= navigableElements.Length) currentIndex = 0;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = navigableElements.Length - 1;
        }
        SelectUIElement(currentIndex);
    }

    private void HandleSelectionKeyboardInput()
    {
        GameObject selectedObj = EventSystem.current.currentSelectedGameObject;
        if (selectedObj == null) return;

        if (selectedObj == bgmSlider.gameObject)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow)) bgmSlider.value += 0.1f;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) bgmSlider.value -= 0.1f;
        }
        else if (selectedObj == sfxSlider.gameObject)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow)) sfxSlider.value += 0.1f;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) sfxSlider.value -= 0.1f;
        }
    }

    private void SelectUIElement(int index)
    {
        if (navigableElements.Length > 0 && index >= 0 && index < navigableElements.Length)
        {
            EventSystem.current.SetSelectedGameObject(navigableElements[index].gameObject);
            currentIndex = index;
        }
    }
}