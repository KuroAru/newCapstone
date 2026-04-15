using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Fungus;

/// <summary>
/// 설정 패널 위에 올리는 저장/불러오기 브라우저(설정·SayDialog와 유사한 밝은 톤). UI는 런타임에 생성됩니다.
/// <see cref="IntegratedSettingUI"/>와 같은 GameObject에 두거나, <see cref="IntegratedSettingUI"/>를 씬에서 찾습니다.
/// </summary>
[DisallowMultipleComponent]
public class SaveLoadBrowserView : MonoBehaviour
{
    [Header("슬롯")]
    [SerializeField] [Range(3, 32)] int slotCount = 12;

    [Header("참조 (비우면 자동 탐색)")]
    [SerializeField] IntegratedSettingUI integratedSettings;
    [SerializeField] SaveSlotManager saveSlotManager;

    GameObject _overlayRoot;

    int _selectedSlot = 1;
    readonly List<SaveRowWidgets> _rows = new List<SaveRowWidgets>();
    SaveSlotRowRenderer _rowRenderer;

    public bool IsOverlayOpen => _overlayRoot != null && _overlayRoot.activeSelf;

    void Start()
    {
        FungusSaveSystemBootstrap.EnsureSaveStack();
        if (saveSlotManager == null)
            saveSlotManager = Object.FindFirstObjectByType<SaveSlotManager>(FindObjectsInactive.Include);
        EnsureUiBuilt();
    }

    /// <summary>
    /// ESC용 <see cref="InGameSettingsPanel"/> 등, 패널이 나중에 켜지는 경우에도 첫 오픈 시 UI를 붙입니다.
    /// </summary>
    public void EnsureUiBuilt()
    {
        if (_overlayRoot != null)
            return;
        FungusSaveSystemBootstrap.EnsureSaveStack();
        if (saveSlotManager == null)
            saveSlotManager = Object.FindFirstObjectByType<SaveSlotManager>(FindObjectsInactive.Include);
        if (!TryResolveSettingsPanelRoot(out RectTransform panelRootRt))
        {
            GameLog.LogWarning("[SaveLoadBrowserView] IntegratedSettingUI.panelRoot 또는 InGameSettingsPanel.SettingPanel 없음 — UI 생성 안 함");
            return;
        }

        SaveBrowserUiBuilder.BuiltUi built = SaveBrowserUiBuilder.BuildUi(
            panelRootRt,
            slotCount,
            OpenOverlay,
            OnClickSave,
            OnClickLoad,
            OnClickDelete,
            CloseOverlay,
            SelectSlot,
            _rows);

        _overlayRoot = built.OverlayRoot;

        _rowRenderer = new SaveSlotRowRenderer(
            _rows,
            built.PreviewTitle,
            built.PreviewSubtitle,
            built.PreviewMeta,
            built.PreviewShot,
            built.BtnSave,
            built.BtnLoad,
            built.BtnDelete)
        {
            SaveSlotManager = saveSlotManager
        };
    }

    bool TryResolveSettingsPanelRoot(out RectTransform panelRootRt)
    {
        panelRootRt = null;
        if (integratedSettings == null)
            integratedSettings = GetComponent<IntegratedSettingUI>();

        if (integratedSettings != null && integratedSettings.panelRoot != null)
        {
            panelRootRt = integratedSettings.panelRoot.GetComponent<RectTransform>();
            return panelRootRt != null;
        }

        InGameSettingsPanel ig = InGameSettingsPanel.instance;
        if (ig != null && ig.SettingPanel != null)
        {
            panelRootRt = ig.SettingPanel.GetComponent<RectTransform>();
            return panelRootRt != null;
        }

        if (integratedSettings == null)
            integratedSettings = Object.FindFirstObjectByType<IntegratedSettingUI>(FindObjectsInactive.Include);
        if (integratedSettings != null && integratedSettings.panelRoot != null)
        {
            panelRootRt = integratedSettings.panelRoot.GetComponent<RectTransform>();
            return panelRootRt != null;
        }

        return false;
    }

    void Update()
    {
        if (!IsOverlayOpen)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            CloseOverlay();
    }

    public void OpenOverlay()
    {
        if (_overlayRoot == null)
            return;
        _overlayRoot.SetActive(true);
        _overlayRoot.transform.SetAsLastSibling();
        RefreshAll();
    }

    public void CloseOverlay()
    {
        if (_overlayRoot != null)
            _overlayRoot.SetActive(false);
    }

    void SelectSlot(int slot)
    {
        _selectedSlot = Mathf.Clamp(slot, 1, slotCount);
        if (_rowRenderer == null)
            return;
        _rowRenderer.SaveSlotManager = saveSlotManager;
        _rowRenderer.RefreshSelectionVisuals(_selectedSlot);
        _rowRenderer.RefreshPreview(_selectedSlot);
        _rowRenderer.RefreshFooter(_selectedSlot);
    }

    void RefreshAll()
    {
        if (_rowRenderer == null)
            return;
        _rowRenderer.SaveSlotManager = saveSlotManager;
        _rowRenderer.RefreshRowTexts();
        SelectSlot(_selectedSlot);
    }

    void OnClickSave()
    {
        if (saveSlotManager == null)
            return;
        Flowchart fc = FlowchartLocator.Find();
        if (fc != null)
            fc.SetIntegerVariable("currentSlot", _selectedSlot);
        saveSlotManager.SaveToSlot(_selectedSlot);
        RefreshAll();
    }

    void OnClickLoad()
    {
        if (saveSlotManager == null || !saveSlotManager.SlotHasData(_selectedSlot))
            return;
        bool sameScene = saveSlotManager.LoadFromSlot(_selectedSlot);
        CloseOverlay();
        if (!sameScene)
            return;
        if (integratedSettings != null && integratedSettings.uiMode == IntegratedSettingUI.UIMode.PopupPanel)
            integratedSettings.ReturnToGame();
        else if (InGameSettingsPanel.instance != null)
            InGameSettingsPanel.instance.CloseSettingPanel();
    }

    void OnClickDelete()
    {
        if (saveSlotManager == null)
            return;
        string key = SaveSlotManager.SlotDataKey(_selectedSlot, saveSlotManager.SlotKeyPrefix);
        SaveManager.Delete(key);
        string thumb = FungusSaveSlotSummary.GetThumbnailPath(_selectedSlot, saveSlotManager.SlotKeyPrefix);
        if (File.Exists(thumb))
            File.Delete(thumb);
        RefreshAll();
    }
}
