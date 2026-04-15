using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.EventSystems;
using Fungus;

public class InGameSettingsPanel : SingletonMonoBehaviour<InGameSettingsPanel>
{
    [System.Obsolete("Use Instance instead.")]
    public static InGameSettingsPanel instance => Instance;

    protected override bool PersistAcrossScenes => true;

    [Header("Fungus 연동")]
    [SerializeField] private Flowchart targetFlowchart;
    [SerializeField] private string fungusVariableName = FungusVariableKeys.IsCalled;
    [SerializeField] private Fungus.DialogInput dialogInput;

    [Header("UI Components")]
    [SerializeField] private GameObject settingPanel;
    public GameObject SettingPanel => settingPanel;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Save / Load — 설정 패널 멀티슬롯")]
    [Tooltip("비우면 씬에서 SaveSlotManager를 자동 탐색합니다.")]
    [SerializeField] SaveSlotManager saveSlotManager;

    [Tooltip("슬롯 N은 배열 인덱스 N-1에 대응합니다. 저장/로드 버튼을 같은 길이로 맞춰 주세요.")]
    [SerializeField] Button[] slotSaveButtons;

    [SerializeField] Button[] slotLoadButtons;

    [Tooltip("선택. 비우거나 짧게 두면 라벨 갱신을 건너뜁니다.")]
    [SerializeField] TMP_Text[] slotInfoLabels;

    [Tooltip("씬에 남은 Fungus SaveMenu(코너 토글) 오브젝트를 끕니다.")]
    [SerializeField] bool disableLegacyFungusSaveMenuInScene = true;

    [Header("Keyboard Navigation")]
    [SerializeField] private Selectable[] navigableElements;
    private int currentIndex = 0;

    [Header("Scene Navigation")]
    [SerializeField] private string mainMenuSceneName = SceneNames.MainMenu;

    const string SettingsCanvasSortingLayerName = "Setting";
    const int SettingsCanvasSortingOrder = 50;

    private ResolutionAudioSettings _resolutionAudio;
    private bool isPanelOpen = false;

    private float playTime = 0f;
    private bool isCounting = true;

    protected override void OnSingletonAwake()
    {
        if (settingPanel != null)
            settingPanel.SetActive(false);

        isPanelOpen = false;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoadedRefreshSaveUi;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedRefreshSaveUi;
    }

    void Start()
    {
        FungusSaveSystemBootstrap.EnsureSaveStack();
        _resolutionAudio = new ResolutionAudioSettings(audioMixer);
        LoadSettings();
        AssignListeners();
        _resolutionAudio.InitializeResolutionDropdown(resolutionDropdown);
        ResolveSaveSlotManager();
        if (disableLegacyFungusSaveMenuInScene)
            DisableLegacyFloatingSaveMenu();
        WireSaveSlotButtons();
        RefreshSaveSlotsUi();
    }

    void Update()
    {
        if (isCounting)
            playTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleSettingPanel();

        if (!isPanelOpen) return;
        HandleKeyboardInput();
    }

    private void OnSceneLoadedRefreshSaveUi(Scene scene, LoadSceneMode mode)
    {
        if (disableLegacyFungusSaveMenuInScene)
            DisableLegacyFloatingSaveMenu();
        RefreshSaveSlotsUi();
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
        bgmSlider.value = _resolutionAudio.GetPersistedBgmLinear();
        sfxSlider.value = _resolutionAudio.GetPersistedSfxLinear();
        fullscreenToggle.isOn = _resolutionAudio.GetPersistedFullscreen();
        _resolutionAudio.ApplyAudioFromLinear(bgmSlider.value, sfxSlider.value);
    }

    private void AssignListeners()
    {
        bgmSlider.onValueChanged.AddListener(SetBgmVolume);
        sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    private void ResolveSaveSlotManager()
    {
        if (saveSlotManager == null)
            saveSlotManager = Object.FindFirstObjectByType<SaveSlotManager>(FindObjectsInactive.Include);
    }

    private static void DisableLegacyFloatingSaveMenu()
    {
        var menus = Object.FindObjectsByType<SaveMenu>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i] != null)
                menus[i].gameObject.SetActive(false);
        }
    }

    private int SlotRowCount()
    {
        int a = slotSaveButtons != null ? slotSaveButtons.Length : 0;
        int b = slotLoadButtons != null ? slotLoadButtons.Length : 0;
        return Mathf.Min(a, b);
    }

    private void WireSaveSlotButtons()
    {
        int n = SlotRowCount();
        for (int i = 0; i < n; i++)
        {
            int slot = i + 1;
            if (slotSaveButtons[i] != null)
            {
                slotSaveButtons[i].onClick.RemoveAllListeners();
                slotSaveButtons[i].onClick.AddListener(() => OnSaveSlotClicked(slot));
            }

            if (slotLoadButtons[i] != null)
            {
                slotLoadButtons[i].onClick.RemoveAllListeners();
                slotLoadButtons[i].onClick.AddListener(() => OnLoadSlotClicked(slot));
            }
        }
    }

    private void OnSaveSlotClicked(int slot)
    {
        ResolveSaveSlotManager();
        if (saveSlotManager == null)
        {
            GameLog.LogWarning("[InGameSettingsPanel] SaveSlotManager 없음");
            return;
        }

        Flowchart fc = FlowchartLocator.Resolve(targetFlowchart);
        if (fc != null)
            fc.SetIntegerVariable("currentSlot", slot);

        saveSlotManager.SaveToSlot(slot);
        RefreshSaveSlotsUi();
    }

    private void OnLoadSlotClicked(int slot)
    {
        ResolveSaveSlotManager();
        if (saveSlotManager == null)
        {
            GameLog.LogWarning("[InGameSettingsPanel] SaveSlotManager 없음");
            return;
        }

        bool sameScene = saveSlotManager.LoadFromSlot(slot);
        RefreshSaveSlotsUi();
        if (sameScene && isPanelOpen)
            ToggleSettingPanel();
    }

    /// <summary>
    /// 슬롯별 로드 가능 여부·라벨을 갱신합니다.
    /// </summary>
    public void RefreshSaveSlotsUi()
    {
        ResolveSaveSlotManager();
        int n = SlotRowCount();
        if (saveSlotManager == null || n == 0)
            return;

        for (int i = 0; i < n; i++)
        {
            int slot = i + 1;
            bool has = saveSlotManager.SlotHasData(slot);

            if (slotLoadButtons != null && i < slotLoadButtons.Length && slotLoadButtons[i] != null)
                slotLoadButtons[i].interactable = has;

            if (slotSaveButtons != null && i < slotSaveButtons.Length && slotSaveButtons[i] != null)
                slotSaveButtons[i].interactable = true;

            if (slotInfoLabels == null || i >= slotInfoLabels.Length || slotInfoLabels[i] == null)
                continue;

            if (!has)
            {
                slotInfoLabels[i].text = "빈 슬롯";
                continue;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            slotInfoLabels[i].text = "저장됨";
#else
            string path = FungusSaveStorage.GetHistoryJsonPath(SaveSlotManager.SlotDataKey(slot));
            if (File.Exists(path))
                slotInfoLabels[i].text = File.GetLastWriteTime(path).ToString("MM/dd HH:mm");
            else
                slotInfoLabels[i].text = "저장됨";
#endif
        }
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

        if (isPanelOpen)
        {
            EnsureSettingsCanvasSortsAboveSayDialog();
            Time.timeScale = 0f;
            RefreshSaveSlotsUi();
            SaveLoadBrowserView browser = GetComponentInChildren<SaveLoadBrowserView>(true);
            if (browser != null)
                browser.EnsureUiBuilt();
        }
        else
            Time.timeScale = 1f;
    }

    void EnsureSettingsCanvasSortsAboveSayDialog()
    {
        if (settingPanel == null)
            return;
        Canvas canvas = settingPanel.GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = settingPanel.GetComponent<Canvas>();
        if (canvas == null)
            return;
        canvas.overrideSorting = true;
        canvas.sortingLayerName = SettingsCanvasSortingLayerName;
        canvas.sortingOrder = SettingsCanvasSortingOrder;
    }

    public void OpenSettingPanel()
    {
        if (!isPanelOpen) ToggleSettingPanel();
    }

    public void CloseSettingPanel()
    {
        if (isPanelOpen) ToggleSettingPanel();
    }

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

    public void SetBgmVolume(float volume)
    {
        _resolutionAudio.SetBgmVolume(volume);
    }

    public void SetSfxVolume(float volume)
    {
        _resolutionAudio.SetSfxVolume(volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        _resolutionAudio.SetFullscreen(isFullscreen);
    }

    public void BackToMainMenu()
    {
        GameLog.Log("메인메뉴 이동 버튼 클릭됨");
        StartCoroutine(GoToMainMenu());
    }

    private IEnumerator GoToMainMenu()
    {
        Time.timeScale = 1f;
        CloseSettingPanel();

        Flowchart fc = FlowchartLocator.Resolve(targetFlowchart);
        if (fc != null)
            fc.SetBooleanVariable(fungusVariableName, false);

        CleanupDontDestroyObjects();
        GameLog.Log("모든 DontDestroyOnLoad 오브젝트 삭제 완료");
        yield return null;

        GameLog.Log($"씬 로드 시도: {mainMenuSceneName}");
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
            if (obj.name == "Variablemanager") continue;
            Destroy(obj);
        }
    }

    public void ReturnToGame()
    {
        CloseSettingPanel();
        GameLog.Log("게임 복귀");
    }
}
