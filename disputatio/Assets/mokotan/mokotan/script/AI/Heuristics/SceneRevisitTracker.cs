using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneRevisitTracker : MonoBehaviour
{
    private class SceneVisitInfo
    {
        public int unsolvedRevisitCount;
        public int noProgressAfterRevisitCount;
        public float lastVisitedAtUnscaledTime;
        public float revisitIntervalSeconds;
        public bool wasSolvedOnLastVisit;
    }

    private static SceneRevisitTracker instance;
    private readonly Dictionary<string, SceneVisitInfo> visitByScene = new Dictionary<string, SceneVisitInfo>();

    public static SceneRevisitTracker Instance => EnsureInstance();

    private static SceneRevisitTracker EnsureInstance()
    {
        if (instance != null)
            return instance;

        var go = new GameObject("SceneRevisitTracker");
        instance = go.AddComponent<SceneRevisitTracker>();
        DontDestroyOnLoad(go);
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
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
        string sceneName = scene.name;
        if (string.IsNullOrEmpty(sceneName))
            return;

        bool solvedNow = PuzzleSolvedStateProvider.IsSolved(sceneName);

        if (!visitByScene.TryGetValue(sceneName, out SceneVisitInfo info))
        {
            info = new SceneVisitInfo
            {
                lastVisitedAtUnscaledTime = Time.unscaledTime,
                wasSolvedOnLastVisit = solvedNow,
            };
            visitByScene[sceneName] = info;
            return;
        }

        float now = Time.unscaledTime;
        float interval = now - info.lastVisitedAtUnscaledTime;
        info.lastVisitedAtUnscaledTime = now;
        info.revisitIntervalSeconds = Mathf.Max(0f, interval);

        if (solvedNow)
        {
            info.unsolvedRevisitCount = 0;
            info.noProgressAfterRevisitCount = 0;
            info.wasSolvedOnLastVisit = true;
            return;
        }

        info.unsolvedRevisitCount += 1;
        if (!info.wasSolvedOnLastVisit)
            info.noProgressAfterRevisitCount += 1;

        info.wasSolvedOnLastVisit = false;
    }

    public HeuristicSignalInput FillRevisitSignals(HeuristicSignalInput input, string sceneName = null)
    {
        string targetScene = string.IsNullOrEmpty(sceneName) ? SceneManager.GetActiveScene().name : sceneName;
        if (string.IsNullOrEmpty(targetScene))
            return input;

        if (!visitByScene.TryGetValue(targetScene, out SceneVisitInfo info))
            return input;

        input.unsolvedRevisitCount = info.unsolvedRevisitCount;
        input.revisitIntervalSeconds = info.revisitIntervalSeconds;
        input.noProgressAfterRevisitCount = info.noProgressAfterRevisitCount;
        return input;
    }

    public IReadOnlyDictionary<string, (int unsolvedRevisitCount, float revisitIntervalSeconds, int noProgressAfterRevisitCount)> GetSnapshot()
    {
        var result = new Dictionary<string, (int, float, int)>();
        foreach (var pair in visitByScene)
        {
            result[pair.Key] = (
                pair.Value.unsolvedRevisitCount,
                pair.Value.revisitIntervalSeconds,
                pair.Value.noProgressAfterRevisitCount
            );
        }

        return result;
    }
}
