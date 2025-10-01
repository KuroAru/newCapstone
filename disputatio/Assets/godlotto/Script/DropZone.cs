using UnityEngine;
using UnityEngine.EventSystems;
// Button 타입을 직접 사용하지 않으므로 UnityEngine.UI는 없어도 됩니다.

public class DropZone : MonoBehaviour, IDropHandler
{
    [Header("회전 버튼들")]
    // Button 타입 대신 GameObject 타입으로 변경하여 활성화/비활성화를 직접 제어합니다.
    public GameObject rotateRightButtonObject;
    public GameObject rotateLeftButtonObject;

    void Start()
    {
        // 게임이 시작될 때 회전 버튼들을 화면에서 완전히 숨깁니다.
        if (rotateRightButtonObject != null)
        {
            rotateRightButtonObject.SetActive(false);
        }
        if (rotateLeftButtonObject != null)
        {
            rotateLeftButtonObject.SetActive(false);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();
        if (draggable != null)
        {
            draggable.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
            draggable.transform.localScale = Vector3.one;

            // 드롭이 성공하면 버튼들을 화면에 다시 표시합니다.
            if (rotateRightButtonObject != null)
            {
                rotateRightButtonObject.SetActive(true);
            }
            if (rotateLeftButtonObject != null)
            {
                rotateLeftButtonObject.SetActive(true);
            }
        }
    }
}