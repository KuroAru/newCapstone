using UnityEngine;

/// <summary>
/// DDOL 싱글톤 자리표시자. 설정 패널 표시는 <see cref="InGameSettingsPanel"/> / <see cref="IntegratedSettingUI"/>가 담당합니다.
/// </summary>
public class PersistentObjectManager : MonoBehaviour
{
    public static PersistentObjectManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
