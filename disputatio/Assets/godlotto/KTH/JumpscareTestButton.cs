using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필수적인 네임스페이스

public class JumpscareTestButton : MonoBehaviour
{
    // 버튼의 OnClick 이벤트에서 호출할 함수
    // 인스펙터 창에서 이동하고 싶은 씬 이름을 직접 입력할 수 있습니다.
    public void ChangeScene(string sceneName)
    {
        // 입력받은 이름의 씬을 로드합니다.
        SceneManager.LoadScene(sceneName);
    }
}