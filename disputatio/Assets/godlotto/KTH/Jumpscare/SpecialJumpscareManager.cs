using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SpecialJumpscareManager : MonoBehaviour
{
    public static SpecialJumpscareManager Instance;

    [Header("눈깜빡임 오버레이 (SpriteRenderer)")]
    [Tooltip("카메라 앞에 배치할 전체화면 눈깜빡임 Sprite")]
    public SpriteRenderer blinkOverlay;

    [Header("효과 설정 (블러)")]
    public Volume globalVolume;

    [Header("시간 및 확률 설정")]
    public float waitTimeToScare = 60f;
    [Range(0f, 100f)]
    public float spawnChance = 100f;
    public float blinkDuration = 0.2f;
    public float closedDuration = 0.1f;
    public string retrySceneName = "MainScene";

    [Header("오브젝트")]
    public GameObject parrotObject;
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

    private static bool hasVisitedSpecialScene = false;
    private bool hasTriggered = false;
    private DepthOfField dof;
    private readonly int blinkAmountProp = Shader.PropertyToID("_BlinkAmount");
    private bool isBlinkSequenceRunning = false;

    // 점프스케어 진행 중 클릭 차단
    private bool isJumpscareInProgress = false;
    // 비활성화한 Canvas 목록 (GameOver 후 필요하면 복원용)
    private List<Canvas> disabledCanvases = new List<Canvas>();

    private Camera mainCam;

    // triggerObject의 보이는 부분만 끄기 위한 캐시
    private SpriteRenderer triggerSpriteRenderer;
    private Collider2D triggerCollider;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // 인스턴스 Material 생성
        if (blinkOverlay != null && blinkOverlay.material != null)
        {
            blinkOverlay.material = new Material(blinkOverlay.material);
            blinkOverlay.material.SetFloat(blinkAmountProp, 0.5f);
        }

        FitBlinkOverlayToScreen();

        // triggerObject의 SpriteRenderer, Collider2D 캐시
        if (triggerObject != null)
        {
            triggerSpriteRenderer = triggerObject.GetComponent<SpriteRenderer>();
            triggerCollider = triggerObject.GetComponent<Collider2D>();
        }

        // globalVolume이 Inspector에서 할당되지 않은 경우 씬에서 자동 탐색
        if (globalVolume == null)
            globalVolume = FindFirstObjectByType<Volume>();

        if (globalVolume != null && globalVolume.profile.TryGet(out dof))
            dof.gaussianMaxRadius.value = 0f;

        jumpscareAnimator.gameObject.SetActive(false);
        if (gameOverObject != null) gameOverObject.SetActive(false);

        if (!hasVisitedSpecialScene)
        {
            float randomValue = Random.Range(0f, 100f);
            if (randomValue <= spawnChance)
            {
                hasVisitedSpecialScene = true;
                SetupEnemyState(true);
            }
            else ShowParrotOnly();
        }
        else ShowParrotOnly();
    }

    /// <summary>
    /// 눈깜빡임 오버레이 Sprite를 카메라 화면 전체를 덮도록 크기를 조절합니다.
    /// </summary>
    private void FitBlinkOverlayToScreen()
    {
        if (blinkOverlay == null) return;

        mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 camPos = mainCam.transform.position;
        blinkOverlay.transform.position = new Vector3(camPos.x, camPos.y, camPos.z + 1f);

        float worldHeight = mainCam.orthographicSize * 2f;
        float worldWidth = worldHeight * mainCam.aspect;

        if (blinkOverlay.sprite != null)
        {
            Vector2 spriteSize = blinkOverlay.sprite.bounds.size;
            blinkOverlay.transform.localScale = new Vector3(
                worldWidth / spriteSize.x,
                worldHeight / spriteSize.y,
                1f
            );
        }
    }

    private void SetupEnemyState(bool isPresent)
    {
        if (isPresent)
        {
            if (parrotObject != null) parrotObject.SetActive(false);
            SetTriggerVisible(true);

            SetHideObjectsByTag(true);

            StartCoroutine(WaitAndExecuteScare());
        }
    }

    private void ShowParrotOnly()
    {
        if (parrotObject != null) parrotObject.SetActive(true);
        SetTriggerVisible(false);

        SetHideObjectsByTag(false);
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
            SceneManager.LoadScene("Hall_playerble");
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

        // 씬의 모든 Canvas 비활성화
        DisableAllCanvases();

        // triggerObject를 끄지 않고, 보이는 부분만 숨김
        SetTriggerVisible(false);

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
    /// 2컷, 3컷, 4컷 시작 키프레임에 이벤트를 배치하세요.
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
        if (gameOverObject != null) gameOverObject.SetActive(true);

        // GameOver 표시 후 클릭 차단 해제 (리트라이 등 클릭 가능)
        isJumpscareInProgress = false;
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
    /// 씬에 있는 모든 활성 Canvas를 찾아 비활성화합니다.
    /// </summary>
    private void DisableAllCanvases()
    {
        disabledCanvases.Clear();
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var canvas in allCanvases)
        {
            if (canvas.gameObject.activeSelf)
            {
                disabledCanvases.Add(canvas);
                canvas.gameObject.SetActive(false);
            }
        }
    }
}