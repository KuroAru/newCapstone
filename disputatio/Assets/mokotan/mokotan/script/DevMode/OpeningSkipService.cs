using UnityEngine;
using UnityEngine.SceneManagement;

public class OpeningSkipService : MonoBehaviour
{
    [SerializeField] private string mainGameplaySceneName = "MainScene";

    public void SkipOpening()
    {
        if (string.IsNullOrEmpty(mainGameplaySceneName))
        {
            Debug.LogWarning("[OpeningSkipService] mainGameplaySceneName이 비어 있습니다.");
            return;
        }

        SceneManager.LoadScene(mainGameplaySceneName);
    }
}
