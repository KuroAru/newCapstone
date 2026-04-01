using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Fungus;

public class InGameSettingsPanel : MonoBehaviour
{
    public static InGameSettingsPanel instance;

    [Header("Fungus 연동")]
    public Flowchart targetFlowchart;
    public string fungusVariableName = "isCalled";
    public Fungus.DialogInput dialogInput;

    [Header("UI Components")]
    public GameObject settingPanel;
    public AudioMixer audioMixer;
    public Slider bgmSlider;
    public Slider sfxSlider;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    [Header("Save / Load (설정 패널)")]
    [Tooltip("비우면 씬에서 SaveSlotManager를 자동 탐색합니다.")]
    [SerializeField] SaveSlotManager saveSlotManager;
    [SerializeField] Button saveGameButton;
    [SerializeField] Button loadGameButton;
    [Tooltip("켜면 Save 프리팹의 코너 토글은 숨기고, ESC 설정이 열릴 때 세이브 UI를 설정 패널 쪽에 표시합니다.")]
    [SerializeField] bool hideFungusSaveMenuHud = true;

    [Tooltip("비우면 settingPanel의 RectTransform 아래에 세이브 UI를 붙입니다. 전용 빈 오브젝트(레이아웃 앵커)를 두어도 됩니다.")]
    [SerializeField] RectTransform saveMenuHostUnderSettings;

    [Header("Keyboard Navigation")]
    public Selectable[] navigableElements;
    private int currentIndex = 0;

    [Header("Scene Navigation")]
    public string mainMenuSceneName = "MainMenuScene";

    private List<Resolution> resolutions;
    private int currentResolutionIndex = 0;
    private bool isPanelOpen = false;

    // ✅ 플레이 시간 관련 변수
    private float playTime = 0f;       
    private bool isCounting = true;

    Transform saveUiLiftOriginalParent;
    int saveUiLiftOriginalSiblingIndex;
    RectTransform saveUiLiftTarget;
    bool saveUiLiftHasCache;

    Transform saveOpenerLiftOriginalParent;
    int saveOpenerLiftOriginalSiblingIndex;
    RectTransform saveOpenerLiftTarget;
    bool saveOpenerLiftHasCache;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        if (settingPanel != null)
            settingPanel.SetActive(false);

        isPanelOpen = false;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoadedForSaveHud;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedForSaveHud;
    }

    void Start()
    {
        LoadSettings();
        AssignListeners(); // 볼륨, 전체화면 리스너 등록
        InitializeResolutionDropdown();
        WireSaveLoadButtons();
        TryHideFungusSaveMenuHud();
    }

    void Update()
    {
        if (isCounting)
            playTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingPanel();
        }

        if (!isPanelOpen) return;
        HandleKeyboardInput();
    }

    private void OnSceneLoadedForSaveHud(Scene scene, LoadSceneMode mode)
    {
        TryHideFungusSaveMenuHud();
    }

    public float GetPlayTime() => playTime;

    public void StopCounting()
    {
        isCounting = false;
    }

    public void ResetPlayTime()
    {
        playTime = 0f;
    }

    private void LoadSettings()
    {
        bgmSlider.value = PlayerPrefs.GetFloat(SettingPlayerPrefsKeys.BgmVolume, SettingPlayerPrefsKeys.DefaultLinearVolume);
        sfxSlider.value = PlayerPrefs.GetFloat(SettingPlayerPrefsKeys.SfxVolume, SettingPlayerPrefsKeys.DefaultLinearVolume);
        
        // ★ 중요: 전체화면 여부도 PlayerPrefs를 맹신하지 않고 현재 상태를 반영하는 것이 안전합니다.
        // 하지만 여기선 PlayerPrefs와 현재 상태를 동기화합니다.
        bool isFull = Screen.fullScreen;
        fullscreenToggle.isOn = isFull; 
        
        SetBgmVolume(bgmSlider.value);
        SetSfxVolume(sfxSlider.value);
    }

    private void AssignListeners()
    {
        bgmSlider.onValueChanged.AddListener(SetBgmVolume);
        sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    private void WireSaveLoadButtons()
    {
        if (saveGameButton != null)
        {
            saveGameButton.onClick.RemoveListener(OnSaveGameClicked);
            saveGameButton.onClick.AddListener(OnSaveGameClicked);
        }

        if (loadGameButton != null)
        {
            loadGameButton.onClick.RemoveListener(OnLoadGameClicked);
            loadGameButton.onClick.AddListener(OnLoadGameClicked);
        }
    }

    private void ResolveSaveSlotManager()
    {
        if (saveSlotManager == null)
            saveSlotManager = FindObjectOfType<SaveSlotManager>(true);
    }

    private SaveMenu ResolveSaveMenu()
    {
        ResolveSaveSlotManager();
        if (saveSlotManager != null && saveSlotManager.saveMenu != null)
            return saveSlotManager.saveMenu;
        return FindObjectOfType<SaveMenu>(true);
    }

    private RectTransform ResolveSaveMenuHost()
    {
        if (saveMenuHostUnderSettings != null)
            return saveMenuHostUnderSettings;
        return settingPanel != null ? settingPanel.transform as RectTransform : null;
    }

    private static void StretchSaveUiToHost(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    private void LiftSaveUiIntoSettingsHost()
    {
        var host = ResolveSaveMenuHost();
        if (host == null)
            return;

        ResolveSaveSlotManager();
        var menu = ResolveSaveMenu();

        if (saveSlotManager != null && saveSlotManager.saveUiReparentRoot != null)
        {
            var target = saveSlotManager.saveUiReparentRoot;
            if (!saveUiLiftHasCache)
            {
                saveUiLiftOriginalParent = target.parent;
                saveUiLiftOriginalSiblingIndex = target.GetSiblingIndex();
                saveUiLiftTarget = target;
                saveUiLiftHasCache = true;
            }

            target.SetParent(host, false);
            StretchSaveUiToHost(target);
            target.gameObject.SetActive(true);
            LiftSavePanelOpenerIntoSettingsHost(host);
            return;
        }

        if (menu != null)
            menu.TryReparentSaveMenuGroupTo(host);

        LiftSavePanelOpenerIntoSettingsHost(host);
    }

    /// <summary>
    /// Save.prefab 등 세이브 패널 오픈 전용 플로팅 버튼을 설정 패널 호스트로 옮깁니다 (전체 스트레치 없음).
    /// </summary>
    private void LiftSavePanelOpenerIntoSettingsHost(RectTransform host)
    {
        if (host == null)
            return;

        ResolveSaveSlotManager();
        if (saveSlotManager == null || saveSlotManager.savePanelOpenerRoot == null)
            return;

        var target = saveSlotManager.savePanelOpenerRoot;
        if (!saveOpenerLiftHasCache)
        {
            saveOpenerLiftOriginalParent = target.parent;
            saveOpenerLiftOriginalSiblingIndex = target.GetSiblingIndex();
            saveOpenerLiftTarget = target;
            saveOpenerLiftHasCache = true;
        }

        target.SetParent(host, false);
        target.SetAsLastSibling();
        target.gameObject.SetActive(true);
    }

    private void ReturnSaveUiFromSettingsHost()
    {
        ReturnSavePanelOpenerFromSettingsHost();

        if (saveUiLiftHasCache && saveUiLiftTarget != null)
        {
            saveUiLiftTarget.SetParent(saveUiLiftOriginalParent, false);
            if (saveUiLiftOriginalParent != null)
            {
                int idx = Mathf.Clamp(saveUiLiftOriginalSiblingIndex, 0, saveUiLiftOriginalParent.childCount - 1);
                saveUiLiftTarget.SetSiblingIndex(idx);
            }

            saveUiLiftTarget.gameObject.SetActive(false);
            saveUiLiftHasCache = false;
            saveUiLiftTarget = null;
            saveUiLiftOriginalParent = null;
        }

        var menu = ResolveSaveMenu();
        menu?.RestoreSaveMenuGroupParentIfReparented();
    }

    private void ReturnSavePanelOpenerFromSettingsHost()
    {
        if (!saveOpenerLiftHasCache || saveOpenerLiftTarget == null)
            return;

        saveOpenerLiftTarget.SetParent(saveOpenerLiftOriginalParent, false);
        if (saveOpenerLiftOriginalParent != null)
        {
            int idx = Mathf.Clamp(saveOpenerLiftOriginalSiblingIndex, 0, saveOpenerLiftOriginalParent.childCount - 1);
            saveOpenerLiftTarget.SetSiblingIndex(idx);
        }

        saveOpenerLiftTarget.gameObject.SetActive(false);
        saveOpenerLiftHasCache = false;
        saveOpenerLiftTarget = null;
        saveOpenerLiftOriginalParent = null;
    }

    private void TryHideFungusSaveMenuHud()
    {
        if (!hideFungusSaveMenuHud)
            return;

        ResolveSaveSlotManager();
        if (saveSlotManager != null && saveSlotManager.saveUiReparentRoot != null)
            saveSlotManager.saveUiReparentRoot.gameObject.SetActive(false);

        if (saveSlotManager != null && saveSlotManager.savePanelOpenerRoot != null)
            saveSlotManager.savePanelOpenerRoot.gameObject.SetActive(false);

        var menu = ResolveSaveMenu();
        if (menu != null)
            menu.HideInGameHud();
    }

    private void SyncSaveMenuWithSettingsPanelOpen(bool open)
    {
        if (!hideFungusSaveMenuHud)
            return;

        var menu = ResolveSaveMenu();

        if (open)
        {
            if (saveSlotManager != null)
                saveSlotManager.EnsureSlotKeyApplied();

            LiftSaveUiIntoSettingsHost();

            if (menu != null)
                menu.ShowSaveMenuPanelForSettings();
        }
        else
        {
            if (menu != null)
                menu.HideSaveMenuPanelForSettings();

            ReturnSaveUiFromSettingsHost();
        }
    }

    /// <summary>
    /// Fungus SaveManager 상태에 맞춰 설정 패널의 세이브/로드 버튼 interactable을 갱신합니다.
    /// </summary>
    private void RefreshSaveLoadInteractable()
    {
        if (saveGameButton == null && loadGameButton == null)
            return;

        ResolveSaveSlotManager();
        if (saveSlotManager != null)
            saveSlotManager.EnsureSlotKeyApplied();

        var fungusSave = FungusManager.Instance != null ? FungusManager.Instance.SaveManager : null;
        var menu = saveSlotManager != null ? saveSlotManager.saveMenu : null;

        if (saveGameButton != null)
            saveGameButton.interactable = fungusSave != null && fungusSave.NumSavePoints > 0;

        if (loadGameButton != null)
        {
            bool canLoad = fungusSave != null && menu != null && fungusSave.SaveDataExists(menu.SaveDataKey);
            loadGameButton.interactable = canLoad;
        }
    }

    public void OnSaveGameClicked()
    {
        ResolveSaveSlotManager();
        if (saveSlotManager == null)
        {
            Debug.LogWarning("[InGameSettingsPanel] SaveSlotManager 없음 — 세이브 불가");
            return;
        }

        saveSlotManager.Save();
        RefreshSaveLoadInteractable();
    }

    public void OnLoadGameClicked()
    {
        ResolveSaveSlotManager();
        if (saveSlotManager == null)
        {
            Debug.LogWarning("[InGameSettingsPanel] SaveSlotManager 없음 — 로드 불가");
            return;
        }

        saveSlotManager.Load();
        RefreshSaveLoadInteractable();
    }

    public void ToggleSettingPanel()
    {
        isPanelOpen = !isPanelOpen;
        settingPanel.SetActive(isPanelOpen);

        if (dialogInput != null)
            dialogInput.enabled = !isPanelOpen;

        Flowchart fc = FlowchartLocator.Resolve(targetFlowchart);
        if (fc != null)
            fc.SetBooleanVariable(fungusVariableName, isPanelOpen);

        SyncSaveMenuWithSettingsPanelOpen(isPanelOpen);

        if (isPanelOpen)
            RefreshSaveLoadInteractable();
    }

    public void OpenSettingPanel()
    {
        if (!isPanelOpen) ToggleSettingPanel();
    }

    public void CloseSettingPanel()
    {
        if (isPanelOpen) ToggleSettingPanel();
    }

    // --- 키보드 입력 처리 생략 (기존과 동일) ---
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
            HandleEnterPress();
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

    private void HandleEnterPress()
    {
        GameObject selectedObj = EventSystem.current.currentSelectedGameObject;
        if (selectedObj == null) return;

        Button button = selectedObj.GetComponent<Button>();
        if (button != null)
            button.onClick.Invoke();
    }

    private void SelectUIElement(int index)
    {
        if (navigableElements.Length > 0 && index >= 0 && index < navigableElements.Length)
        {
            EventSystem.current.SetSelectedGameObject(navigableElements[index].gameObject);
            currentIndex = index;
        }
    }
    // ----------------------------------------

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
        int currentScreenIndex = 0;
        for (int i = 0; i < resolutions.Count; i++)
        {
            var r = resolutions[i];
            if (r.width == Screen.width && r.height == Screen.height)
                currentScreenIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        currentResolutionIndex = currentScreenIndex;
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownChanged);
    }

    private void OnResolutionDropdownChanged(int index)
    {
        SetResolution(index);
    }

    private void SetResolution(int index)
    {
        if (index < 0 || index >= resolutions.Count) return;
        currentResolutionIndex = index;
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        
        // 인게임에서 바꿀 때만 PlayerPrefs에 저장
        PlayerPrefs.SetInt(SettingPlayerPrefsKeys.ResolutionIndex, currentResolutionIndex);
    }

    public void BackToMainMenu()
    {
        Debug.Log("메인메뉴 이동 버튼 클릭됨");
        StartCoroutine(GoToMainMenu());
    }

    private IEnumerator GoToMainMenu()
    {
        Time.timeScale = 1f;
        Flowchart fc = FlowchartLocator.Resolve(targetFlowchart);
        if (fc != null)
            fc.SetBooleanVariable(fungusVariableName, false);

        CleanupDontDestroyObjects();
        Debug.Log("모든 DontDestroyOnLoad 오브젝트 삭제 완료");
        yield return null;

        Debug.Log($"씬 로드 시도: {mainMenuSceneName}");
        SceneManager.LoadScene(mainMenuSceneName);

        Destroy(gameObject);
    }

    private void CleanupDontDestroyObjects()
    {
        var temp = new GameObject("TempSceneProbe");
        DontDestroyOnLoad(temp);
        var ddScene = temp.scene;
        Destroy(temp);

        var roots = new List<GameObject>();
        ddScene.GetRootGameObjects(roots);

        foreach (var obj in roots)
        {
            if (obj == gameObject) continue;
            if (obj.GetComponent<GlobalVariables>() != null) continue;
            if (obj.name == "Variablemanager") continue; // 오타 수정 (VariableManager 일 수 있음)
            Destroy(obj);
        }
    }

    public void ReturnToGame()
    {
        CloseSettingPanel();
        Debug.Log("게임 복귀");
    }
}