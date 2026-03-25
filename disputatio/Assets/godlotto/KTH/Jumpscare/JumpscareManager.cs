using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering; 
using UnityEngine.Rendering.Universal; 

[System.Serializable]
public struct JumpscareSceneData
{
    public string sceneName;        // 씬 이름
    public Vector2 spawnPosition;   // 등장 위치
    [Range(0f, 100f)]
    public float spawnChance;       // 해당 씬에서의 등장 확률
}

public class JumpscareManager : MonoBehaviour
{
    public static JumpscareManager Instance;

    [Header("씬별 설정 목록")]
    public List<JumpscareSceneData> targetScenes;

    [Header("효과 설정 (셰이더 & 블러)")]
    public Image blinkImage; 
    public Volume globalVolume;

    [Header("시간 설정")]
    public float waitTimeToScare = 3f;
    public float animationDuration = 2f;
    public float blinkDuration = 0.2f; // [추가됨] 눈 깜빡임 속도
    public float closedDuration = 0.1f; // [추가됨] 눈 감고 있는 시간
    public string retrySceneName = "MainScene";

    [Header("UI 할당")]
    public RectTransform triggerButtonRect;
    public Animator jumpscareAnimator;
    public GameObject gameOverPanel;
    public Button retryButton;

    private bool hasTriggered = false;
    
    // [추가됨] 포스트 프로세싱 및 셰이더 변수
    private DepthOfField dof; 
    private readonly int blinkAmountProp = Shader.PropertyToID("_BlinkAmount");

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // [추가됨] 시작할 때 머티리얼 복제 및 블러 효과 0으로 초기화
        if (blinkImage != null && blinkImage.material != null)
        {
            blinkImage.material = new Material(blinkImage.material);
            blinkImage.material.SetFloat(blinkAmountProp, 0.5f);
        }
        if (globalVolume != null && globalVolume.profile.TryGet(out dof))
        {
            dof.gaussianMaxRadius.value = 0f;
        }
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
        ResetJumpscareState();

        // 현재 씬 데이터 찾기
        foreach (var data in targetScenes)
        {
            if (data.sceneName == scene.name)
            {
                float randomValue = Random.Range(0f, 100f);
                if (randomValue <= data.spawnChance)
                {
                    SpawnTriggerSprite(data.spawnPosition);
                }
                break;
            }
        }
    }

    private void ResetJumpscareState()
    {
        StopAllCoroutines();
        hasTriggered = false;
        if (triggerButtonRect != null) triggerButtonRect.gameObject.SetActive(false);
        if (jumpscareAnimator != null) jumpscareAnimator.gameObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // [추가됨] 씬이 이동하거나 리셋될 때 화면을 다시 맑게(눈 뜬 상태) 되돌림
        if (blinkImage != null && blinkImage.material != null)
            blinkImage.material.SetFloat(blinkAmountProp, 0.5f);
        if (dof != null)
            dof.gaussianMaxRadius.value = 0f;
    }

    private void SpawnTriggerSprite(Vector2 spawnPos)
    {
        triggerButtonRect.anchoredPosition = spawnPos;
        triggerButtonRect.gameObject.SetActive(true);

        Button btn = triggerButtonRect.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(ExecuteJumpscare);
        }

        StartCoroutine(WaitAndExecuteScare());
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
        triggerButtonRect.gameObject.SetActive(false);

        // 애니메이션 위치를 씬 데이터에 맞춰 이동
        RectTransform animRect = jumpscareAnimator.GetComponent<RectTransform>();
        animRect.anchoredPosition = triggerButtonRect.anchoredPosition;

        // [수정됨] 직접 애니메이션을 켜는 대신, 눈 깜빡임 시퀀스 실행
        StartCoroutine(FullJumpscareSequence());
    }

    // [추가됨] 눈 감기 -> 애니메이션 켜기 -> 눈 뜨기 통합 시퀀스
    private IEnumerator FullJumpscareSequence()
    {
        yield return StartCoroutine(AnimateBlink(0.5f, 0f, 0f, 2.0f, blinkDuration));
        yield return new WaitForSeconds(closedDuration);
        
        jumpscareAnimator.gameObject.SetActive(true);
        jumpscareAnimator.SetTrigger("Scare");
        
        yield return StartCoroutine(AnimateBlink(0f, 0.5f, 2.0f, 0f, blinkDuration));
    }

    // [추가됨] 셰이더와 포스트 프로세싱 수치를 서서히 변경하는 함수
    private IEnumerator AnimateBlink(float bStart, float bEnd, float blStart, float blEnd, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            if (blinkImage != null && blinkImage.material != null)
                blinkImage.material.SetFloat(blinkAmountProp, Mathf.Lerp(bStart, bEnd, t));
            
            if (dof != null) 
                dof.gaussianMaxRadius.value = Mathf.Lerp(blStart, blEnd, t);
                
            yield return null;
        }

        if (blinkImage != null && blinkImage.material != null)
            blinkImage.material.SetFloat(blinkAmountProp, bEnd);
        
        if (dof != null) 
            dof.gaussianMaxRadius.value = blEnd;
    }

    public void OnJumpscareFinished()
    {
        jumpscareAnimator.gameObject.SetActive(false);
        gameOverPanel.SetActive(true);

        retryButton.onClick.RemoveAllListeners();
        retryButton.onClick.AddListener(() => SceneManager.LoadScene(retrySceneName));
    }
}