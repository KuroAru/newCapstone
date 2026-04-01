using UnityEngine;
using Fungus;
using UnityEngine.SceneManagement;

/// <summary>
/// Fungus SaveMenu와 연동하여 슬롯별 세이브/로드를 안정적으로 처리하는 매니저.
/// - currentSlot 기반으로 saveDataKey를 개별화.
/// - 씬 이동 시 SaveMenu / Flowchart 자동 재탐색.
/// - 다른 씬 세이브일 경우 자동 전환 후 로드.
/// - GetSavedSceneName 미구현 문제 해결.
/// </summary>
public class SaveSlotManager : MonoBehaviour
{
    [Header("Fungus 연동")]
    public Flowchart flowchart;

    [Header("SaveMenu 참조")]
    public Fungus.SaveMenu saveMenu;

    [Header("설정 패널에 세이브 UI 붙이기")]
    [Tooltip("예: Opening_Office의 SavePannel 루트 RectTransform. 지정 시 ESC 설정을 열 때 이 UI를 설정 패널 안으로 옮깁니다.")]
    public RectTransform saveUiReparentRoot;

    [Tooltip("세이브 패널을 여는 플로팅 버튼 루트(예: Save.prefab RectTransform). saveUiReparentRoot와 별도인 경우 플레이 중 숨김·설정 패널 호스트로 같이 옮깁니다.")]
    public RectTransform savePanelOpenerRoot;

    private string baseSaveKey = "FungusSaveData_Slot";
    private string currentSaveKey;

    private void Awake()
    {
        ResolveRefs();
        ApplyCurrentSlotKey();
    }

    private void ResolveRefs()
    {
        if (saveMenu == null)
            saveMenu = FindObjectOfType<Fungus.SaveMenu>(true);
        if (flowchart == null)
            flowchart = FindObjectOfType<Flowchart>(true);
    }

    /// <summary>
    /// currentSlot에 따라 SaveMenu의 saveDataKey를 설정
    /// </summary>
    private void ApplyCurrentSlotKey()
    {
        if (flowchart == null)
        {
            Debug.LogWarning("[SaveSlotManager] Flowchart 연결 안 됨");
            return;
        }

        int slot = flowchart.GetIntegerVariable("currentSlot");
        if (slot < 1) slot = 1;

        currentSaveKey = $"{baseSaveKey}{slot}";

        if (saveMenu != null)
            saveMenu.SetSaveKey(currentSaveKey);

        Debug.Log($"[SaveSlotManager] saveDataKey → {currentSaveKey}");
    }

    /// <summary>
    /// Flowchart <c>currentSlot</c>에 맞춰 SaveMenu 키를 맞춥니다. 설정 패널 등에서 로드 버튼 활성 여부를 갱신할 때 사용합니다.
    /// </summary>
    public void EnsureSlotKeyApplied()
    {
        ResolveRefs();
        ApplyCurrentSlotKey();
    }

    public void Save()
    {
        ResolveRefs();

        if (saveMenu == null)
        {
            Debug.LogError("[SaveSlotManager] SaveMenu 찾기 실패 → Save 중단");
            return;
        }

        ApplyCurrentSlotKey();
        Debug.Log($"[SaveSlotManager] Save 호출 - {currentSaveKey}");
        saveMenu.Save();
    }

    public void Load()
    {
        Debug.Log("[SaveSlotManager] Load() 실행 시도됨");

        ResolveRefs();

        if (saveMenu == null)
        {
            Debug.LogError("[SaveSlotManager] SaveMenu 찾기 실패 → Load 중단");
            return;
        }

        saveMenu.gameObject.SetActive(true);
        saveMenu.enabled = true;

        ApplyCurrentSlotKey();

        // ✅ PlayerPrefs에서 직접 씬 이름 추출
        string targetScene = GetSavedSceneNameFromPrefs(currentSaveKey);
        string currentScene = SceneManager.GetActiveScene().name;

        Debug.Log($"[SaveSlotManager] 현재 씬: {currentScene}, 저장된 씬: {targetScene}");

        if (!string.IsNullOrEmpty(targetScene) && targetScene != currentScene)
        {
            Debug.Log($"[SaveSlotManager] 다른 씬 감지 → '{targetScene}' 로드 후 복원 예정");
            SceneManager.sceneLoaded += OnSceneLoadedForRestore;
            SceneManager.LoadScene(targetScene);
            return;
        }

        InternalLoadNow();
    }

    /// <summary>
    /// PlayerPrefs에서 저장된 데이터 문자열을 직접 파싱하여 씬 이름을 얻는 함수
    /// </summary>
    private string GetSavedSceneNameFromPrefs(string key)
    {
        string json = PlayerPrefs.GetString(key, "");
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning($"[SaveSlotManager] {key} 키에 저장된 데이터 없음");
            return null;
        }

        // Fungus SaveData JSON 내에 "sceneName":"XXX" 형태로 저장됨
        int idx = json.IndexOf("\"sceneName\":\"");
        if (idx == -1) return null;

        int start = idx + "\"sceneName\":\"".Length;
        int end = json.IndexOf("\"", start);
        if (end == -1) return null;

        string sceneName = json.Substring(start, end - start);
        return sceneName;
    }

    private void OnSceneLoadedForRestore(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedForRestore;
        Debug.Log($"[SaveSlotManager] 씬 '{scene.name}' 로드 완료 → 세이브 데이터 복원 시작");

        ResolveRefs();
        InternalLoadNow();
    }

    private void InternalLoadNow()
    {
        var saveManager = FungusManager.Instance.SaveManager;
        if (saveManager != null)
        {
            saveManager.ClearHistory();
            Fungus.SaveManagerSignals.DoSaveReset();
            Debug.Log("[SaveSlotManager] SaveManager 히스토리 초기화 완료");
        }

        saveMenu.Load();
        Debug.Log("[SaveSlotManager] Load 호출 완료 (씬 복원 성공)");
    }
}
