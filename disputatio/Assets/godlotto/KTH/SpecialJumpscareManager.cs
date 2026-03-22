using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering; 
using UnityEngine.Rendering.Universal; 

public class SpecialJumpscareManager : MonoBehaviour
{
    public static SpecialJumpscareManager Instance; 

    [Header("입력 및 효과 설정")]
    public CanvasGroup inputBlocker;
    public Image blinkImage; 
    public Volume globalVolume; 

    [Header("시간 및 확률 설정")]
    [Tooltip("적이 등장한 후 점프스케어까지 대기 시간")]
    public float waitTimeToScare = 3f; // 사라졌던 변수 부활!
    [Range(0f, 100f)]
    public float spawnChance = 100f;
    public float blinkDuration = 0.2f; 
    public float closedDuration = 0.1f; 
    public string retrySceneName = "MainScene";

    [Header("오브젝트 및 UI")]
    public GameObject parrotObject;
    public RectTransform triggerButtonRect;
    public Animator jumpscareAnimator;
    public GameObject gameOverPanel;
    public Button retryButton;

    private static bool hasVisitedSpecialScene = false;
    private bool hasTriggered = false;
    private DepthOfField dof;
    private readonly int blinkAmountProp = Shader.PropertyToID("_BlinkAmount");
    private GraphicRaycaster _canvasRaycaster;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        _canvasRaycaster = GetComponent<GraphicRaycaster>();
    }

    void Start()
    {
        // 초기화 로직
        if (blinkImage != null && blinkImage.material != null)
        {
            blinkImage.material = new Material(blinkImage.material);
            blinkImage.material.SetFloat(blinkAmountProp, 0.5f);
        }

        if (globalVolume != null && globalVolume.profile.TryGet(out dof))
        {
            dof.gaussianMaxRadius.value = 0f;
        }

        if (inputBlocker != null) inputBlocker.blocksRaycasts = false;
        // BlinkEffect는 전체 화면 + Raycast Target이라 CanvasGroup(InputBlocker)과 무관하게 클릭을 먹는다.
        SetBlinkRaycastBlocking(false);
        jumpscareAnimator.gameObject.SetActive(false);
        gameOverPanel.SetActive(false);

        // 첫 방문 체크
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

    private void SetupEnemyState(bool isPresent)
    {
        if (isPresent)
        {
            if (parrotObject != null) parrotObject.SetActive(false);
            triggerButtonRect.gameObject.SetActive(true);
            triggerButtonRect.GetComponent<Button>().onClick.AddListener(ExecuteJumpscare);
            
            // 이제 인스펙터의 waitTimeToScare 값을 사용합니다!
            StartCoroutine(WaitAndExecuteScare());
        }
    }

    private void ShowParrotOnly()
    {
        if (parrotObject != null) parrotObject.SetActive(true);
        triggerButtonRect.gameObject.SetActive(false);
        SetBlinkRaycastBlocking(false);
        // 점프스케어를 쓰지 않을 때는 이 오버레이 캔버스가 켜져 있어도 레이캐스트를 처리하지 않게 한다.
        if (_canvasRaycaster != null)
            _canvasRaycaster.enabled = false;
    }

    private IEnumerator WaitAndExecuteScare()
    {
        yield return new WaitForSeconds(waitTimeToScare); //
        ExecuteJumpscare();
    }

    public void ExecuteJumpscare()
    {
        if (hasTriggered) return;
        hasTriggered = true;
        StopAllCoroutines();
        triggerButtonRect.gameObject.SetActive(false);

        if (inputBlocker != null) inputBlocker.blocksRaycasts = true;
        if (_canvasRaycaster != null)
            _canvasRaycaster.enabled = true;
        StartCoroutine(FullJumpscareSequence());
    }

    private IEnumerator FullJumpscareSequence()
    {
        // 눈 감기 + 흐려지기
        yield return StartCoroutine(AnimateBlink(0.5f, 0f, 0f, 2.0f, blinkDuration));
        yield return new WaitForSeconds(closedDuration);
        
        // 눈 뜨기 + 선명해지기 (이때 애니메이션 시작)
        jumpscareAnimator.gameObject.SetActive(true);
        jumpscareAnimator.SetTrigger("Scare");
        yield return StartCoroutine(AnimateBlink(0f, 0.5f, 2.0f, 0f, blinkDuration));
    }

    private IEnumerator AnimateBlink(float bStart, float bEnd, float blStart, float blEnd, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            blinkImage.material.SetFloat(blinkAmountProp, Mathf.Lerp(bStart, bEnd, t));
            if (dof != null) dof.gaussianMaxRadius.value = Mathf.Lerp(blStart, blEnd, t);
            yield return null;
        }
    }

    // 애니메이션 종료 후 호출 (Retry 버튼 잠금 해제 포함)
    public void OnJumpscareFinished()
    {
        jumpscareAnimator.gameObject.SetActive(false);
        gameOverPanel.SetActive(true);
        
        // 중요: 여기서 입력 잠금을 풀어줘야 Retry 버튼이 눌립니다!
        if (inputBlocker != null) inputBlocker.blocksRaycasts = false;
        // InputBlocker만 끄면 Blink 전체화면 Image가 여전히 레이를 먹어서 하위 씬 UI(패널 등)가 막힌다.
        SetBlinkRaycastBlocking(false);

        retryButton.onClick.RemoveAllListeners();
        retryButton.onClick.AddListener(() => SceneManager.LoadScene(retrySceneName));
    }

    private void SetBlinkRaycastBlocking(bool block)
    {
        if (blinkImage != null)
            blinkImage.raycastTarget = block;
    }
}