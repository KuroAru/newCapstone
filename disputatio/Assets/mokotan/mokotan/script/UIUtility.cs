using UnityEngine;
using UnityEngine.EventSystems;

public class UIUtility : MonoBehaviour
{
    /// <summary>
    /// EventSystem의 현재 선택된 오브젝트를 해제(null)합니다.
    /// </summary>
    public void ClearEventSystemSelection()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }
}