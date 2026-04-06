using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fungus;

/// <summary>
/// 설정 패널 위에 올리는 저장/불러오기 브라우저(설정·SayDialog와 유사한 밝은 톤). UI는 런타임에 생성됩니다.
/// <see cref="IntegratedSettingUI"/>와 같은 GameObject에 두거나, <see cref="IntegratedSettingUI"/>를 씬에서 찾습니다.
/// </summary>
[DisallowMultipleComponent]
public class SaveLoadBrowserView : MonoBehaviour
{
    // IntroScene SettingPanel / godlotto SayDialog 톤 (반투명 흰 패널 + 짙은 회색 텍스트)
    static readonly Color ColDimOverlay = new Color(0f, 0f, 0f, 0.38f);
    static readonly Color ColInnerPanel = new Color(1f, 1f, 1f, 0.94f);
    static readonly Color ColFrameBorder = new Color(0.72f, 0.72f, 0.72f, 1f);
    static readonly Color ColRow = new Color(0.9f, 0.9f, 0.9f, 1f);
    static readonly Color ColRowSelected = new Color(0.78f, 0.78f, 0.78f, 1f);
    static readonly Color ColPrimaryText = new Color(0.19607843f, 0.19607843f, 0.19607843f, 1f);
    static readonly Color ColMutedText = new Color(0.45f, 0.45f, 0.45f, 1f);
    static readonly Color ColSaveEntryText = Color.black;
    /// <summary>메인 / 세이브 / 게임 버튼 사이 가로 간격(픽셀).</summary>
    const float SaveEntryButtonGapPx = 10f;

    // 세이브 오버레이 전용 TMP 크기(설정 패널 진입 버튼은 Main Button과 동일하게 유지)
    const float OverlayTitleFontSize = 42f;
    const float OverlayHintFontSize = 20f;
    const float OverlayPreviewTitleSize = 34f;
    const float OverlayPreviewSubtitleSize = 26f;
    const float OverlayPreviewMetaSize = 22f;
    const float OverlayFooterLabelSize = 26f;
    const float OverlaySlotLine1Size = 26f;
    const float OverlaySlotLine2Size = 20f;
    const float OverlayFooterMinHeight = 54f;
    const float OverlaySlotRowMinHeight = 84f;

    [Header("슬롯")]
    [SerializeField] [Range(3, 32)] int slotCount = 12;

    [Header("참조 (비우면 자동 탐색)")]
    [SerializeField] IntegratedSettingUI integratedSettings;
    [SerializeField] SaveSlotManager saveSlotManager;

    GameObject _openEntryButtonRoot;
    GameObject _overlayRoot;
    RectTransform _slotListContent;
    TMP_Text _previewTitle;
    TMP_Text _previewSubtitle;
    TMP_Text _previewMeta;
    Image _previewShot;
    Button _btnSave;
    Button _btnLoad;
    Button _btnDelete;
    Button _btnBack;

    int _selectedSlot = 1;
    readonly List<SaveRowWidgets> _rows = new List<SaveRowWidgets>();

    struct SaveRowWidgets
    {
        public int Slot;
        public Button Button;
        public Image Background;
        public TextMeshProUGUI Line1;
        public TextMeshProUGUI Line2;
    }

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
            Debug.LogWarning("[SaveLoadBrowserView] IntegratedSettingUI.panelRoot 또는 InGameSettingsPanel.settingPanel 없음 — UI 생성 안 함");
            return;
        }
        BuildUi(panelRootRt);
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
        if (ig != null && ig.settingPanel != null)
        {
            panelRootRt = ig.settingPanel.GetComponent<RectTransform>();
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

    void BuildUi(RectTransform panelRoot)
    {
        if (_overlayRoot != null)
            return;

        int uiLayer = panelRoot.gameObject.layer;

        _openEntryButtonRoot = new GameObject("SaveLoadEntryButton", typeof(RectTransform));
        RectTransform entryRt = _openEntryButtonRoot.GetComponent<RectTransform>();
        entryRt.SetParent(panelRoot, false);

        Transform mainBtnTf = FindDescendantByName(panelRoot, "Main Button");
        Transform gameBtnTf = FindDescendantByName(panelRoot, "Game Button");
        RectTransform mainRt = mainBtnTf != null ? mainBtnTf.GetComponent<RectTransform>() : null;
        RectTransform gameRt = gameBtnTf != null ? gameBtnTf.GetComponent<RectTransform>() : null;

        TextMeshProUGUI panelFontSource = mainBtnTf != null
            ? mainBtnTf.GetComponentInChildren<TextMeshProUGUI>(true)
            : null;
        if (panelFontSource == null)
            panelFontSource = panelRoot.GetComponentInChildren<TextMeshProUGUI>(true);

        if (mainRt != null)
        {
            _openEntryButtonRoot.layer = mainBtnTf.gameObject.layer;
            entryRt.anchorMin = mainRt.anchorMin;
            entryRt.anchorMax = mainRt.anchorMax;
            entryRt.pivot = mainRt.pivot;
            entryRt.sizeDelta = mainRt.sizeDelta;
            entryRt.localScale = mainRt.localScale;
            entryRt.localRotation = mainRt.localRotation;

            if (gameRt != null)
            {
                entryRt.anchoredPosition = AnchoredPositionBetweenMainAndGame(entryRt, mainRt, gameRt, SaveEntryButtonGapPx);
                int gameIdx = gameRt.GetSiblingIndex();
                entryRt.SetSiblingIndex(gameIdx);
            }
            else
            {
                entryRt.anchoredPosition = AnchoredPositionAfterRect(entryRt, mainRt, SaveEntryButtonGapPx);
                entryRt.SetSiblingIndex(mainRt.GetSiblingIndex() + 1);
            }

            Button refBtn = mainBtnTf.GetComponent<Button>();
            Image refImg = mainBtnTf.GetComponent<Image>();
            TextMeshProUGUI refTmp = mainBtnTf.GetComponentInChildren<TextMeshProUGUI>(true);

            Button entryBtn = _openEntryButtonRoot.AddComponent<Button>();
            Image entryImg = _openEntryButtonRoot.AddComponent<Image>();
            if (refImg != null)
            {
                entryImg.sprite = refImg.sprite;
                entryImg.type = refImg.type;
                entryImg.preserveAspect = refImg.preserveAspect;
                entryImg.color = refImg.color;
                entryImg.raycastTarget = refImg.raycastTarget;
                entryImg.maskable = refImg.maskable;
                entryImg.pixelsPerUnitMultiplier = refImg.pixelsPerUnitMultiplier;
            }
            else
            {
                entryImg.color = Color.white;
            }

            entryImg.raycastTarget = true;

            if (refBtn != null)
            {
                entryBtn.transition = refBtn.transition;
                entryBtn.colors = refBtn.colors;
                entryBtn.navigation = refBtn.navigation;
            }
            entryBtn.targetGraphic = entryImg;
            entryBtn.interactable = true;
            entryBtn.onClick.AddListener(OpenOverlay);

            GameObject entryLabelGo = new GameObject("Text (TMP)", typeof(RectTransform));
            entryLabelGo.layer = _openEntryButtonRoot.layer;
            RectTransform entryLabelRt = entryLabelGo.GetComponent<RectTransform>();
            entryLabelRt.SetParent(entryRt, false);
            StretchFull(entryLabelRt);
            TextMeshProUGUI entryTmp = entryLabelGo.AddComponent<TextMeshProUGUI>();
            entryTmp.text = "세이브";
            if (refTmp != null)
                CopyTmpStyleFrom(entryTmp, refTmp);
            else
            {
                entryTmp.fontSize = 40;
                entryTmp.alignment = TextAlignmentOptions.Center;
            }
            entryTmp.color = ColSaveEntryText;
            entryTmp.raycastTarget = false;
        }
        else
        {
            _openEntryButtonRoot.layer = uiLayer;
            entryRt.anchorMin = new Vector2(0.5f, 1f);
            entryRt.anchorMax = new Vector2(0.5f, 1f);
            entryRt.pivot = new Vector2(0.5f, 1f);
            entryRt.anchoredPosition = new Vector2(0f, -24f);
            entryRt.sizeDelta = new Vector2(420f, 52f);

            Button entryBtn = _openEntryButtonRoot.AddComponent<Button>();
            Image entryImg = _openEntryButtonRoot.AddComponent<Image>();
            entryImg.color = ColRow;
            entryImg.raycastTarget = true;
            entryBtn.targetGraphic = entryImg;
            entryBtn.interactable = true;
            entryBtn.onClick.AddListener(OpenOverlay);

            GameObject entryLabelGo = new GameObject("Text", typeof(RectTransform));
            entryLabelGo.layer = uiLayer;
            RectTransform entryLabelRt = entryLabelGo.GetComponent<RectTransform>();
            entryLabelRt.SetParent(entryRt, false);
            StretchFull(entryLabelRt);
            TextMeshProUGUI entryTmp = entryLabelGo.AddComponent<TextMeshProUGUI>();
            entryTmp.text = "세이브";
            entryTmp.fontSize = 26;
            entryTmp.alignment = TextAlignmentOptions.Center;
            entryTmp.color = ColSaveEntryText;
            entryTmp.raycastTarget = false;
            CopyTmpFontAndMaterialsFrom(entryTmp, panelFontSource);
        }

        _overlayRoot = new GameObject("SaveLoadBrowserOverlay", typeof(RectTransform));
        _overlayRoot.layer = uiLayer;
        RectTransform overlayRt = _overlayRoot.GetComponent<RectTransform>();
        overlayRt.SetParent(panelRoot, false);
        StretchFull(overlayRt);
        _overlayRoot.transform.SetAsLastSibling();

        Image dim = _overlayRoot.AddComponent<Image>();
        dim.color = ColDimOverlay;
        dim.raycastTarget = true;

        GameObject frame = new GameObject("Frame", typeof(RectTransform), typeof(Image));
        frame.layer = uiLayer;
        RectTransform frameRt = frame.GetComponent<RectTransform>();
        frameRt.SetParent(overlayRt, false);
        frameRt.anchorMin = new Vector2(0.04f, 0.06f);
        frameRt.anchorMax = new Vector2(0.96f, 0.94f);
        frameRt.offsetMin = Vector2.zero;
        frameRt.offsetMax = Vector2.zero;
        frame.GetComponent<Image>().color = ColFrameBorder;

        GameObject inner = new GameObject("Inner", typeof(RectTransform), typeof(Image));
        inner.layer = uiLayer;
        RectTransform innerRt = inner.GetComponent<RectTransform>();
        innerRt.SetParent(frameRt, false);
        innerRt.anchorMin = new Vector2(0.003f, 0.008f);
        innerRt.anchorMax = new Vector2(0.997f, 0.992f);
        innerRt.offsetMin = Vector2.zero;
        innerRt.offsetMax = Vector2.zero;
        inner.GetComponent<Image>().color = ColInnerPanel;

        GameObject titleGo = new GameObject("Title", typeof(RectTransform));
        titleGo.layer = uiLayer;
        RectTransform titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.SetParent(innerRt, false);
        titleRt.anchorMin = new Vector2(0f, 0.88f);
        titleRt.anchorMax = new Vector2(1f, 0.98f);
        titleRt.offsetMin = new Vector2(24f, 0f);
        titleRt.offsetMax = new Vector2(-24f, -4f);
        TextMeshProUGUI titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "저장 / 불러오기";
        titleTmp.fontSize = OverlayTitleFontSize;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.color = ColPrimaryText;
        titleTmp.alignment = TextAlignmentOptions.Left;
        titleTmp.raycastTarget = false;
        CopyTmpFontAndMaterialsFrom(titleTmp, panelFontSource);

        GameObject body = new GameObject("Body", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        body.layer = uiLayer;
        RectTransform bodyRt = body.GetComponent<RectTransform>();
        bodyRt.SetParent(innerRt, false);
        bodyRt.anchorMin = new Vector2(0f, 0.17f);
        bodyRt.anchorMax = new Vector2(1f, 0.86f);
        bodyRt.offsetMin = new Vector2(20f, 8f);
        bodyRt.offsetMax = new Vector2(-20f, -8f);
        HorizontalLayoutGroup bodyH = body.GetComponent<HorizontalLayoutGroup>();
        bodyH.spacing = 16f;
        bodyH.childForceExpandWidth = true;
        bodyH.childForceExpandHeight = true;
        bodyH.childControlWidth = true;
        bodyH.childControlHeight = true;

        GameObject leftCol = new GameObject("SlotColumn", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        leftCol.layer = uiLayer;
        RectTransform leftRt = leftCol.GetComponent<RectTransform>();
        leftRt.SetParent(bodyRt, false);
        leftCol.GetComponent<Image>().color = ColRow;
        LayoutElement leftLe = leftCol.GetComponent<LayoutElement>();
        leftLe.flexibleWidth = 0.38f;
        leftLe.minWidth = 280f;

        GameObject scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollGo.layer = uiLayer;
        RectTransform scrollRt = scrollGo.GetComponent<RectTransform>();
        scrollRt.SetParent(leftRt, false);
        StretchFull(scrollRt);
        scrollGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.06f);
        ScrollRect sr = scrollGo.GetComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.scrollSensitivity = 40f;

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        viewport.layer = uiLayer;
        RectTransform vpRt = viewport.GetComponent<RectTransform>();
        vpRt.SetParent(scrollRt, false);
        StretchFull(vpRt);
        viewport.GetComponent<Image>().color = new Color(0, 0, 0, 0.02f);

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.layer = uiLayer;
        _slotListContent = content.GetComponent<RectTransform>();
        _slotListContent.SetParent(vpRt, false);
        _slotListContent.anchorMin = new Vector2(0f, 1f);
        _slotListContent.anchorMax = new Vector2(1f, 1f);
        _slotListContent.pivot = new Vector2(0.5f, 1f);
        _slotListContent.anchoredPosition = Vector2.zero;
        _slotListContent.sizeDelta = new Vector2(0f, 0f);
        VerticalLayoutGroup vg = content.GetComponent<VerticalLayoutGroup>();
        vg.spacing = 8f;
        vg.padding = new RectOffset(8, 8, 8, 8);
        vg.childControlHeight = true;
        vg.childControlWidth = true;
        vg.childForceExpandWidth = true;
        ContentSizeFitter fit = content.GetComponent<ContentSizeFitter>();
        fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.viewport = vpRt;
        sr.content = _slotListContent;

        GameObject rightCol = new GameObject("DetailColumn", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        rightCol.layer = uiLayer;
        RectTransform rightRt = rightCol.GetComponent<RectTransform>();
        rightRt.SetParent(bodyRt, false);
        rightCol.GetComponent<Image>().color = ColRow;
        LayoutElement rightLe = rightCol.GetComponent<LayoutElement>();
        rightLe.flexibleWidth = 0.62f;
        VerticalLayoutGroup rightV = rightCol.GetComponent<VerticalLayoutGroup>();
        rightV.spacing = 14f;
        rightV.padding = new RectOffset(16, 16, 16, 16);
        rightV.childControlHeight = true;
        rightV.childControlWidth = true;
        rightV.childForceExpandWidth = true;

        GameObject shotGo = new GameObject("Screenshot", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter), typeof(LayoutElement));
        shotGo.layer = uiLayer;
        RectTransform shotRt = shotGo.GetComponent<RectTransform>();
        shotRt.SetParent(rightRt, false);
        _previewShot = shotGo.GetComponent<Image>();
        _previewShot.color = new Color(0.88f, 0.88f, 0.88f, 1f);
        AspectRatioFitter arf = shotGo.GetComponent<AspectRatioFitter>();
        arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        arf.aspectRatio = 16f / 9f;
        LayoutElement shotLe = shotGo.GetComponent<LayoutElement>();
        shotLe.preferredHeight = 220f;
        shotLe.flexibleHeight = 1f;

        _previewTitle = CreateTmpLine(rightRt, "DetailTitle", OverlayPreviewTitleSize, ColPrimaryText, FontStyles.Bold, uiLayer, panelFontSource);
        _previewSubtitle = CreateTmpLine(rightRt, "DetailSubtitle", OverlayPreviewSubtitleSize, ColPrimaryText, FontStyles.Normal, uiLayer, panelFontSource);
        _previewMeta = CreateTmpLine(rightRt, "DetailMeta", OverlayPreviewMetaSize, ColMutedText, FontStyles.Normal, uiLayer, panelFontSource);

        // 힌트를 푸터보다 먼저 자식으로 두면, 겹치는 영역에서도 푸터 버튼이 레이캐스트 우선이 됩니다.
        GameObject hint = new GameObject("Hint", typeof(RectTransform));
        hint.layer = uiLayer;
        RectTransform hintRt = hint.GetComponent<RectTransform>();
        hintRt.SetParent(innerRt, false);
        hintRt.anchorMin = new Vector2(0f, 0f);
        hintRt.anchorMax = new Vector2(1f, 0.075f);
        hintRt.offsetMin = new Vector2(24f, 4f);
        hintRt.offsetMax = new Vector2(-24f, 4f);
        TextMeshProUGUI hintTmp = hint.AddComponent<TextMeshProUGUI>();
        hintTmp.text = "선택 (클릭) · (ESC) 뒤로";
        hintTmp.fontSize = OverlayHintFontSize;
        hintTmp.color = ColMutedText;
        hintTmp.alignment = TextAlignmentOptions.Left;
        hintTmp.raycastTarget = false;
        CopyTmpFontAndMaterialsFrom(hintTmp, panelFontSource);

        GameObject footer = new GameObject("Footer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        footer.layer = uiLayer;
        RectTransform footRt = footer.GetComponent<RectTransform>();
        footRt.SetParent(innerRt, false);
        footRt.anchorMin = new Vector2(0.04f, 0.055f);
        footRt.anchorMax = new Vector2(0.96f, 0.155f);
        footRt.offsetMin = Vector2.zero;
        footRt.offsetMax = Vector2.zero;
        HorizontalLayoutGroup footH = footer.GetComponent<HorizontalLayoutGroup>();
        footH.spacing = 12f;
        footH.childAlignment = TextAnchor.MiddleCenter;
        footH.childForceExpandHeight = true;
        footH.childForceExpandWidth = true;
        footH.childControlHeight = true;
        footH.childControlWidth = true;

        _btnSave = CreateFooterButton(footRt, "저장", OnClickSave, uiLayer, panelFontSource);
        _btnLoad = CreateFooterButton(footRt, "불러오기", OnClickLoad, uiLayer, panelFontSource);
        _btnDelete = CreateFooterButton(footRt, "삭제", OnClickDelete, uiLayer, panelFontSource);
        _btnBack = CreateFooterButton(footRt, "뒤로", CloseOverlay, uiLayer, panelFontSource);

        BuildSlotRows(uiLayer, panelFontSource);
        _overlayRoot.SetActive(false);
    }

    static TMP_Text CreateTmpLine(Transform parent, string name, float size, Color c, FontStyles style, int layer, TextMeshProUGUI fontSource)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = layer;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.minHeight = size + 14f;
        le.flexibleWidth = 1f;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = size;
        tmp.color = c;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        CopyTmpFontAndMaterialsFrom(tmp, fontSource);
        return tmp;
    }

    static Button CreateFooterButton(RectTransform footRt, string label, UnityEngine.Events.UnityAction onClick, int layer, TextMeshProUGUI fontSource)
    {
        GameObject go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        go.layer = layer;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(footRt, false);
        Image img = go.GetComponent<Image>();
        img.color = ColRowSelected;
        img.raycastTarget = true;
        LayoutElement le = go.GetComponent<LayoutElement>();
        le.minHeight = OverlayFooterMinHeight;
        le.flexibleWidth = 1f;
        Button b = go.AddComponent<Button>();
        b.targetGraphic = img;
        b.interactable = true;
        b.onClick.AddListener(onClick);

        GameObject tgo = new GameObject("Txt", typeof(RectTransform));
        tgo.layer = layer;
        RectTransform trt = tgo.GetComponent<RectTransform>();
        trt.SetParent(rt, false);
        StretchFull(trt);
        TextMeshProUGUI tmp = tgo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = OverlayFooterLabelSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = ColPrimaryText;
        tmp.raycastTarget = false;
        CopyTmpFontAndMaterialsFrom(tmp, fontSource);
        return b;
    }

    void BuildSlotRows(int layer, TextMeshProUGUI fontSource)
    {
        _rows.Clear();
        for (int i = 1; i <= slotCount; i++)
        {
            int slot = i;
            GameObject row = new GameObject("Slot_" + slot, typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(Button));
            row.layer = layer;
            RectTransform rt = row.GetComponent<RectTransform>();
            rt.SetParent(_slotListContent, false);
            Image bg = row.GetComponent<Image>();
            bg.color = ColRow;
            LayoutElement le = row.GetComponent<LayoutElement>();
            le.minHeight = OverlaySlotRowMinHeight;
            Button btn = row.GetComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => SelectSlot(slot));

            GameObject t1 = new GameObject("L1", typeof(RectTransform));
            t1.layer = layer;
            RectTransform t1r = t1.GetComponent<RectTransform>();
            t1r.SetParent(rt, false);
            t1r.anchorMin = new Vector2(0.04f, 0.5f);
            t1r.anchorMax = new Vector2(0.96f, 1f);
            t1r.offsetMin = Vector2.zero;
            t1r.offsetMax = Vector2.zero;
            TextMeshProUGUI tmp1 = t1.AddComponent<TextMeshProUGUI>();
            tmp1.fontSize = OverlaySlotLine1Size;
            tmp1.color = ColPrimaryText;
            tmp1.text = "슬롯 " + slot;
            tmp1.raycastTarget = false;
            CopyTmpFontAndMaterialsFrom(tmp1, fontSource);

            GameObject t2 = new GameObject("L2", typeof(RectTransform));
            t2.layer = layer;
            RectTransform t2r = t2.GetComponent<RectTransform>();
            t2r.SetParent(rt, false);
            t2r.anchorMin = new Vector2(0.04f, 0f);
            t2r.anchorMax = new Vector2(0.96f, 0.5f);
            t2r.offsetMin = Vector2.zero;
            t2r.offsetMax = Vector2.zero;
            TextMeshProUGUI tmp2 = t2.AddComponent<TextMeshProUGUI>();
            tmp2.fontSize = OverlaySlotLine2Size;
            tmp2.color = ColMutedText;
            tmp2.raycastTarget = false;
            CopyTmpFontAndMaterialsFrom(tmp2, fontSource);

            _rows.Add(new SaveRowWidgets
            {
                Slot = slot,
                Button = btn,
                Background = bg,
                Line1 = tmp1,
                Line2 = tmp2
            });
        }
    }

    static Transform FindDescendantByName(Transform root, string objectName)
    {
        if (root == null || string.IsNullOrEmpty(objectName))
            return null;
        if (root.name == objectName)
            return root;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDescendantByName(root.GetChild(i), objectName);
            if (found != null)
                return found;
        }
        return null;
    }

    static void CopyTmpFontAndMaterialsFrom(TextMeshProUGUI dst, TextMeshProUGUI src)
    {
        if (dst == null || src == null)
            return;
        dst.font = src.font;
        dst.fontSharedMaterials = src.fontSharedMaterials;
    }

    static void CopyTmpStyleFrom(TextMeshProUGUI dst, TextMeshProUGUI src)
    {
        if (dst == null || src == null)
            return;
        dst.font = src.font;
        dst.fontSharedMaterials = src.fontSharedMaterials;
        dst.fontSize = src.fontSize;
        dst.fontWeight = src.fontWeight;
        dst.fontStyle = src.fontStyle;
        dst.color = src.color;
        dst.alignment = src.alignment;
        dst.enableAutoSizing = src.enableAutoSizing;
        dst.fontSizeMin = src.fontSizeMin;
        dst.fontSizeMax = src.fontSizeMax;
        dst.characterSpacing = src.characterSpacing;
        dst.wordSpacing = src.wordSpacing;
        dst.lineSpacing = src.lineSpacing;
    }

    /// <summary>
    /// main 오른쪽 + gap, game 왼쪽 − gap 사이에서 세이브 버튼을 가로 중앙 정렬합니다.
    /// </summary>
    static Vector2 AnchoredPositionBetweenMainAndGame(RectTransform entry, RectTransform main, RectTransform game, float gapPx)
    {
        float mainRight = main.anchoredPosition.x + main.sizeDelta.x * (1f - main.pivot.x);
        float gameLeft = game.anchoredPosition.x - game.sizeDelta.x * game.pivot.x;
        float innerL = mainRight + gapPx;
        float innerR = gameLeft - gapPx;
        float w = entry.sizeDelta.x;
        float span = innerR - innerL;
        float leftEdge = span >= w ? innerL + (span - w) * 0.5f : innerL;
        float x = leftEdge + w * entry.pivot.x;
        return new Vector2(x, main.anchoredPosition.y);
    }

    static Vector2 AnchoredPositionAfterRect(RectTransform entry, RectTransform main, float gapPx)
    {
        float mainRight = main.anchoredPosition.x + main.sizeDelta.x * (1f - main.pivot.x);
        float leftEdge = mainRight + gapPx;
        float x = leftEdge + entry.sizeDelta.x * entry.pivot.x;
        return new Vector2(x, main.anchoredPosition.y);
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
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
        RefreshSelectionVisuals();
        RefreshPreview();
        RefreshFooter();
    }

    void RefreshAll()
    {
        RefreshRowTexts();
        SelectSlot(_selectedSlot);
    }

    void RefreshRowTexts()
    {
        string prefix = saveSlotManager != null ? saveSlotManager.SlotKeyPrefix : FungusSaveStorage.DefaultSlotKeyPrefix;
        foreach (SaveRowWidgets w in _rows)
        {
            bool has = saveSlotManager != null && saveSlotManager.SlotHasData(w.Slot);
            if (!has)
            {
                w.Line1.text = $"슬롯 {w.Slot} — 빈 슬롯";
                w.Line2.text = "";
                continue;
            }

            FungusSaveSlotSummary.TryReadSlotSummary(w.Slot, prefix, out string scene, out string desc);
            w.Line1.text = string.IsNullOrEmpty(scene) ? $"슬롯 {w.Slot}" : scene;
            w.Line2.text = string.IsNullOrEmpty(desc) ? "저장됨" : desc;
        }
    }

    void RefreshSelectionVisuals()
    {
        foreach (SaveRowWidgets w in _rows)
            w.Background.color = w.Slot == _selectedSlot ? ColRowSelected : ColRow;
    }

    void RefreshPreview()
    {
        string prefix = saveSlotManager != null ? saveSlotManager.SlotKeyPrefix : FungusSaveStorage.DefaultSlotKeyPrefix;
        bool has = saveSlotManager != null && saveSlotManager.SlotHasData(_selectedSlot);
        if (!has)
        {
            _previewTitle.text = $"슬롯 {_selectedSlot}";
            _previewSubtitle.text = "빈 슬롯";
            _previewMeta.text = "이 슬롯에 저장하면 현재 진행 상황이 기록됩니다.";
            ClearPreviewImage();
            return;
        }

        FungusSaveSlotSummary.TryReadSlotSummary(_selectedSlot, prefix, out string scene, out string desc);
        _previewTitle.text = string.IsNullOrEmpty(scene) ? $"슬롯 {_selectedSlot}" : scene;
        _previewSubtitle.text = string.IsNullOrEmpty(desc) ? "" : desc;
        _previewMeta.text = Application.productName;

        string thumbPath = FungusSaveSlotSummary.GetThumbnailPath(_selectedSlot, prefix);
        if (File.Exists(thumbPath))
        {
            byte[] bytes = File.ReadAllBytes(thumbPath);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
            if (tex.LoadImage(bytes))
            {
                if (_previewShot.sprite != null)
                {
                    Destroy(_previewShot.sprite.texture);
                    Destroy(_previewShot.sprite);
                }
                _previewShot.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                _previewShot.color = Color.white;
            }
        }
        else
            ClearPreviewImage();
    }

    void ClearPreviewImage()
    {
        if (_previewShot.sprite != null)
        {
            Destroy(_previewShot.sprite.texture);
            Destroy(_previewShot.sprite);
            _previewShot.sprite = null;
        }
        _previewShot.color = new Color(0.88f, 0.88f, 0.88f, 1f);
    }

    void RefreshFooter()
    {
        if (saveSlotManager == null)
            saveSlotManager = Object.FindFirstObjectByType<SaveSlotManager>(FindObjectsInactive.Include);

        bool has = saveSlotManager != null && saveSlotManager.SlotHasData(_selectedSlot);
        _btnLoad.interactable = has;
        _btnDelete.interactable = has;
        bool canSave = saveSlotManager != null && saveSlotManager.GetResolvedSaveManager() != null;
        _btnSave.interactable = canSave;
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
