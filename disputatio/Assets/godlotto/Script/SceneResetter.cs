using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneResetter : MonoBehaviour
{
    public string mainMenuSceneName = "MainMenuScene"; // 메인 메뉴 씬 이름 수정하세요

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        CleanupDontDestroyObjects();
    }

    private void CleanupDontDestroyObjects()
    {
        // 임시 오브젝트를 만들어 DontDestroyOnLoad 영역을 참조
        GameObject temp = new GameObject("TempSceneProbe");
        DontDestroyOnLoad(temp);

        var dontDestroyScene = temp.scene;
        Destroy(temp);

        List<GameObject> roots = new List<GameObject>();
        dontDestroyScene.GetRootGameObjects(roots);

        foreach (GameObject obj in roots)
        {
            Destroy(obj);
        }

        Debug.Log("모든 DontDestroyOnLoad 오브젝트 삭제 완료");
    }
}
