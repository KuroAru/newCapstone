using System;

using UnityEngine;

using UnityEngine.UI;

using TMPro;



public class BookPanelController : MonoBehaviour

{

    [Header("페이지 오브젝트 목록")]

    public GameObject[] pages;



    [Header("가정부 방 스크랩북 (선택)")]

    [Tooltip("비어 있으면 CookBook_Panel에서 Resources/MaidRoomCookbookScrapbook 를 자동 로드")]

    [SerializeField] private TextAsset scrapbookContentJson;

    [Tooltip("오른쪽 페이지 — 가정부 메모 (가운데 정렬)")]

    [SerializeField] private TextMeshProUGUI scrapbookPageTextOverlay;

    [Tooltip("왼쪽 페이지 하단 — 레시피 본문 (비어 있으면 런타임에 CookBookRecipeText 생성)")]

    [SerializeField] private TextMeshProUGUI scrapbookRecipeTextOverlay;



    public const string RecipeIllustrationChildName = "RecipeIllustration";



    [Header("페이지별 배경 (선택, 페이지 루트 Image)")]

    [Tooltip("종이·스프레드 전체를 덮는 스프라이트. 인스펙터에서만 지정 (Resources 자동 로드 없음)")]

    [SerializeField] private Sprite[] pageBackgroundSprites;



    [Header("왼쪽 레시피 일러 (선택, 자식 RecipeIllustration)")]

    [Tooltip("각 페이지 아래 RecipeIllustration 오브젝트의 Image. CookBook_Panel이고 비어 있으면 Resources/CookbookPages/1,2,… 로드")]

    [SerializeField] private Sprite[] pageRecipeIllustrationSprites;



    private int currentPageIndex = 0;



    private string PREF_KEY;



    private CookbookPagePair[] _cookbookSplitPages;

    private string[] _scrapbookLegacyBodies;



    private const float CookBookMemoFontSize = 28f;

    // 왼쪽 페이지 중앙(≈x0.252) 아래로 레시피 블록 — 일러스트는 씬 RecipeIllustration이 같은 X에 고정 박스
    private const float CookBookRecipeFontSize = 26f;



    private static readonly Vector2 CookBookMemoAnchorMin = new Vector2(0.54f, 0.2f);

    private static readonly Vector2 CookBookMemoAnchorMax = new Vector2(0.84f, 0.82f);

    private static readonly Vector4 CookBookMemoMargin = new Vector4(12f, 16f, 12f, 16f);



    // 왼쪽 페이지 세로 중심과 맞춘 좁은 열(가운데 정렬 TMP)
    private static readonly Vector2 CookBookRecipeAnchorMin = new Vector2(0.118f, 0.11f);

    private static readonly Vector2 CookBookRecipeAnchorMax = new Vector2(0.386f, 0.465f);

    private static readonly Vector4 CookBookRecipeMargin = new Vector4(12f, 4f, 12f, 8f);



    private static readonly Vector2 CookBookLegacyMemoAnchorMin = new Vector2(0.52f, 0.14f);

    private static readonly Vector2 CookBookLegacyMemoAnchorMax = new Vector2(0.86f, 0.87f);

    private static readonly Vector4 CookBookLegacyMemoMargin = new Vector4(8f, 12f, 14f, 12f);



    [Serializable]

    private class CookbookPagePair

    {

        public string recipe;

        public string memo;

    }



    [Serializable]

    private class CookbookSplitRoot

    {

        public CookbookPagePair[] pages;

    }



    [Serializable]

    private class ScrapbookPagesJson

    {

        public string[] pages;

    }



    void Awake()

    {

        PREF_KEY = "LastBookPage_" + gameObject.name;



        if (scrapbookContentJson == null && gameObject.name == "CookBook_Panel")

            scrapbookContentJson = Resources.Load<TextAsset>("MaidRoomCookbookScrapbook");



        if (scrapbookPageTextOverlay == null)

        {

            var t = transform.Find("CookBookScrapText");

            if (t != null)

                scrapbookPageTextOverlay = t.GetComponent<TextMeshProUGUI>();

        }



        TryLoadScrapbookJson();



        TryAssignRecipeSpritesFromResources();

        ApplyPageBackgroundSprites();

        ApplyRecipeIllustrationSprites();



        if (gameObject.name == "CookBook_Panel")

        {

            if (_cookbookSplitPages != null && _cookbookSplitPages.Length > 0)

            {

                EnsureCookBookRecipeTextUi();

                ApplyCookBookMemoLayoutCentered();

                ApplyCookBookRecipeLayout();

            }

            else

                ApplyCookBookMemoLayoutLegacy();

        }

    }



    private void TryLoadScrapbookJson()

    {

        _cookbookSplitPages = null;

        _scrapbookLegacyBodies = null;

        if (scrapbookContentJson == null)

            return;



        var raw = scrapbookContentJson.text;

        if (raw.IndexOf("\"recipe\"", StringComparison.Ordinal) >= 0)

        {

            var root = JsonUtility.FromJson<CookbookSplitRoot>(raw);

            if (root?.pages != null && root.pages.Length > 0)

                _cookbookSplitPages = root.pages;

            return;

        }



        var legacy = JsonUtility.FromJson<ScrapbookPagesJson>(raw);

        _scrapbookLegacyBodies = legacy?.pages;

    }



    private void EnsureCookBookRecipeTextUi()

    {

        if (gameObject.name != "CookBook_Panel" || scrapbookPageTextOverlay == null)

            return;

        if (scrapbookRecipeTextOverlay != null)

            return;



        var tr = transform.Find("CookBookRecipeText");

        if (tr != null)

        {

            scrapbookRecipeTextOverlay = tr.GetComponent<TextMeshProUGUI>();

            if (scrapbookRecipeTextOverlay != null)

                return;

        }



        var go = new GameObject("CookBookRecipeText", typeof(RectTransform));

        var tmp = go.AddComponent<TextMeshProUGUI>();

        var src = scrapbookPageTextOverlay;

        tmp.font = src.font;

        tmp.fontSharedMaterial = src.fontSharedMaterial;

        tmp.fontSize = CookBookRecipeFontSize;

        tmp.color = src.color;

        tmp.raycastTarget = false;

        tmp.richText = true;

        tmp.text = string.Empty;



        var rt = tmp.rectTransform;

        rt.SetParent(transform, false);

        rt.SetSiblingIndex(scrapbookPageTextOverlay.transform.GetSiblingIndex());

        scrapbookRecipeTextOverlay = tmp;

    }



    private void ApplyCookBookMemoLayoutCentered()

    {

        if (gameObject.name != "CookBook_Panel" || scrapbookPageTextOverlay == null)

            return;



        var rt = scrapbookPageTextOverlay.rectTransform;

        rt.anchorMin = CookBookMemoAnchorMin;

        rt.anchorMax = CookBookMemoAnchorMax;

        rt.pivot = new Vector2(0.5f, 0.5f);

        rt.offsetMin = Vector2.zero;

        rt.offsetMax = Vector2.zero;

        rt.anchoredPosition = Vector2.zero;



        var tmp = scrapbookPageTextOverlay;

        tmp.fontSize = CookBookMemoFontSize;

        tmp.textWrappingMode = TextWrappingModes.Normal;

        tmp.overflowMode = TextOverflowModes.Overflow;

        tmp.wordWrappingRatios = 0.5f;

        tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;

        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;

        tmp.margin = CookBookMemoMargin;

    }



    private void ApplyCookBookRecipeLayout()

    {

        if (gameObject.name != "CookBook_Panel" || scrapbookRecipeTextOverlay == null)

            return;



        var rt = scrapbookRecipeTextOverlay.rectTransform;

        rt.anchorMin = CookBookRecipeAnchorMin;

        rt.anchorMax = CookBookRecipeAnchorMax;

        rt.pivot = new Vector2(0.5f, 1f);

        rt.offsetMin = Vector2.zero;

        rt.offsetMax = Vector2.zero;

        rt.anchoredPosition = Vector2.zero;



        var tmp = scrapbookRecipeTextOverlay;

        tmp.fontSize = CookBookRecipeFontSize;

        tmp.textWrappingMode = TextWrappingModes.Normal;

        tmp.overflowMode = TextOverflowModes.Overflow;

        tmp.wordWrappingRatios = 0.5f;

        tmp.lineSpacing = 2f;

        tmp.paragraphSpacing = 8f;

        tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;

        tmp.verticalAlignment = VerticalAlignmentOptions.Top;

        tmp.margin = CookBookRecipeMargin;

        tmp.extraPadding = true;

    }



    private void ApplyCookBookMemoLayoutLegacy()

    {

        if (gameObject.name != "CookBook_Panel" || scrapbookPageTextOverlay == null)

            return;



        var rt = scrapbookPageTextOverlay.rectTransform;

        rt.anchorMin = CookBookLegacyMemoAnchorMin;

        rt.anchorMax = CookBookLegacyMemoAnchorMax;

        rt.pivot = new Vector2(0.5f, 1f);

        rt.offsetMin = Vector2.zero;

        rt.offsetMax = Vector2.zero;

        rt.anchoredPosition = Vector2.zero;



        var tmp = scrapbookPageTextOverlay;

        tmp.textWrappingMode = TextWrappingModes.Normal;

        tmp.overflowMode = TextOverflowModes.Overflow;

        tmp.wordWrappingRatios = 0.5f;

        tmp.horizontalAlignment = HorizontalAlignmentOptions.Left;

        tmp.verticalAlignment = VerticalAlignmentOptions.Top;

        tmp.margin = CookBookLegacyMemoMargin;

    }



    private static bool IsNullOrAllNull(Sprite[] arr)

    {

        if (arr == null || arr.Length == 0) return true;

        foreach (var s in arr)

            if (s != null) return false;

        return true;

    }



    private void TryAssignRecipeSpritesFromResources()

    {

        if (gameObject.name != "CookBook_Panel" || pages == null || pages.Length == 0)

            return;

        if (!IsNullOrAllNull(pageRecipeIllustrationSprites))

            return;



        var loaded = new Sprite[pages.Length];

        var any = false;

        for (var i = 0; i < pages.Length; i++)

        {

            loaded[i] = Resources.Load<Sprite>($"CookbookPages/{i + 1}");

            if (loaded[i] != null)

                any = true;

        }



        if (any)

            pageRecipeIllustrationSprites = loaded;

    }



    private void ApplyPageBackgroundSprites()

    {

        if (pageBackgroundSprites == null || pages == null)

            return;



        for (var i = 0; i < pages.Length; i++)

        {

            if (i >= pageBackgroundSprites.Length)

                break;

            var sp = pageBackgroundSprites[i];

            if (sp == null)

                continue;

            var img = pages[i].GetComponent<Image>();

            if (img != null)

                img.sprite = sp;

        }

    }



    private void ApplyRecipeIllustrationSprites()

    {

        if (pageRecipeIllustrationSprites == null || pages == null)

            return;



        for (var i = 0; i < pages.Length; i++)

        {

            if (i >= pageRecipeIllustrationSprites.Length)

                break;

            var sp = pageRecipeIllustrationSprites[i];

            if (sp == null)

                continue;

            var art = pages[i].transform.Find(RecipeIllustrationChildName);

            if (art == null)

                continue;

            var img = art.GetComponent<Image>();

            if (img == null)

                continue;

            img.sprite = sp;

            img.enabled = sp != null;

        }

    }



    void OnEnable()

    {

        currentPageIndex = PlayerPrefs.GetInt(PREF_KEY, 0);

        ShowPage(currentPageIndex);

    }



    public void NextPage()

    {

        if (currentPageIndex < pages.Length - 1)

        {

            currentPageIndex++;

            ShowPage(currentPageIndex);

            SaveCurrentPage();

        }

    }



    public void PreviousPage()

    {

        if (currentPageIndex > 0)

        {

            currentPageIndex--;

            ShowPage(currentPageIndex);

            SaveCurrentPage();

        }

    }



    private void ShowPage(int index)

    {

        index = Mathf.Clamp(index, 0, pages.Length - 1);



        for (int i = 0; i < pages.Length; i++)

            pages[i].SetActive(i == index);



        UpdateScrapbookOverlay(index);

    }



    private void UpdateScrapbookOverlay(int index)

    {

        if (scrapbookPageTextOverlay == null)

            return;



        if (gameObject.name == "CookBook_Panel" && _cookbookSplitPages != null && _cookbookSplitPages.Length > 0)

        {

            EnsureCookBookRecipeTextUi();



            string memo = string.Empty;

            string recipe = string.Empty;

            if (index >= 0 && index < _cookbookSplitPages.Length)

            {

                var p = _cookbookSplitPages[index];

                memo = p.memo ?? string.Empty;

                recipe = p.recipe ?? string.Empty;

            }



            scrapbookPageTextOverlay.text = memo;

            if (scrapbookRecipeTextOverlay != null)

                scrapbookRecipeTextOverlay.text = recipe;



            LayoutRebuilder.ForceRebuildLayoutImmediate(scrapbookPageTextOverlay.rectTransform);

            scrapbookPageTextOverlay.ForceMeshUpdate(true);

            if (scrapbookRecipeTextOverlay != null)

            {

                LayoutRebuilder.ForceRebuildLayoutImmediate(scrapbookRecipeTextOverlay.rectTransform);

                scrapbookRecipeTextOverlay.ForceMeshUpdate(true);

            }



            return;

        }



        if (_scrapbookLegacyBodies == null || _scrapbookLegacyBodies.Length == 0)

            return;



        if (scrapbookRecipeTextOverlay != null)

            scrapbookRecipeTextOverlay.text = string.Empty;



        if (index >= 0 && index < _scrapbookLegacyBodies.Length && !string.IsNullOrEmpty(_scrapbookLegacyBodies[index]))

            scrapbookPageTextOverlay.text = _scrapbookLegacyBodies[index];

        else

            scrapbookPageTextOverlay.text = string.Empty;



        if (gameObject.name == "CookBook_Panel")

        {

            LayoutRebuilder.ForceRebuildLayoutImmediate(scrapbookPageTextOverlay.rectTransform);

            scrapbookPageTextOverlay.ForceMeshUpdate(true);

        }

    }



    private void SaveCurrentPage()

    {

        PlayerPrefs.SetInt(PREF_KEY, currentPageIndex);

        PlayerPrefs.Save();

    }



    void OnDisable()

    {

        SaveCurrentPage();

    }

}


