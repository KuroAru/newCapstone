using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public struct JumpscareSceneData
{
    public string sceneName;        // 씬 이름
    [Tooltip("카메라 중심을 (0,0)으로 하는 상대 좌표입니다. (0,0) 입력 시 화면 정중앙에 스폰됩니다.")]
    public Vector2 spawnPosition;
    [Range(0f, 100f)]
    [Tooltip("씬 진입 직후 첫 판정에서의 등장 확률(%). 실패 시 같은 씬에 일정 시간 이상 머무르면 100%로 등장합니다.")]
    public float spawnChance;
}

[System.Serializable]
public struct AutoRegisteredSpawnPositionOverride
{
    [Tooltip("자동 등록 대상 sceneName과 정확히 일치해야 적용됩니다.")]
    public string sceneName;
    [Tooltip("해당 자동 등록 씬에서 사용할 적 스폰 위치입니다.")]
    public Vector2 spawnPosition;
}

public class JumpscareManager : MonoBehaviour
{
    public static JumpscareManager Instance;
    public static event Action OnPlayerDied;
    public static event Action OnEnemyAppeared; // 추가: 적이 스폰되었을 때
public static event Action OnJumpscareReset; // 추가: 상태가 초기화될 때 (씬 이동 등)

    private const float SpawnPositionZeroEpsilonSq = 1e-6f;
    private const string SpriteUnlitShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";
    private const string MainCanvasTag = "MainCanvas";
    /// <summary>눈깜빡임·게임오버 전면 스프라이트 평면 Z = 카메라 Z + 이 값 (Blink와 동일).</summary>
    private const float OverlayPlaneZOffsetFromCamera = 1f;
    private const float NonRightHallSpawnChancePercent = 20f;
    private const string RightHallSceneName = "Hall_Right";

    [Header("씬별 설정 목록")]
    public List<JumpscareSceneData> targetScenes;

    [Header("Mokotan 점프스케어 씬 자동 등록")]
    [Tooltip("코드에 정의된 Mokotan 복도·층 씬이 targetScenes에 없을 때 추가하며, 이때 사용할 진입 직후 spawnChance입니다. 이미 있는 sceneName은 건드리지 않습니다.")]
    [Range(0f, 100f)]
    [SerializeField] private float defaultSpawnChanceForMokotanScenes = 20f;
    [Tooltip("자동 등록되는 씬에 공통 적용할 기본 적 스폰 위치입니다.")]
    [SerializeField] private Vector2 defaultAutoRegisteredSpawnPosition = Vector2.zero;
    [Tooltip("자동 등록되는 씬별 적 스폰 위치 오버라이드 목록입니다. sceneName 일치 시 이 값이 우선 적용됩니다.")]
    [SerializeField] private List<AutoRegisteredSpawnPositionOverride> autoRegisteredSpawnPositionOverrides = new List<AutoRegisteredSpawnPositionOverride>();

    [Tooltip("진입 직후 스폰에 실패한 경우, 같은 활성 씬에 이 시간(초) 이상 머무르면 등장 확률 100%로 트리거를 띄웁니다.")]
    [SerializeField] private float guaranteedJumpscareAfterSeconds = 60f;

    /// <summary>
    /// 에셋 파일명(확장자 제외)과 동일한 Scene.name을 가정합니다. Awake에서 targetScenes에 없는 이름만 병합합니다.
    /// </summary>
    private static readonly string[] MokotanJumpscareSceneNames =
    {
        "Hall_Left",
        "Hall_Left2",
        "Hallway_Left2",
        "Hall_Right",
        "Hall_Right2",
        "Hallway_Right2",
        "Hallway_Right",
        "2floorRight",
        "2floorLeft",
        "2floorHallway_Right",
        "2floorHallway_Left",
    };

    [Header("눈깜빡임 오버레이 (SpriteRenderer)")]
    [Tooltip("카메라 앞에 배치할 전체화면 눈깜빡임 Sprite")]
    public SpriteRenderer blinkOverlay;

    [Header("효과 설정 (블러)")]
    private Volume globalVolume;

    [Header("시간 설정")]
    public float waitTimeToScare = 3f;
    public float animationDuration = 2f;
    public float blinkDuration = 0.2f;
    public float closedDuration = 0.1f;
    public string retrySceneName = "MainScene";

    [Header("트리거 깊이 (2D 오쏘)")]
    [Tooltip("트리거 월드 Z = Main Camera Z + 이 값. (예: 카메라 z=-10, 스프라이트 평면 z=0 → 10)")]
    [SerializeField] private float triggerWorldZOffsetFromCamera = 10f;

    [Header("트리거 스프라이트")]
    [Tooltip("Lit 씬 조명 없이도 보이게 URP Sprite-Unlit을 씁니다. 끄면 프리팹 머티리얼을 유지합니다.")]
    [SerializeField] private bool useUnlitMaterialForTrigger = true;
#if UNITY_EDITOR
    [Tooltip("에디터/개발 빌드에서만 스폰 직후 트리거 렌더 상태를 한 프레임 뒤 로그합니다.")]
    [SerializeField] private bool logTriggerRenderingAfterSpawn;
#endif

    [Header("오브젝트 할당")]
    [Tooltip("적 클릭 트리거용 오브젝트 (SpriteRenderer + Collider2D 필요)")]
    public GameObject triggerObject;
    public Animator jumpscareAnimator;

    [Header("게임오버 오브젝트")]
    [Tooltip("게임오버 시 표시할 오브젝트 (SpriteRenderer 기반)")]
    public GameObject gameOverObject;
    [Tooltip("리트라이 클릭 영역 (Collider2D 필요)")]
    public GameObject retryClickObject;

    [Header("적 등장 시 숨길 오브젝트")]
    [Tooltip("적이 등장하면 비활성화될 Sprite 오브젝트들의 Tag")]
    public string hideObjectTag = "HideOnEnemy";

    private bool hasTriggered = false;
    private DepthOfField dof;
    private readonly int blinkAmountProp = Shader.PropertyToID("_BlinkAmount");
    private bool isBlinkSequenceRunning = false;

    // 점프스케어 진행 중 클릭 차단
    private bool isJumpscareInProgress = false;
    // 점프스케어 시 비활성화할 SayDialog
    private GameObject sayDialogObject;

    // 눈깜빡임 오버레이 자동 크기 조절용
    private Camera mainCam;

    // triggerObject의 보이는 부분만 끄기 위한 캐시
    private SpriteRenderer triggerSpriteRenderer;
    private Collider2D triggerCollider;
    private readonly List<GameObject> hiddenMainCanvases = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureMokotanJumpscareScenesRegistered();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// MokotanJumpscareSceneNames에 있으나 targetScenes에 없는 씬을 기본 spawn 설정으로 추가합니다.
    /// </summary>
    private void EnsureMokotanJumpscareScenesRegistered()
    {
        if (targetScenes == null)
            targetScenes = new List<JumpscareSceneData>();

        foreach (string sceneName in MokotanJumpscareSceneNames)
        {
            if (string.IsNullOrEmpty(sceneName))
                continue;

            bool exists = false;
            foreach (var entry in targetScenes)
            {
                if (entry.sceneName == sceneName)
                {
                    exists = true;
                    break;
                }
            }

            if (exists)
                continue;

            Vector2 autoRegisteredSpawnPosition = GetAutoRegisteredSpawnPosition(sceneName);
            targetScenes.Add(new JumpscareSceneData
            {
                sceneName = sceneName,
                spawnPosition = autoRegisteredSpawnPosition,
                spawnChance = defaultSpawnChanceForMokotanScenes
            });
        }
    }

    private Vector2 GetAutoRegisteredSpawnPosition(string sceneName)
    {
        foreach (var entry in autoRegisteredSpawnPositionOverrides)
        {
            if (string.Equals(entry.sceneName, sceneName, StringComparison.Ordinal))
                return entry.spawnPosition;
        }

        return defaultAutoRegisteredSpawnPosition;
    }

    private void Start()
    {
        InitBlinkMaterial();
        FindAndBindVolume();
        FitBlinkOverlayToScreen();
        FitGameOverOverlayToScreen();

        // triggerObject의 SpriteRenderer, Collider2D 캐시
        if (triggerObject != null)
        {
            triggerSpriteRenderer = triggerObject.GetComponent<SpriteRenderer>();
            triggerCollider = triggerObject.GetComponent<Collider2D>();
        }

        ApplyUnlitTriggerMaterialIfNeeded();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAndBindVolume();
        ResetJumpscareState();
        FitBlinkOverlayToScreen();
        FitGameOverOverlayToScreen();

        bool isTargetScene = false;

        foreach (var data in targetScenes)
        {
            if (data.sceneName == scene.name)
            {
                isTargetScene = true;
                float effectiveSpawnChance = IsRightHallScene(scene.name) ? data.spawnChance : NonRightHallSpawnChancePercent;
                float randomValue = UnityEngine.Random.Range(0f, 100f);
                if (randomValue <= effectiveSpawnChance)
                {
                    SpawnTrigger(data.spawnPosition);
                }
                else if (guaranteedJumpscareAfterSeconds > 0f)
                {
                    StartCoroutine(GuaranteedSpawnAfterStay(scene.name, data.spawnPosition));
                }
                break;
            }
        }

        // targetScene이 아니면 점프스케어 관련 오브젝트를 완전히 숨김
        if (!isTargetScene)
        {
            HideAllJumpscareObjects();
        }
    }

    private static bool IsRightHallScene(string sceneName)
    {
        return string.Equals(sceneName, RightHallSceneName, StringComparison.Ordinal);
    }

    private IEnumerator GuaranteedSpawnAfterStay(string expectedSceneName, Vector2 spawnPosition)
    {
        yield return new WaitForSeconds(guaranteedJumpscareAfterSeconds);
        if (hasTriggered)
            yield break;
        if (SceneManager.GetActiveScene().name != expectedSceneName)
            yield break;
        SpawnTrigger(spawnPosition);
    }

    /// <summary>
    /// 눈깜빡임 오버레이 Sprite를 카메라 화면 전체를 덮도록 크기를 조절합니다.
    /// Sprite를 카메라 바로 앞에 배치합니다.
    /// </summary>
    private void FitBlinkOverlayToScreen()
    {
        if (blinkOverlay == null) return;
        FitFullscreenSpriteRendererToMainCamera(blinkOverlay);
    }

    /// <summary>
    /// 게임오버 전면 스프라이트를 현재 Main Camera ortho 뷰에 맞춥니다 (DontDestroyOnLoad 시 씬별 카메라 대응).
    /// </summary>
    private void FitGameOverOverlayToScreen()
    {
        if (gameOverObject == null) return;
        SpriteRenderer sr = gameOverObject.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        FitFullscreenSpriteRendererToMainCamera(sr);
    }

    private void FitFullscreenSpriteRendererToMainCamera(SpriteRenderer sr)
    {
        if (sr == null) return;

        mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 camPos = mainCam.transform.position;
        sr.transform.position = new Vector3(camPos.x, camPos.y, camPos.z + OverlayPlaneZOffsetFromCamera);

        float worldHeight = mainCam.orthographicSize * 2f;
        float worldWidth = worldHeight * mainCam.aspect;

        if (sr.sprite != null)
        {
            Vector2 spriteSize = sr.sprite.bounds.size;
            sr.transform.localScale = new Vector3(
                worldWidth / spriteSize.x,
                worldHeight / spriteSize.y,
                1f
            );
        }
    }

    /// <summary>
    /// 현재 씬에서 Global Volume을 찾아 DepthOfField를 바인딩합니다.
    /// </summary>
    private void FindAndBindVolume()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        Volume[] allVolumes = FindObjectsByType<Volume>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        globalVolume = null;
        dof = null;

        // isGlobal이 true이고 DepthOfField를 가진 Volume을 우선 탐색
        foreach (var v in allVolumes)
        {
            if (v.isGlobal && v.profile != null && v.profile.TryGet(out DepthOfField foundDof))
            {
                globalVolume = v;
                dof = foundDof;
                break;
            }
        }

        // isGlobal Volume에서 못 찾았으면, 아무 Volume이라도 DoF가 있는 것을 사용
        if (dof == null)
        {
            foreach (var v in allVolumes)
            {
                if (v.profile != null && v.profile.TryGet(out DepthOfField foundDof))
                {
                    globalVolume = v;
                    dof = foundDof;
                    break;
                }
            }
        }

        if (globalVolume != null && dof != null)
        {
            dof.active = true;
            dof.gaussianMaxRadius.overrideState = true;
            dof.gaussianMaxRadius.value = 0f;
        }
        else
        {
            Debug.LogWarning($"[JumpscareManager] 씬 '{sceneName}'에서 DepthOfField를 가진 Volume을 찾지 못했습니다!");
        }
    }

    private void InitBlinkMaterial()
    {
        if (blinkOverlay != null && blinkOverlay.material != null)
        {
            // 인스턴스 Material 생성 (원본 Material을 오염시키지 않기 위함)
            blinkOverlay.material = new Material(blinkOverlay.material);
            blinkOverlay.material.SetFloat(blinkAmountProp, 0.5f);
        }
    }

    private void ResetJumpscareState()
    {
        isBlinkSequenceRunning = false;
        StopAllCoroutines();
        hasTriggered = false;
        isJumpscareInProgress = false;

        SetTriggerVisible(false);
        if (jumpscareAnimator != null) jumpscareAnimator.gameObject.SetActive(false);
        if (gameOverObject != null) gameOverObject.SetActive(false);

        if (blinkOverlay != null && blinkOverlay.material != null)
            blinkOverlay.material.SetFloat(blinkAmountProp, 0.5f);
        if (dof != null)
            dof.gaussianMaxRadius.value = 0f;

        SetHideObjectsByTag(false);
        SetMainCanvasVisible(true);

        // 이전 점프스케어에서 꺼놓은 SayDialog 복원
        RestoreSayDialog();

        OnJumpscareReset?.Invoke(); // 이 줄을 추가합니다.
    }

    private void SpawnTrigger(Vector2 spawnPos)
    {
        // HideAllJumpscareObjects로 꺼졌을 수 있으므로 먼저 활성화
        if (triggerObject != null) triggerObject.SetActive(true);
        if (blinkOverlay != null) blinkOverlay.gameObject.SetActive(true);

        if (triggerObject != null)
        {
            float worldZ = GetTriggerWorldPlaneZ();

            // 👇 카메라 기준 상대 좌표를 계산하도록 수정된 부분입니다.
            if (mainCam == null)
            {
                mainCam = Camera.main;
            }
            
            // 카메라 중심 위치 가져오기
            Vector2 cameraCenter = mainCam != null ? (Vector2)mainCam.transform.position : Vector2.zero;

            // 카메라 중심 X, Y에 인스펙터에서 설정한 spawnPos(오프셋)를 더합니다.
            float wx = cameraCenter.x + spawnPos.x;
            float wy = cameraCenter.y + spawnPos.y;

            triggerObject.transform.position = new Vector3(wx, wy, worldZ);
        }

        SetTriggerVisible(true);
        OnEnemyAppeared?.Invoke(); // 이 줄을 추가합니다.

        SetHideObjectsByTag(true);
        SetMainCanvasVisible(false);

    #if UNITY_EDITOR
        if (logTriggerRenderingAfterSpawn && triggerObject != null)
            StartCoroutine(DebugLogTriggerRenderingState());
    #endif

        StartCoroutine(WaitAndExecuteScare());
    }

    /// <summary>
    /// 메인 카메라와 동일한 2D 스프라이트 평면(일반적으로 z=0)에 맞춘 트리거 월드 Z.
    /// </summary>
    private float GetTriggerWorldPlaneZ()
    {
        if (mainCam == null)
            mainCam = Camera.main;
        if (mainCam == null)
            return 0f;
        return mainCam.transform.position.z + triggerWorldZOffsetFromCamera;
    }

    private void ApplyUnlitTriggerMaterialIfNeeded()
    {
        if (!useUnlitMaterialForTrigger || triggerSpriteRenderer == null)
            return;

        var shader = Shader.Find(SpriteUnlitShaderName);
        if (shader == null)
        {
            Debug.LogWarning($"[JumpscareManager] 셰이더를 찾을 수 없습니다: '{SpriteUnlitShaderName}'. 트리거 머티리얼을 바꾸지 않습니다.");
            return;
        }

        triggerSpriteRenderer.material = new Material(shader);
    }

#if UNITY_EDITOR
    private IEnumerator DebugLogTriggerRenderingState()
    {
        yield return null;

        if (triggerSpriteRenderer == null || triggerObject == null)
            yield break;

        var cam = Camera.main;
        if (cam == null)
        {
            Debug.Log("[JumpscareManager] DebugTrigger: Main Camera 없음");
            yield break;
        }

        Vector3 vp = cam.WorldToViewportPoint(triggerObject.transform.position);
    }
#endif

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (Camera.main == null) return;
        if (isJumpscareInProgress) return; // 점프스케어 진행 중 클릭 차단

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit == null) return;

        // 트리거 오브젝트 클릭 감지
        if (!hasTriggered && triggerObject != null
            && triggerCollider != null && triggerCollider.enabled
            && hit.gameObject == triggerObject)
        {
            ExecuteJumpscare();
            return;
        }

        // 리트라이 클릭 감지
        if (retryClickObject != null && retryClickObject.activeSelf
            && hit.gameObject == retryClickObject)
        {
            SceneManager.LoadScene(retrySceneName);
        }
    }

    private IEnumerator WaitAndExecuteScare()
    {
        yield return new WaitForSeconds(waitTimeToScare);
        ExecuteJumpscare();
    }

    public void ExecuteJumpscare()
    {
        if (hasTriggered) return;
        hasTriggered = true;

        StopAllCoroutines();

        // 클릭 차단 시작
        isJumpscareInProgress = true;

        // SayDialog 비활성화
        DisableSayDialog();

        // triggerObject를 끄지 않고, 보이는 부분만 숨김
        SetTriggerVisible(false);

        // 점프스케어 애니메이터를 트리거가 있던 위치에 배치 (월드 좌표)
        jumpscareAnimator.transform.position = triggerObject.transform.position;

        StartCoroutine(FullJumpscareSequence());
    }

    private IEnumerator FullJumpscareSequence()
    {
        // 눈 감기
        yield return StartCoroutine(AnimateBlink(0.5f, 0f, 0f, 2.0f, blinkDuration));
        yield return new WaitForSeconds(closedDuration);

        // Animator 활성화 & 재생 시작
        jumpscareAnimator.gameObject.SetActive(true);
        jumpscareAnimator.SetTrigger("Scare");

        // 눈 뜨기
        yield return StartCoroutine(AnimateBlink(0f, 0.5f, 2.0f, 0f, blinkDuration));
    }

    /// <summary>
    /// Animation Event에서 호출하는 메서드입니다.
    /// 애니메이션 클립의 2컷, 3컷, 4컷 시작 키프레임에 이벤트를 배치하세요.
    /// </summary>
    public void OnFrameTransition()
    {
        if (isBlinkSequenceRunning) return;
        StartCoroutine(FrameTransitionBlink());
    }

    private IEnumerator FrameTransitionBlink()
    {
        isBlinkSequenceRunning = true;

        yield return StartCoroutine(AnimateBlink(0.5f, 0f, 0f, 2.0f, blinkDuration));
        yield return new WaitForSeconds(closedDuration);
        yield return StartCoroutine(AnimateBlink(0f, 0.5f, 2.0f, 0f, blinkDuration));

        isBlinkSequenceRunning = false;
    }

    private IEnumerator AnimateBlink(float bStart, float bEnd, float blStart, float blEnd, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (blinkOverlay != null && blinkOverlay.material != null)
                blinkOverlay.material.SetFloat(blinkAmountProp, Mathf.Lerp(bStart, bEnd, t));

            if (dof != null)
                dof.gaussianMaxRadius.value = Mathf.Lerp(blStart, blEnd, t);

            yield return null;
        }

        if (blinkOverlay != null && blinkOverlay.material != null)
            blinkOverlay.material.SetFloat(blinkAmountProp, bEnd);

        if (dof != null)
            dof.gaussianMaxRadius.value = blEnd;
    }

    public void OnJumpscareFinished()
    {
        jumpscareAnimator.gameObject.SetActive(false);
        FitGameOverOverlayToScreen();
        if (gameOverObject != null) gameOverObject.SetActive(true);
        OnPlayerDied?.Invoke();

        // GameOver 표시 후 클릭 차단 해제
        isJumpscareInProgress = false;
    }

    /// <summary>
    /// triggerObject 자체는 활성 상태를 유지하면서,
    /// SpriteRenderer와 Collider2D만 켜고 끕니다.
    /// (자식인 Jumpscare 오브젝트에 영향을 주지 않기 위함)
    /// </summary>
    private void SetTriggerVisible(bool visible)
    {
        if (triggerSpriteRenderer != null) triggerSpriteRenderer.enabled = visible;
        if (triggerCollider != null) triggerCollider.enabled = visible;
    }

    private void SetHideObjectsByTag(bool hide)
    {
        if (string.IsNullOrEmpty(hideObjectTag)) return;

        GameObject[] targets = GameObject.FindGameObjectsWithTag(hideObjectTag);
        foreach (var obj in targets)
        {
            if (obj != null)
                obj.SetActive(!hide);
        }
    }

    /// <summary>
    /// 복도 등 일부 씬에서 MainCanvas가 Screen Space Overlay로 적 위를 덮는 문제를 방지합니다.
    /// 점프스케어 중에는 MainCanvas 태그 UI를 잠시 끄고, 종료/리셋 시 원복합니다.
    /// </summary>
    private void SetMainCanvasVisible(bool visible)
    {
        if (!visible)
        {
            hiddenMainCanvases.Clear();
            GameObject[] canvases;
            try
            {
                canvases = GameObject.FindGameObjectsWithTag(MainCanvasTag);
            }
            catch (UnityException)
            {
                return;
            }

            foreach (var canvas in canvases)
            {
                if (canvas == null || !canvas.activeSelf)
                    continue;

                canvas.SetActive(false);
                hiddenMainCanvases.Add(canvas);
            }
            return;
        }

        for (int i = 0; i < hiddenMainCanvases.Count; i++)
        {
            GameObject canvas = hiddenMainCanvases[i];
            if (canvas != null)
                canvas.SetActive(true);
        }
        hiddenMainCanvases.Clear();
    }

    /// <summary>
    /// 씬에서 Fungus SayDialog를 찾아 비활성화합니다.
    /// </summary>
    private void DisableSayDialog()
    {
        // Fungus의 SayDialog는 "SayDialog" 이름으로 찾을 수 있음
        if (sayDialogObject == null)
            sayDialogObject = GameObject.Find("SayDialog");

        if (sayDialogObject != null && sayDialogObject.activeSelf)
            sayDialogObject.SetActive(false);
    }

    /// <summary>
    /// 비활성화했던 SayDialog를 다시 켭니다.
    /// </summary>
    private void RestoreSayDialog()
    {
        if (sayDialogObject != null && !sayDialogObject.activeSelf)
            sayDialogObject.SetActive(true);
        sayDialogObject = null;
    }

    /// <summary>
    /// targetScene이 아닌 씬에서 점프스케어 관련 오브젝트를 완전히 숨깁니다.
    /// triggerObject 자체를 비활성화하여 자식(Jumpscare 등)까지 모두 숨깁니다.
    /// </summary>
    private void HideAllJumpscareObjects()
    {
        if (triggerObject != null) triggerObject.SetActive(false);
        if (jumpscareAnimator != null) jumpscareAnimator.gameObject.SetActive(false);
        if (gameOverObject != null) gameOverObject.SetActive(false);
        if (blinkOverlay != null) blinkOverlay.gameObject.SetActive(false);
    }
}