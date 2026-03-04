using UnityEngine;
using UnityEngine.SceneManagement;
using Fungus;

public class WhenClikcedButton : MonoBehaviour
{
    public static WhenClikcedButton Instance;

    [Header("UI 연결")]
    [SerializeField] private GameObject panelToActivate; // 지도 전체 패널
    
    [Header("Room Objects (Fungus 연동용)")]
    public GameObject unLocked, kitchen, locked, lib, lock2, made, bed, wife, Child, Tutor, Lock_2_1, Lock_2_2, Lock_2_3, Lock_2_4;

    private Transform targetCanvas; // 현재 씬의 캔버스
    private Transform originalParent; // 지도의 원래 집 (프리펩 내부)
    private Flowchart flowchart;

    void Awake()
    {
        // 1. 싱글톤 및 중복 방지
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동 시 파괴 방지
            
            // ★ 핵심: 태어나자마자 원래 부모 위치를 기억해둡니다.
            if (panelToActivate != null)
            {
                originalParent = panelToActivate.transform.parent;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
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

    // 씬이 로드될 때마다 실행
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새 씬의 캔버스와 플로우차트를 찾습니다.
        RefreshReferences();
    }

    private void RefreshReferences()
    {
        flowchart = GameObject.FindObjectOfType<Flowchart>();

        Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>();
        foreach (Canvas c in canvases)
        {
            // 카메라 모드 캔버스 우선 탐색
            if (c.renderMode == RenderMode.ScreenSpaceCamera)
            {
                targetCanvas = c.transform;
                c.worldCamera = Camera.main; // 새 카메라 연결
                break;
            }
        }

        // 씬 이동 직후에는 지도가 닫혀있는 상태로 인식되게 변수 초기화
        if (flowchart != null)
        {
            flowchart.SetBooleanVariable("isCalled", false);
            UpdateAllRoomStates(); // 방 상태 갱신
        }
    }

    void Update()
    {
        if (targetCanvas == null) FindNewSceneCanvas();
        if (flowchart != null) UpdateAllRoomStates();
    }

    // --- [기능 1] 지도 열기 (지도 아이콘 클릭 시) ---
    public void OnOpenMapClick()
    {
        if (flowchart == null) flowchart = GameObject.FindObjectOfType<Flowchart>();
        if (flowchart == null) return;

        bool isCalled = flowchart.GetBooleanVariable("isCalled");

        // 닫혀있을 때만 염
        if (panelToActivate != null && targetCanvas != null && !isCalled)
        {
            panelToActivate.SetActive(true);
            panelToActivate.transform.SetParent(targetCanvas, false); // 캔버스로 입양

            // 위치 및 크기 초기화
            RectTransform rect = panelToActivate.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.SetAsLastSibling(); // 맨 앞으로

            flowchart.SetBooleanVariable("isCalled", true);
        }
    }

    public void OnCloseMapClick()
    {
        if (panelToActivate != null && originalParent != null)
        {
            panelToActivate.SetActive(false);
            panelToActivate.transform.SetParent(originalParent, false); // 원래 집으로 복귀
        }

        if (flowchart != null)
        {
            flowchart.SetBooleanVariable("isCalled", false);
        }
    }

    // --- [기능 3] 씬 이동 (지도 내 말풍선 클릭 시) ---
    public void MoveScene(string sceneName)
    {
        // 이동 전에 지도를 안전하게 프리펩 안으로 넣고 닫습니다.
        OnCloseMapClick();
        
        // 씬 로드
        SceneManager.LoadScene(sceneName);
    }

    // 버튼 연결용 함수들 (Inspector에서 연결)
    public void GoKitchen() => MoveScene("Kitchen");
    public void GoMaid() => MoveScene("MaidRoom");
    public void GoStudy() => MoveScene("StudyRoom");
    public void GoTutor() => MoveScene("TutorRoom");
    public void GoChild() => MoveScene("ChildRoom");
    public void GoWife() => MoveScene("WifeRoom");
    public void GoBed() => MoveScene("Bedroom");


    // --- 내부 유틸리티 함수들 ---
    private void FindNewSceneCanvas()
    {
        Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>();
        foreach (Canvas c in canvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceCamera)
            {
                targetCanvas = c.transform;
                if (c.worldCamera == null) c.worldCamera = Camera.main;
                break;
            }
        }
    }

    private void UpdateAllRoomStates()
    {
        // Fungus 변수 체크 로직 (기존 코드 유지)
        ChangeOBKitchen(); ChangeOBLib(); ChangeOBMade(); ChangeOBWife(); ChangeOBBed(); ChangeOBChild(); ChangeOBTutor();
    }

    private void ChangeOBKitchen() { if (CheckVar("ElectricOn")) SwapActive(unLocked, kitchen); }
    private void ChangeOBLib() { if (CheckVar("UsedStudyKey")) SwapActive(locked, lib); }
    private void ChangeOBMade() { if (CheckVar("UsedMaidKey")) SwapActive(lock2, made); }
    private void ChangeOBBed() { if (CheckVar("UsedBedKey")) SwapActive(Lock_2_4, bed); }
    private void ChangeOBWife() { if (CheckVar("UsedWifeKey")) SwapActive(Lock_2_3, wife); }
    private void ChangeOBTutor() { if (CheckVar("UsedTutorKey")) SwapActive(Lock_2_2, Tutor); }
    private void ChangeOBChild() { if (CheckVar("UsedChildKey")) SwapActive(Lock_2_1, Child); }

    private bool CheckVar(string key)
    {
        if (flowchart == null) return false;
        return flowchart.GetBooleanVariable(key);
    }

    private void SwapActive(GameObject lockObj, GameObject openObj)
    {
        if(lockObj != null) lockObj.SetActive(false);
        if(openObj != null) openObj.SetActive(true);
    }
}