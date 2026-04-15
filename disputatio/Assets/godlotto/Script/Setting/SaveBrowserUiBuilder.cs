using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 저장/불러오기 브라우저 오버레이의 런타임 UI 생성.
/// </summary>
public static class SaveBrowserUiBuilder
{
    // IntroScene SettingPanel / godlotto SayDialog 톤 (반투명 흰 패널 + 짙은 회색 텍스트)
    public static readonly Color ColDimOverlay = new Color(0f, 0f, 0f, 0.38f);
    public static readonly Color ColInnerPanel = new Color(1f, 1f, 1f, 0.94f);
    public static readonly Color ColFrameBorder = new Color(0.72f, 0.72f, 0.72f, 1f);
    public static readonly Color ColRow = new Color(0.9f, 0.9f, 0.9f, 1f);
    public static readonly Color ColRowSelected = new Color(0.78f, 0.78f, 0.78f, 1f);
    public static readonly Color ColPrimaryText = new Color(0.19607843f, 0.19607843f, 0.19607843f, 1f);
    public static readonly Color ColMutedText = new Color(0.45f, 0.45f, 0.45f, 1f);
    public static readonly Color ColSaveEntryText = Color.black;

    /// <summary>메인 / 세이브 / 게임 버튼 사이 가로 간격(픽셀).</summary>
    public const float SaveEntryButtonGapPx = 10f;

    public const float OverlayTitleFontSize = 42f;
    public const float OverlayHintFontSize = 20f;
    public const float OverlayPreviewTitleSize = 34f;
    public const float OverlayPreviewSubtitleSize = 26f;
    public const float OverlayPreviewMetaSize = 22f;
    public const float OverlayFooterLabelSize = 26f;
    public const float OverlaySlotLine1Size = 26f;
    public const float OverlaySlotLine2Size = 20f;
    public const float OverlayFooterMinHeight = 54f;
    public const float OverlaySlotRowMinHeight = 84f;

    public sealed class BuiltUi
    {
        public GameObject OpenEntryButtonRoot;
        public GameObject OverlayRoot;
        public RectTransform SlotListContent;
        public TMP_Text PreviewTitle;
        public TMP_Text PreviewSubtitle;
        public TMP_Text PreviewMeta;
        public Image PreviewShot;
        public Button BtnSave;
        public Button BtnLoad;
        public Button BtnDelete;
        public Button BtnBack;
    }

    public static BuiltUi BuildUi(
        RectTransform panelRoot,
        int slotCount,
        UnityAction openOverlay,
        UnityAction onSave,
        UnityAction onLoad,
        UnityAction onDelete,
        UnityAction onBack,
        Action<int> onSlotSelected,
        List<SaveRowWidgets> rows)
    {
        int uiLayer = panelRoot.gameObject.layer;

        BuiltUi result = new BuiltUi();

        result.OpenEntryButtonRoot = new GameObject("SaveLoadEntryButton", typeof(RectTransform));
        RectTransform entryRt = result.OpenEntryButtonRoot.GetComponent<RectTransform>();
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
            result.OpenEntryButtonRoot.layer = mainBtnTf.gameObject.layer;
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

            Button entryBtn = result.OpenEntryButtonRoot.AddComponent<Button>();
            Image entryImg = result.OpenEntryButtonRoot.AddComponent<Image>();
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
            entryBtn.onClick.AddListener(openOverlay);

            GameObject entryLabelGo = new GameObject("Text (TMP)", typeof(RectTransform));
            entryLabelGo.layer = result.OpenEntryButtonRoot.layer;
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
            result.OpenEntryButtonRoot.layer = uiLayer;
            entryRt.anchorMin = new Vector2(0.5f, 1f);
            entryRt.anchorMax = new Vector2(0.5f, 1f);
            entryRt.pivot = new Vector2(0.5f, 1f);
            entryRt.anchoredPosition = new Vector2(0f, -24f);
            entryRt.sizeDelta = new Vector2(420f, 52f);

            Button entryBtn = result.OpenEntryButtonRoot.AddComponent<Button>();
            Image entryImg = result.OpenEntryButtonRoot.AddComponent<Image>();
            entryImg.color = ColRow;
            entryImg.raycastTarget = true;
            entryBtn.targetGraphic = entryImg;
            entryBtn.interactable = true;
            entryBtn.onClick.AddListener(openOverlay);

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

        result.OverlayRoot = new GameObject("SaveLoadBrowserOverlay", typeof(RectTransform));
        result.OverlayRoot.layer = uiLayer;
        RectTransform overlayRt = result.OverlayRoot.GetComponent<RectTransform>();
        overlayRt.SetParent(panelRoot, false);
        StretchFull(overlayRt);
        result.OverlayRoot.transform.SetAsLastSibling();

        Image dim = result.OverlayRoot.AddComponent<Image>();
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
        result.SlotListContent = content.GetComponent<RectTransform>();
        result.SlotListContent.SetParent(vpRt, false);
        result.SlotListContent.anchorMin = new Vector2(0f, 1f);
        result.SlotListContent.anchorMax = new Vector2(1f, 1f);
        result.SlotListContent.pivot = new Vector2(0.5f, 1f);
        result.SlotListContent.anchoredPosition = Vector2.zero;
        result.SlotListContent.sizeDelta = new Vector2(0f, 0f);
        VerticalLayoutGroup vg = content.GetComponent<VerticalLayoutGroup>();
        vg.spacing = 8f;
        vg.padding = new RectOffset(8, 8, 8, 8);
        vg.childControlHeight = true;
        vg.childControlWidth = true;
        vg.childForceExpandWidth = true;
        ContentSizeFitter fit = content.GetComponent<ContentSizeFitter>();
        fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.viewport = vpRt;
        sr.content = result.SlotListContent;

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
        result.PreviewShot = shotGo.GetComponent<Image>();
        result.PreviewShot.color = new Color(0.88f, 0.88f, 0.88f, 1f);
        AspectRatioFitter arf = shotGo.GetComponent<AspectRatioFitter>();
        arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        arf.aspectRatio = 16f / 9f;
        LayoutElement shotLe = shotGo.GetComponent<LayoutElement>();
        shotLe.preferredHeight = 220f;
        shotLe.flexibleHeight = 1f;

        result.PreviewTitle = CreateTmpLine(rightRt, "DetailTitle", OverlayPreviewTitleSize, ColPrimaryText, FontStyles.Bold, uiLayer, panelFontSource);
        result.PreviewSubtitle = CreateTmpLine(rightRt, "DetailSubtitle", OverlayPreviewSubtitleSize, ColPrimaryText, FontStyles.Normal, uiLayer, panelFontSource);
        result.PreviewMeta = CreateTmpLine(rightRt, "DetailMeta", OverlayPreviewMetaSize, ColMutedText, FontStyles.Normal, uiLayer, panelFontSource);

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

        result.BtnSave = CreateFooterButton(footRt, "저장", onSave, uiLayer, panelFontSource);
        result.BtnLoad = CreateFooterButton(footRt, "불러오기", onLoad, uiLayer, panelFontSource);
        result.BtnDelete = CreateFooterButton(footRt, "삭제", onDelete, uiLayer, panelFontSource);
        result.BtnBack = CreateFooterButton(footRt, "뒤로", onBack, uiLayer, panelFontSource);

        BuildSlotRows(result.SlotListContent, slotCount, uiLayer, panelFontSource, onSlotSelected, rows);
        result.OverlayRoot.SetActive(false);

        return result;
    }

    static void BuildSlotRows(
        RectTransform slotListContent,
        int slotCount,
        int layer,
        TextMeshProUGUI fontSource,
        Action<int> onSlotSelected,
        List<SaveRowWidgets> rows)
    {
        rows.Clear();
        for (int i = 1; i <= slotCount; i++)
        {
            int slot = i;
            GameObject row = new GameObject("Slot_" + slot, typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(Button));
            row.layer = layer;
            RectTransform rt = row.GetComponent<RectTransform>();
            rt.SetParent(slotListContent, false);
            Image bg = row.GetComponent<Image>();
            bg.color = ColRow;
            LayoutElement le = row.GetComponent<LayoutElement>();
            le.minHeight = OverlaySlotRowMinHeight;
            Button btn = row.GetComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => onSlotSelected(slot));

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

            rows.Add(new SaveRowWidgets
            {
                Slot = slot,
                Button = btn,
                Background = bg,
                Line1 = tmp1,
                Line2 = tmp2
            });
        }
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

    static Button CreateFooterButton(RectTransform footRt, string label, UnityAction onClick, int layer, TextMeshProUGUI fontSource)
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

    public static void CopyTmpFontAndMaterialsFrom(TextMeshProUGUI dst, TextMeshProUGUI src)
    {
        if (dst == null || src == null)
            return;
        dst.font = src.font;
        dst.fontSharedMaterials = src.fontSharedMaterials;
    }

    public static void CopyTmpStyleFrom(TextMeshProUGUI dst, TextMeshProUGUI src)
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
}
