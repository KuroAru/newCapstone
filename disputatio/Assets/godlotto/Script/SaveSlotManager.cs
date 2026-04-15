using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using Fungus;
using UnityEngine.SceneManagement;

/// <summary>
/// Fungus <see cref="SaveManager"/>를 직접 사용하는 멀티슬롯 세이브/로드.
/// 슬롯별로 히스토리를 1포인트로 덮어써 VN식 슬롯에 가깝게 동작합니다.
/// </summary>
public class SaveSlotManager : MonoBehaviour
{
    public const int MaxSlots = 32;

    [Header("Fungus")]
    [Tooltip("비우면 FlowchartLocator(Variablemanager) 사용. currentSlot 정수 변수가 있는 Flowchart를 지정하세요.")]
    [SerializeField] Flowchart flowchart;

    [Tooltip("스냅샷에 쓸 Save Point 키 — Variablemanager 문자열 변수명 (예: SavePointKey). 없으면 씬명_SettingsSave.")]
    [SerializeField] string savePointKeyVariableName = "SavePointKey";

    [Header("키 접두사 (Fungus 기본과 동일 패턴)")]
    [SerializeField] string slotKeyPrefix = FungusSaveStorage.DefaultSlotKeyPrefix;

    [Tooltip("마지막으로 저장/로드한 슬롯 (메인 메뉴 등)")]
    [SerializeField] string lastUsedSlotPrefsKey = "LastUsedFungusSaveSlot";

    [Header("미리보기 썸네일")]
    [SerializeField] bool captureSlotThumbnail = true;

    Coroutine _thumbnailCoroutine;

    public string SlotKeyPrefix => slotKeyPrefix;

    /// <summary>
    /// Fungus <see cref="SaveManager"/>를 찾습니다. Instance 프로퍼티가 아직 채워지지 않았거나(비활성 등) 씬에 단독으로 있을 때도 보완합니다.
    /// </summary>
    public SaveManager GetResolvedSaveManager()
    {
        FungusManager fm = FungusManager.Instance;
        if (fm != null)
        {
            if (fm.SaveManager != null)
                return fm.SaveManager;
            SaveManager onSame = fm.GetComponent<SaveManager>();
            if (onSame != null)
                return onSame;
        }
        return Object.FindFirstObjectByType<SaveManager>(FindObjectsInactive.Include);
    }

    public static string SlotDataKey(int slot, string prefix = FungusSaveStorage.DefaultSlotKeyPrefix)
    {
        if (slot < 1)
            slot = 1;
        return prefix + slot;
    }

    public int GetResolvedSlotIndex()
    {
        Flowchart preferred = FlowchartLocator.Resolve(flowchart);
        if (preferred != null)
        {
            int s = preferred.GetIntegerVariable("currentSlot");
            if (s >= 1 && s <= MaxSlots)
                return s;
        }

        Flowchart[] charts = Object.FindObjectsByType<Flowchart>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < charts.Length; i++)
        {
            int s = charts[i].GetIntegerVariable("currentSlot");
            if (s >= 1 && s <= MaxSlots)
                return s;
        }

        return 1;
    }

    public bool SlotHasData(int slot)
    {
        SaveManager saveManager = GetResolvedSaveManager();
        if (saveManager == null)
            return false;
        return saveManager.SaveDataExists(SlotDataKey(slot, slotKeyPrefix));
    }

    /// <summary>
    /// Fungus가 인식할 수 있는 키로 현재 상태 스냅샷을 만들고 지정 슬롯 파일에만 기록합니다.
    /// </summary>
    public void SaveToSlot(int slot)
    {
        if (slot < 1 || slot > MaxSlots)
        {
            GameLog.LogWarning($"[SaveSlotManager] 슬롯 범위 밖: {slot}");
            return;
        }

        SaveManager saveManager = GetResolvedSaveManager();
        if (saveManager == null)
        {
            Debug.LogError("[SaveSlotManager] SaveManager 없음");
            return;
        }

        string pointKey = ResolveSnapshotSavePointKey();
        string description = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        PersistInventoryToFlowchart();
        saveManager.ClearHistory();
        saveManager.AddSavePoint(pointKey, description);
        saveManager.Save(SlotDataKey(slot, slotKeyPrefix));

        PlayerPrefs.SetInt(lastUsedSlotPrefsKey, slot);
        PlayerPrefs.Save();

        GameLog.Log($"[SaveSlotManager] 슬롯 {slot} 저장 ({pointKey})");

        if (captureSlotThumbnail)
        {
            if (_thumbnailCoroutine != null)
                StopCoroutine(_thumbnailCoroutine);
            _thumbnailCoroutine = StartCoroutine(CaptureAndWriteSlotThumbnail(slot));
        }
    }

    IEnumerator CaptureAndWriteSlotThumbnail(int slot)
    {
        float prevScale = Time.timeScale;
        if (prevScale <= 0f)
            Time.timeScale = 1f;
        yield return new WaitForEndOfFrame();
        Texture2D full = null;
        try
        {
            string dir = Application.persistentDataPath + "/FungusSaves/";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string path = FungusSaveSlotSummary.GetThumbnailPath(slot, slotKeyPrefix);

            if (TryWriteThumbnailFromChangeSp(slot, path))
                yield break;

            full = ScreenCapture.CaptureScreenshotAsTexture();
            if (full == null)
                yield break;

            int tw = SaveThumbnailEncoder.ThumbnailMaxWidth;
            int th = Mathf.Max(1, Mathf.RoundToInt(full.height * (tw / (float)full.width)));
            Texture2D scaled = ScaleTextureBlit(full, tw, th);
            File.WriteAllBytes(path, scaled.EncodeToPNG());
            Destroy(scaled);
        }
        finally
        {
            if (full != null)
                Destroy(full);
            _thumbnailCoroutine = null;
            Time.timeScale = prevScale;
        }
    }

    /// <summary>
    /// <see cref="ChangeSP"/> 스택 또는 <c>SceneName</c> 매핑 스프라이트로 PNG를 씁니다.
    /// </summary>
    bool TryWriteThumbnailFromChangeSp(int slot, string path)
    {
        Sprite sp = null;
        if (ChangeSP.TryPeekSaveThumbnailSprite(out Sprite peeked))
            sp = peeked;

        if (sp == null)
        {
            ChangeSP catalog = ChangeSP.FindCatalog();
            Flowchart fc = FlowchartLocator.Find();
            if (catalog != null && fc != null)
            {
                string sceneName = fc.GetStringVariable("SceneName");
                if (!string.IsNullOrEmpty(sceneName))
                    sp = catalog.GetThumbnailSpriteForSceneName(sceneName);
            }
        }

        if (sp == null)
            return false;

        if (SaveThumbnailEncoder.TryWriteSpriteAsSlotPng(sp, path))
        {
            GameLog.Log($"[SaveSlotManager] 슬롯 {slot} 썸네일: ChangeSP 스프라이트 저장");
            return true;
        }

        return false;
    }

    static Texture2D ScaleTextureBlit(Texture2D src, int w, int h)
    {
        RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        RenderTexture prev = RenderTexture.active;
        Graphics.Blit(src, rt);
        RenderTexture.active = rt;
        Texture2D dst = new Texture2D(w, h, TextureFormat.RGB24, false);
        dst.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        dst.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return dst;
    }

    /// <summary>
    /// Flowchart의 currentSlot에 맞춰 저장 (레거시 Fungus CallMethod 호환).
    /// </summary>
    public void Save()
    {
        SaveToSlot(GetResolvedSlotIndex());
    }

    /// <summary>
    /// 슬롯을 로드합니다.
    /// </summary>
    /// <returns>
    /// true: 현재 씬에서 로드가 적용됨(호출 측에서 일시정지 해제 권장).
    /// false: 다른 씬으로 넘어가거나, 실패로 조기 반환.
    /// </returns>
    public bool LoadFromSlot(int slot)
    {
        if (slot < 1 || slot > MaxSlots)
        {
            GameLog.LogWarning($"[SaveSlotManager] 슬롯 범위 밖: {slot}");
            return false;
        }

        SaveManager saveManager = GetResolvedSaveManager();
        if (saveManager == null)
        {
            Debug.LogError("[SaveSlotManager] SaveManager 없음");
            return false;
        }

        string dataKey = SlotDataKey(slot, slotKeyPrefix);
        if (!saveManager.SaveDataExists(dataKey))
        {
            GameLog.LogWarning($"[SaveSlotManager] 슬롯 {slot} 데이터 없음");
            return false;
        }

        PlayerPrefs.SetInt(lastUsedSlotPrefsKey, slot);
        PlayerPrefs.Save();

        string targetScene = FungusSaveStorage.TryParseSceneNameFromHistoryJson(FungusSaveStorage.ReadHistoryRaw(dataKey));
        string currentScene = SceneManager.GetActiveScene().name;

        if (!string.IsNullOrEmpty(targetScene) && targetScene != currentScene)
        {
            Time.timeScale = 1f;
            SceneManager.sceneLoaded += OnSceneLoadedForRestore;
            SceneManager.LoadScene(targetScene);
            _pendingLoadKey = dataKey;
            return false;
        }

        Time.timeScale = 1f;
        InternalLoadNow(dataKey);
        return true;
    }

    string _pendingLoadKey;

    /// <summary>
    /// currentSlot 슬롯 로드 (레거시 호환).
    /// </summary>
    public void Load()
    {
        LoadFromSlot(GetResolvedSlotIndex());
    }

    /// <summary>
    /// 메인 메뉴 등: 마지막 사용 슬롯 또는 1번 슬롯 로드.
    /// </summary>
    public void LoadLastOrFirstSlot()
    {
        int s = PlayerPrefs.GetInt(lastUsedSlotPrefsKey, 1);
        if (!SlotHasData(s))
        {
            for (int i = 1; i <= MaxSlots; i++)
            {
                if (SlotHasData(i))
                {
                    s = i;
                    break;
                }
            }
        }

        if (SlotHasData(s))
            LoadFromSlot(s);
        else
            GameLog.LogWarning("[SaveSlotManager] 로드할 슬롯 데이터가 없습니다.");
    }

    private void OnSceneLoadedForRestore(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedForRestore;
        if (string.IsNullOrEmpty(_pendingLoadKey))
            return;

        string key = _pendingLoadKey;
        _pendingLoadKey = null;
        InternalLoadNow(key);
    }

    private void PersistInventoryToFlowchart()
    {
        Flowchart fc = FlowchartLocator.Find();
        if (fc == null) return;
        InventoryManager inv = InventoryManager.Instance;
        if (inv == null) return;
        string ids = string.Join(",", inv.Items.Select(i => i.itemId.ToString()));
        fc.SetStringVariable(FungusVariableKeys.InventoryItemIds, ids);
    }

    private void InternalLoadNow(string dataKey)
    {
        SaveManager saveManager = GetResolvedSaveManager();
        if (saveManager == null)
        {
            Debug.LogError("[SaveSlotManager] InternalLoadNow: SaveManager를 찾을 수 없습니다.");
            return;
        }

        saveManager.ClearHistory();
        SaveManagerSignals.DoSaveReset();
        saveManager.Load(dataKey);
        GameLog.Log($"[SaveSlotManager] 로드 완료: {dataKey}");
    }

    private string ResolveSnapshotSavePointKey()
    {
        Flowchart fc = FlowchartLocator.Find();
        if (fc != null && !string.IsNullOrEmpty(savePointKeyVariableName))
        {
            string k = fc.GetStringVariable(savePointKeyVariableName);
            if (!string.IsNullOrEmpty(k))
                return k;
        }

        return SceneManager.GetActiveScene().name + "_SettingsSave";
    }
}
