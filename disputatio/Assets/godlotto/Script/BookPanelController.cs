using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BookPanelController : MonoBehaviour
{
    // ── 인벤토리 북 기준 기본값 ─────────────────────────────────────
    // 스프레드 전체 기준(0~1):
    //   좌 페이지 내부: x 0.04~0.47  →  중앙 x≈0.255
    //   우 페이지 내부: x 0.53~0.96  →  중앙 x≈0.745
    //   상하 여백 제외 y: 0.10~0.88
    // ──────────────────────────────────────────────────────────────

    [Serializable]
    public class PageLayoutSettings
    {
        [Header("음식 일러스트 (좌 페이지 중앙)")]
        [Tooltip("정규화 앵커 고정점. 피벗은 항상 (0.5, 0.5).")]
        public Vector2 illustrationAnchor   = new Vector2(0.255f, 0.63f);
        [Tooltip("이미지 크기(픽셀). SizeDelta로 적용됩니다.")]
        public Vector2 illustrationSize     = new Vector2(260f, 175f);
        [Tooltip("앵커 기준 추가 오프셋.")]
        public Vector2 illustrationOffset   = Vector2.zero;

        [Header("레시피 텍스트 (좌 페이지 하단)")]
        public Vector2 recipeAnchorMin  = new Vector2(0.06f, 0.12f);
        public Vector2 recipeAnchorMax  = new Vector2(0.44f, 0.50f);
        [Tooltip("TMP margin (Left, Top, Right, Bottom).")]
        public Vector4 recipeMargin     = new Vector4(8f, 4f, 8f, 6f);
        public float   recipeFontSize   = 26f;

        [Header("메모 텍스트 (우 페이지 전체)")]
        public Vector2 memoAnchorMin    = new Vector2(0.55f, 0.12f);
        public Vector2 memoAnchorMax    = new Vector2(0.92f, 0.82f);
        [Tooltip("TMP margin (Left, Top, Right, Bottom).")]
        public Vector4 memoMargin       = new Vector4(12f, 14f, 12f, 14f);
        public float   memoFontSize     = 28f;
    }

    [Tooltip("요리책(가정부 스크랩북) 전용 레이아웃·자동 리소스 로드에 사용합니다.")]
    [SerializeField] private bool isCookbookPanel;

    [Header("페이지 오브젝트 목록")]
    public GameObject[] pages;

    [Header("페이지별 레이아웃 (비우면 글로벌 기본값 사용)")]
    [Tooltip("pages[] 배열과 같은 인덱스로 맞춥니다. 부족하면 마지막 항목을 재사용합니다.")]
    [SerializeField] private PageLayoutSettings[] pageLayouts;

    [Header("글로벌 기본 레이아웃 (pageLayouts 미지정 시 사용)")]
    [SerializeField] private PageLayoutSettings defaultLayout = new PageLayoutSettings();

    [Header("가정부 방 스크랩북 (선택)")]
    [Tooltip("비어 있으면 CookBook_Panel에서 Resources/MaidRoomCookbookScrapbook 를 자동 로드")]
    [SerializeField] private TextAsset scrapbookContentJson;
    [Tooltip("오른쪽 페이지 — 가정부 메모")]
    [SerializeField] private TextMeshProUGUI scrapbookPageTextOverlay;
    [Tooltip("왼쪽 페이지 하단 — 레시피 본문")]
    [SerializeField] private TextMeshProUGUI scrapbookRecipeTextOverlay;

    public const string RecipeIllustrationChildName = "RecipeIllustration";

    [Header("페이지별 배경 (선택)")]
    [SerializeField] private Sprite[] pageBackgroundSprites;
    [Header("왼쪽 레시피 일러 (선택, 자식 RecipeIllustration)")]
    [SerializeField] private Sprite[] pageRecipeIllustrationSprites;

    [Header("페이지 넘기기 애니메이션")]
    [Tooltip("넘기기 프레임 순서 (InventoryBook_01 → 02 → 03 → 04). 비우면 애니메이션 없음.")]
    [SerializeField] private Sprite[] turnForwardFrames;
    [Tooltip("애니메이션 오버레이 Image. 책 배경 위에 올라갈 Image 컴포넌트를 연결하세요.")]
    [SerializeField] private Image turnAnimationImage;
    [Tooltip("프레임 하나당 표시 시간 (초). 낮을수록 빠릅니다.")]
    [SerializeField] private float frameDuration = 0.07f;
    [Tooltip("페이지 내용이 교체되는 프레임 인덱스 (0부터). 보통 절반 지점.")]
    [SerializeField] private int contentSwapFrame = 2;

    private int currentPageIndex = 0;
    private bool _isTurning = false;
    private string PREF_KEY;
    private CookbookPagePair[] _cookbookSplitPages;
    private string[] _scrapbookLegacyBodies;

    [Serializable]
    private class CookbookPagePair { public string recipe; public string memo; }
    [Serializable]
    private class CookbookSplitRoot { public CookbookPagePair[] pages; }
    [Serializable]
    private class ScrapbookPagesJson { public string[] pages; }

    void Awake()
    {
        if (turnAnimationImage != null)
            turnAnimationImage.raycastTarget = false;

        PREF_KEY = "LastBookPage_" + gameObject.name;
        if (scrapbookContentJson == null && isCookbookPanel)
            scrapbookContentJson = Resources.Load<TextAsset>("MaidRoomCookbookScrapbook");
        if (scrapbookPageTextOverlay == null)
        {
            var t = transform.Find("CookBookScrapText");
            if (t != null) scrapbookPageTextOverlay = t.GetComponent<TextMeshProUGUI>();
        }
        DeactivateOrphanPageChildren();
        TryLoadScrapbookJson();
        TryAssignRecipeSpritesFromResources();
        TryAssignPageBackgroundSpritesFromResources();
        ApplyPageBackgroundSprites();
        ApplyRecipeIllustrationSprites();

        // 초기 전체 레이아웃 적용 (페이지 0 기준)
        ApplyLayoutForPage(0);

        if (isCookbookPanel)
        {
            if (_cookbookSplitPages != null && _cookbookSplitPages.Length > 0)
                EnsureCookBookRecipeTextUi();
        }
    }

    // ── 페이지별 레이아웃 ─────────────────────────────────────────

    private PageLayoutSettings GetLayout(int pageIndex)
    {
        if (pageLayouts != null && pageLayouts.Length > 0)
        {
            int idx = Mathf.Clamp(pageIndex, 0, pageLayouts.Length - 1);
            if (pageLayouts[idx] != null) return pageLayouts[idx];
        }
        return defaultLayout ?? new PageLayoutSettings();
    }

    /// <summary>지정 페이지의 레이아웃(일러스트·레시피·메모 앵커)을 즉시 적용합니다.</summary>
    private void ApplyLayoutForPage(int pageIndex)
    {
        if (pages == null) return;
        var layout = GetLayout(pageIndex);

        // 일러스트 — 해당 페이지 오브젝트에만 적용
        if (pageIndex >= 0 && pageIndex < pages.Length && pages[pageIndex] != null)
            ApplyIllustrationLayout(pages[pageIndex], layout);

        // 레시피·메모 텍스트 오버레이 — 전체 공유 오브젝트에 적용
        ApplyRecipeTextLayout(layout);
        ApplyMemoTextLayout(layout);
    }

    private void ApplyIllustrationLayout(GameObject page, PageLayoutSettings layout)
    {
        var tr = page.transform.Find(RecipeIllustrationChildName) as RectTransform;
        if (tr == null) return;
        tr.anchorMin        = layout.illustrationAnchor;
        tr.anchorMax        = layout.illustrationAnchor;
        tr.pivot            = new Vector2(0.5f, 0.5f);
        tr.anchoredPosition = layout.illustrationOffset;
        tr.sizeDelta        = layout.illustrationSize;
    }

    private void ApplyRecipeTextLayout(PageLayoutSettings layout)
    {
        if (scrapbookRecipeTextOverlay == null) return;
        var rt = scrapbookRecipeTextOverlay.rectTransform;
        rt.anchorMin        = layout.recipeAnchorMin;
        rt.anchorMax        = layout.recipeAnchorMax;
        rt.pivot            = new Vector2(0.5f, 1f);
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        var tmp = scrapbookRecipeTextOverlay;
        tmp.fontSize              = layout.recipeFontSize;
        tmp.margin                = layout.recipeMargin;
        tmp.textWrappingMode      = TextWrappingModes.Normal;
        tmp.overflowMode          = TextOverflowModes.Overflow;
        tmp.wordWrappingRatios    = 0.5f;
        tmp.horizontalAlignment   = HorizontalAlignmentOptions.Center;
        tmp.verticalAlignment     = VerticalAlignmentOptions.Top;
        tmp.lineSpacing           = 2f;
        tmp.paragraphSpacing      = 8f;
    }

    private void ApplyMemoTextLayout(PageLayoutSettings layout)
    {
        if (scrapbookPageTextOverlay == null) return;
        var rt = scrapbookPageTextOverlay.rectTransform;
        rt.anchorMin        = layout.memoAnchorMin;
        rt.anchorMax        = layout.memoAnchorMax;
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        var tmp = scrapbookPageTextOverlay;
        tmp.fontSize              = layout.memoFontSize;
        tmp.margin                = layout.memoMargin;
        tmp.textWrappingMode      = TextWrappingModes.Normal;
        tmp.overflowMode          = TextOverflowModes.Overflow;
        tmp.wordWrappingRatios    = 0.5f;
        tmp.horizontalAlignment   = HorizontalAlignmentOptions.Center;
        tmp.verticalAlignment     = VerticalAlignmentOptions.Middle;
    }

    // ── 페이지 이동 ──────────────────────────────────────────────

    void OnEnable()
    {
        currentPageIndex = PlayerPrefs.GetInt(PREF_KEY, 0);
        ShowPage(currentPageIndex);
    }

    public void NextPage()
    {
        if (_isTurning || currentPageIndex >= pages.Length - 1) return;
        int next = currentPageIndex + 1;
        StartCoroutine(TurnPage(next, forward: true));
    }

    public void PreviousPage()
    {
        if (_isTurning || currentPageIndex <= 0) return;
        int next = currentPageIndex - 1;
        StartCoroutine(TurnPage(next, forward: false));
    }

    private IEnumerator TurnPage(int nextIndex, bool forward)
    {
        _isTurning = true;

        // 애니메이션 스프라이트가 없으면 즉시 전환
        if (turnForwardFrames == null || turnForwardFrames.Length == 0 || turnAnimationImage == null)
        {
            currentPageIndex = nextIndex;
            ShowPageImmediate(currentPageIndex);
            SaveCurrentPage();
            _isTurning = false;
            yield break;
        }

        // 앞방향: 01→02→03→04, 뒷방향: 04→03→02→01
        int frameCount = turnForwardFrames.Length;
        turnAnimationImage.gameObject.SetActive(true);
        var col = turnAnimationImage.color;
        col.a = 1f;
        turnAnimationImage.color = col;
        bool contentSwapped = false;

        for (int i = 0; i < frameCount; i++)
        {
            int fi = forward ? i : (frameCount - 1 - i);
            var sprite = turnForwardFrames[fi];
            if (sprite != null) turnAnimationImage.sprite = sprite;

            // 지정 프레임에서 페이지 내용 교체 (오버레이 뒤에서 조용히)
            if (!contentSwapped && i >= contentSwapFrame)
            {
                currentPageIndex = nextIndex;
                ShowPageImmediate(currentPageIndex);
                contentSwapped = true;
            }

            yield return new WaitForSeconds(frameDuration);
        }

        // 혹시 교체가 안 됐으면 마지막에 교체
        if (!contentSwapped)
        {
            currentPageIndex = nextIndex;
            ShowPageImmediate(currentPageIndex);
        }

        col = turnAnimationImage.color;
        col.a = 0f;
        turnAnimationImage.color = col;
        turnAnimationImage.gameObject.SetActive(false);
        SaveCurrentPage();
        _isTurning = false;
    }

    private void ShowPageImmediate(int index)
    {
        index = Mathf.Clamp(index, 0, pages.Length - 1);
        for (int i = 0; i < pages.Length; i++)
            pages[i].SetActive(i == index);
        ApplyLayoutForPage(index);
        UpdateScrapbookOverlay(index);
    }

    // 기존 ShowPage는 OnEnable 전용으로 유지
    private void ShowPage(int index) => ShowPageImmediate(index);

    // ── 스크랩북 텍스트 ───────────────────────────────────────────

    private void UpdateScrapbookOverlay(int index)
    {
        if (scrapbookPageTextOverlay == null) return;
        if (isCookbookPanel && _cookbookSplitPages != null && _cookbookSplitPages.Length > 0)
        {
            EnsureCookBookRecipeTextUi();
            string memo   = string.Empty;
            string recipe = string.Empty;
            if (index >= 0 && index < _cookbookSplitPages.Length)
            {
                memo   = _cookbookSplitPages[index].memo   ?? string.Empty;
                recipe = _cookbookSplitPages[index].recipe ?? string.Empty;
            }
            scrapbookPageTextOverlay.text = memo;
            if (scrapbookRecipeTextOverlay != null) scrapbookRecipeTextOverlay.text = WrapAtPeriod(recipe);
            ForceRebuild(scrapbookPageTextOverlay);
            ForceRebuild(scrapbookRecipeTextOverlay);
            return;
        }
        if (_scrapbookLegacyBodies == null || _scrapbookLegacyBodies.Length == 0) return;
        if (scrapbookRecipeTextOverlay != null) scrapbookRecipeTextOverlay.text = string.Empty;
        scrapbookPageTextOverlay.text =
            (index >= 0 && index < _scrapbookLegacyBodies.Length && !string.IsNullOrEmpty(_scrapbookLegacyBodies[index]))
            ? _scrapbookLegacyBodies[index] : string.Empty;
        if (isCookbookPanel) ForceRebuild(scrapbookPageTextOverlay);
    }

    /// <summary>온점(. 및 。) 뒤에 줄바꿈을 삽입합니다.</summary>
    private static string WrapAtPeriod(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        // ". " 또는 "." 뒤에 줄바꿈 — 이미 줄바꿈이 있으면 중복 삽입하지 않음
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            sb.Append(c);
            if ((c == '.' || c == '。' || c == ',' || c == '，') && i + 1 < text.Length && text[i + 1] != '\n')
            {
                // 온점 바로 뒤 공백은 건너뜀
                if (i + 1 < text.Length && text[i + 1] == ' ') i++;
                sb.Append('\n');
            }
        }
        return sb.ToString().TrimEnd();
    }

    private static void ForceRebuild(TextMeshProUGUI tmp)
    {
        if (tmp == null) return;
        LayoutRebuilder.ForceRebuildLayoutImmediate(tmp.rectTransform);
        tmp.ForceMeshUpdate(true);
    }

    // ── 초기화 헬퍼 ──────────────────────────────────────────────

    private void TryLoadScrapbookJson()
    {
        _cookbookSplitPages    = null;
        _scrapbookLegacyBodies = null;
        if (scrapbookContentJson == null) return;
        var raw = scrapbookContentJson.text;
        if (raw.IndexOf("\"recipe\"", StringComparison.Ordinal) >= 0)
        {
            var root = JsonUtility.FromJson<CookbookSplitRoot>(raw);
            if (root?.pages != null && root.pages.Length > 0) _cookbookSplitPages = root.pages;
            return;
        }
        _scrapbookLegacyBodies = JsonUtility.FromJson<ScrapbookPagesJson>(raw)?.pages;
    }

    private void EnsureCookBookRecipeTextUi()
    {
        if (!isCookbookPanel || scrapbookPageTextOverlay == null) return;
        if (scrapbookRecipeTextOverlay != null) return;
        var tr = transform.Find("CookBookRecipeText");
        if (tr != null)
        {
            scrapbookRecipeTextOverlay = tr.GetComponent<TextMeshProUGUI>();
            if (scrapbookRecipeTextOverlay != null) return;
        }
        var go  = new GameObject("CookBookRecipeText", typeof(RectTransform));
        var tmp = go.AddComponent<TextMeshProUGUI>();
        var src = scrapbookPageTextOverlay;
        tmp.font               = src.font;
        tmp.fontSharedMaterial = src.fontSharedMaterial;
        tmp.color              = src.color;
        tmp.raycastTarget      = false;
        tmp.richText           = true;
        tmp.text               = string.Empty;
        var rt = tmp.rectTransform;
        rt.SetParent(transform, false);
        rt.SetSiblingIndex(src.transform.GetSiblingIndex());
        scrapbookRecipeTextOverlay = tmp;
    }

    private void DeactivateOrphanPageChildren()
    {
        if (pages == null || pages.Length == 0) return;
        var pageSet = new System.Collections.Generic.HashSet<GameObject>(pages);
        foreach (Transform child in transform)
            if (child.GetComponent<Image>() != null
                && !pageSet.Contains(child.gameObject)
                && child.gameObject.name.StartsWith("CookBookPage"))
                child.gameObject.SetActive(false);
    }

    private static bool IsNullOrAllNull(Sprite[] arr)
    {
        if (arr == null || arr.Length == 0) return true;
        foreach (var s in arr) if (s != null) return false;
        return true;
    }

    private void TryAssignRecipeSpritesFromResources()
    {
        if (!isCookbookPanel || pages == null || pages.Length == 0) return;
        if (!IsNullOrAllNull(pageRecipeIllustrationSprites)) return;
        var loaded = new Sprite[pages.Length];
        bool any = false;
        for (int i = 0; i < pages.Length; i++)
        {
            loaded[i] = Resources.Load<Sprite>($"CookbookPages/{i + 1}");
            if (loaded[i] != null) any = true;
        }
        if (any) pageRecipeIllustrationSprites = loaded;
    }

    private void TryAssignPageBackgroundSpritesFromResources()
    {
        if (!isCookbookPanel || pages == null || pages.Length == 0) return;
        if (!IsNullOrAllNull(pageBackgroundSprites)) return;
        var loaded = new Sprite[pages.Length];
        bool any = false;
        for (int i = 0; i < pages.Length; i++)
        {
            loaded[i] = Resources.Load<Sprite>($"CookbookPageBg/{i + 1}");
            if (loaded[i] != null) any = true;
        }
        if (any) pageBackgroundSprites = loaded;
    }

    private void ApplyPageBackgroundSprites()
    {
        if (pageBackgroundSprites == null || pages == null) return;
        for (int i = 0; i < pages.Length && i < pageBackgroundSprites.Length; i++)
        {
            if (pageBackgroundSprites[i] == null) continue;
            var img = pages[i].GetComponent<Image>();
            if (img != null) img.sprite = pageBackgroundSprites[i];
        }
    }

    private void ApplyRecipeIllustrationSprites()
    {
        if (pageRecipeIllustrationSprites == null || pages == null) return;
        for (int i = 0; i < pages.Length && i < pageRecipeIllustrationSprites.Length; i++)
        {
            var sp = pageRecipeIllustrationSprites[i];
            if (sp == null) continue;
            var art = pages[i].transform.Find(RecipeIllustrationChildName);
            if (art == null) continue;
            var img = art.GetComponent<Image>();
            if (img == null) continue;
            img.sprite  = sp;
            img.enabled = true;
        }
    }

    private void SaveCurrentPage()
    {
        PlayerPrefs.SetInt(PREF_KEY, currentPageIndex);
        PlayerPrefs.Save();
    }

    void OnDisable() => SaveCurrentPage();
}
