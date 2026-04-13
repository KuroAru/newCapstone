using UnityEngine;
using UnityEngine.SceneManagement;
using Fungus;

public class WhenClikcedButton : MonoBehaviour
{
    public static WhenClikcedButton Instance;

    [Header("UI 연결")]
    [SerializeField] private GameObject panelToActivate;

    [Header("Room Objects (Panel의 자식으로 설정 필수)")]
    public GameObject unLocked, kitchen, locked, lib, lock2, made, bed, wife, Child, Tutor, Lock_2_1, Lock_2_2, Lock_2_3, Lock_2_4;

    private Transform targetCanvas;
    private Flowchart globalFlowchart; // Variablemanager의 변수를 가져올 용도

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); //

            // 지도가 이 오브젝트(싱글톤)를 따라다니게 부모를 설정합니다.
            if (panelToActivate != null)
            {
                panelToActivate.transform.SetParent(this.transform);
                panelToActivate.SetActive(false);
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            FindGlobalManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshReferences();
    }

    private void Update()
    {
        // 일부 경로(단축키/다른 스크립트)로 지도가 켜질 때 OnOpenMapClick을 거치지 않을 수 있어
        // 패널이 열린 동안에는 방 상태를 지속 동기화합니다.
        if (panelToActivate == null || !panelToActivate.activeInHierarchy)
            return;

        if (globalFlowchart == null)
            FindGlobalManager();

        UpdateAllRoomStates();
    }

    private void FindGlobalManager()
    {
        // 캐시가 씬 전환 후에도 유효하지만, Variablemanager 재탐색이 안전합니다.
        globalFlowchart = FlowchartLocator.Find();
    }

    private void RefreshReferences()
    {
        // 전역 인벤토리 캔버스를 우선적으로 찾습니다.
        GameObject invCanvas = GameObject.Find("GlobalInventoryCanvas_Prefab");
        if (invCanvas != null)
        {
            targetCanvas = invCanvas.transform;
        }
        else
        {
            Canvas c = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Exclude);
            if (c != null) targetCanvas = c.transform;
        }

        // 매 씬 로드마다 Variablemanager Flowchart를 다시 묶어 잘못된 차트·stale 참조를 방지합니다.
        FindGlobalManager();

        // Variablemanager를 못 찾는 씬에서도 전역(Fungus Global) fallback으로 상태를 갱신한다.
        if (globalFlowchart != null)
            globalFlowchart.SetBooleanVariable("isCalled", false); //

        UpdateAllRoomStates(); // 씬 로드 시 상태 갱신
    }

    public void OnOpenMapClick()
    {
        if (targetCanvas == null) RefreshReferences();

        if (panelToActivate != null && targetCanvas != null)
        {
            // 열기 전 최신 변수 상태 반영
            UpdateAllRoomStates();

            panelToActivate.SetActive(true);
            panelToActivate.transform.SetParent(targetCanvas, false); //

            RectTransform rect = panelToActivate.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;
                rect.SetAsLastSibling();
            }

            if (globalFlowchart != null) globalFlowchart.SetBooleanVariable("isCalled", true);
        }
    }

    public void OnCloseMapClick()
    {
        if (panelToActivate != null)
        {
            panelToActivate.SetActive(false);
            // 씬 이동 시 파괴되지 않게 다시 싱글톤 자식으로 회수
            panelToActivate.transform.SetParent(this.transform);
        }

        if (globalFlowchart != null) globalFlowchart.SetBooleanVariable("isCalled", false);
    }

    public void MoveScene(string sceneName)
    {
        OnCloseMapClick();
        SceneManager.LoadScene(sceneName); //
    }

    // 씬 이동용 함수들
    public void GoKitchen() => MoveScene("Kitchen");
    public void GoMaid() => MoveScene("MaidRoom");
    public void GoStudy() => MoveScene("StudyRoom");
    public void GoTutor() => MoveScene("TutorRoom");
    public void GoChild() => MoveScene("ChildRoom");
    public void GoWife() => MoveScene("WifeRoom");
    public void GoBed() => MoveScene("BedRoom");

    public void UpdateAllRoomStates()
    {
        // 각 방의 상태를 변수에 맞춰 강제로 고정합니다.
        UpdateRoomVisibility(unLocked, kitchen, "ElectricOn");
        UpdateRoomVisibility(locked, lib, "UsedStudyKey");
        UpdateRoomVisibility(lock2, made, "UsedMaidKey");
        UpdateRoomVisibility(Lock_2_4, bed, "UsedBedKey");
        UpdateRoomVisibility(Lock_2_3, wife, "UsedWifeKey");
        UpdateRoomVisibility(Lock_2_2, Tutor, "UsedTutorKey");
        UpdateRoomVisibility(Lock_2_1, Child, "UsedChildKey");
    }

    // ★ 핵심 수정 부분: 변수가 false일 때도 상태를 제어합니다.
    private void UpdateRoomVisibility(GameObject lockObj, GameObject openObj, string key)
    {
        bool isOpen = CheckVar(key);

        // 방이 열렸다면 자물쇠를 끄고 방을 켬, 반대면 자물쇠를 켜고 방을 끔
        if (lockObj != null) lockObj.SetActive(!isOpen);
        if (openObj != null) openObj.SetActive(isOpen);

        // 일부 맵 노드는 open 오브젝트 내부에 잠금 배지(자식 UI)가 같이 들어있다.
        // 전역 bool이 true인 경우 배지를 숨겨 "열림" 상태가 시각적으로 보이도록 강제한다.
        if (openObj != null && key == "UsedMaidKey")
            ToggleLockBadgeChildren(openObj, !isOpen);
    }

    private static void ToggleLockBadgeChildren(GameObject root, bool showLockBadge)
    {
        if (root == null) return;

        Transform[] all = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            Transform t = all[i];
            if (t == null || t == root.transform) continue;

            string n = t.name;
            if (string.IsNullOrEmpty(n)) continue;

            string lower = n.ToLowerInvariant();
            if (lower.Contains("lock") || lower.Contains("자물쇠"))
                t.gameObject.SetActive(showLockBadge);
        }
    }

    private bool CheckVar(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        // 1) 씬의 Variablemanager(Flowchart) 값을 우선
        if (globalFlowchart != null && globalFlowchart.GetBooleanVariable(key))
            return true;

        // 2) 변수 항목 누락/씬 교체 시에도 Fungus 전역 저장소를 fallback으로 조회
        return FlowchartLocator.GetFungusGlobalBoolean(key);
    }
}