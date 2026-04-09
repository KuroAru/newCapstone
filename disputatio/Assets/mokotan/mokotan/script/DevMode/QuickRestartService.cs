using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuickRestartService : MonoBehaviour
{
    private bool isRestartInProgress;

    public void TriggerRestart()
    {
        if (isRestartInProgress)
            return;

        StartCoroutine(RestartCurrentSceneAsync());
    }

    private IEnumerator RestartCurrentSceneAsync()
    {
        isRestartInProgress = true;
        string activeSceneName = SceneManager.GetActiveScene().name;
        AsyncOperation op = SceneManager.LoadSceneAsync(activeSceneName);
        while (op != null && !op.isDone)
            yield return null;
        isRestartInProgress = false;
    }
}
