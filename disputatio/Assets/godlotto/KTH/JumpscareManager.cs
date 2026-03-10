using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public struct JumpscareSceneData
{
    public string sceneName;        // 씬 이름
    public Vector2 spawnPosition;   // 등장 위치
    [Range(0f, 100f)]
    public float spawnChance;       // 해당 씬에서의 등장 확률 (추가됨)
}

public class JumpscareManager : MonoBehaviour
{
    public static JumpscareManager Instance;

    [Header("씬별 설정 목록")]
    public List<JumpscareSceneData> targetScenes;

    [Header("시간 설정")]
    public float waitTimeToScare = 3f;
    public float animationDuration = 2f;
    public string retrySceneName = "MainScene";

    [Header("UI 할당")]
    public RectTransform triggerButtonRect;
    public Animator jumpscareAnimator;
    public GameObject gameOverPanel;
    public Button retryButton;

    private bool hasTriggered = false;

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
                // 공통 확률이 아니라 데이터에 들어있는 'spawnChance'를 사용
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

        RectTransform animRect = jumpscareAnimator.GetComponent<RectTransform>();
        animRect.anchoredPosition = triggerButtonRect.anchoredPosition;

        jumpscareAnimator.gameObject.SetActive(true);
        jumpscareAnimator.SetTrigger("Scare");
    }

    // 애니메이션 마지막 프레임에서 호출될 함수
    public void OnJumpscareFinished()
    {
        jumpscareAnimator.gameObject.SetActive(false);
        gameOverPanel.SetActive(true);

        retryButton.onClick.RemoveAllListeners();
        retryButton.onClick.AddListener(() => SceneManager.LoadScene(retrySceneName));
    }
}