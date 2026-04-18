using System.Collections;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SpecialJumpscareManager : SingletonMonoBehaviour<SpecialJumpscareManager>
{
    public static event Action OnPlayerDied;
    public static event Action OnEnemyAppeared; // 추가: 적이 스폰되었을 때
    public static event Action OnJumpscareReset; // 추가: 상태가 초기화될 때 (씬 이동 등)

    [Header("눈깜빡임 오버레이 (SpriteRenderer)")]
    [Tooltip("카메라 앞에 배치할 전체화면 눈깜빡임 Sprite")]
    public SpriteRenderer blinkOverlay;

    [Header("효과 설정 (블러)")]
    public Volume globalVolume;

    [Header("시간 및 확률 설정")]
    public float waitTimeToScare = 3f;
    [Range(0f, 100f)]
    public float spawnChance = 100f;
    public float blinkDuration = 0.2f;
    public float closedDuration = 0.1f;
    public string retrySceneName = SceneNames.MainScene;

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

    [Header("포스트 프로세싱")]
    [Tooltip("적이 등장하면 활성화될 ChromaticAberration")]
    private ChromaticAberration chromaticAberration;
    private Coroutine chromaticCoroutine;

    private static bool hasVisitedSpecialScene = false;
    private bool hasTriggered = false;
    private DepthOfField dof;
    private readonly int blinkAmountProp = Shader.PropertyToID("_BlinkAmount");
    private bool isBlinkSequenceRunning = false;

    // 점프스케어 진행 중 클릭 차단
    private bool isJumpscareInProgress = false;
    // 점프스케어 시 비활성화할 SayDialog
    private GameObject sayDialogObject;

    private Camera mainCam;

    // triggerObject의 보이는 부분만 끄기 위한 캐시
    private SpriteRenderer triggerSpriteRenderer;
    private Collider2D triggerCollider;

    void Start()
    {
        // 연결된 Volume의 Profile에서 Chromatic Aberration 컴포넌트 가져오기
        if (globalVolume != null && globalVolume.profile.TryGet(out chromaticAberration))
        {
            // 게임 시작 시 기본 상태를 꺼짐(0)으로 설정합니다.
            chromaticAberration.intensity.value = 0f; 
        }
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
            float randomValue = UnityEngine.Random.Range(0f, 100f);
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
            OnEnemyAppeared?.Invoke(); // 이 줄을 추가합니다.

            ChromaticOn();
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
        OnPlayerDied?.Invoke();
        OnJumpscareReset?.Invoke(); // 이 줄을 추가합니다.
        ChromaticOff();

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
    /// Chromatic Aberration 효과를 켜고 10초 주기로 1 -> 0 -> 1을 반복합니다.
    /// </summary>
    public void ChromaticOn()
    {
        if (chromaticAberration == null) return;

        // 이미 실행 중인 코루틴이 있다면 중복 실행을 막기 위해 정지
        if (chromaticCoroutine != null)
        {
            StopCoroutine(chromaticCoroutine);
        }
        
        chromaticCoroutine = StartCoroutine(ChromaticRoutine());
    }

    /// <summary>
    /// Chromatic Aberration 효과를 완전히 끕니다.
    /// </summary>
    public void ChromaticOff()
    {
        if (chromaticAberration == null) return;

        if (chromaticCoroutine != null)
        {
            StopCoroutine(chromaticCoroutine);
            chromaticCoroutine = null;
        }

        chromaticAberration.intensity.value = 0f;
    }

    private IEnumerator ChromaticRoutine()
    {
        float timer = 0f;

        while (true)
        {
            timer += Time.deltaTime;
            
            // 10초를 주기로 1 -> 0 -> 1이 되도록 Cosine 함수 사용
            // Mathf.Cos는 기본적으로 1 -> -1 -> 1로 변하므로, 이를 1 -> 0 -> 1로 정규화합니다.
            float intensity = 0.5f * Mathf.Cos((timer / 10f) * 2f * Mathf.PI) + 0.5f;
            
            chromaticAberration.intensity.value = intensity;

            yield return null;
        }
    }

    /// <summary>
    /// 씬에서 Fungus SayDialog를 찾아 비활성화합니다.
    /// </summary>
    private void DisableSayDialog()
    {
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
}