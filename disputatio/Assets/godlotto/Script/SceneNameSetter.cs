using UnityEngine;
using Fungus;
using UnityEngine.SceneManagement;

public class SceneNameSetter : MonoBehaviour
{
    [Header("연결할 전역 Flowchart")]
    public Flowchart globalFlowchart;

    [Header("씬 이름을 기록할 전역 문자열 변수명")]
    public string sceneVarName = "SceneName";

    [Header("세이브 포인트 키로 사용할 전역 문자열 변수명")]
    public string savePointKeyVarName = "SavePointKey";

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        UpdateSceneVariables(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateSceneVariables(scene.name);
    }

    private void UpdateSceneVariables(string currentSceneName)
    {
        Flowchart fc = FlowchartLocator.Find();
        if (fc == null)
        {
            GameLog.LogWarning("[SceneNameSetter] Variablemanager Flowchart를 찾지 못했습니다.");
            return;
        }

        globalFlowchart = fc;

        // SceneName 갱신
        if (!string.IsNullOrEmpty(sceneVarName))
        {
            fc.SetStringVariable(sceneVarName, currentSceneName);
            GameLog.Log($"[SceneNameSetter] SceneName → '{currentSceneName}'");
        }

        // SavePointKey 갱신 (원래대로 _Start 포함)
        if (!string.IsNullOrEmpty(savePointKeyVarName))
        {
            string saveKey = currentSceneName + "_Start";
            fc.SetStringVariable(savePointKeyVarName, saveKey);
            GameLog.Log($"[SceneNameSetter] SavePointKey → '{saveKey}'");
        }
    }
}
