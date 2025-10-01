using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition; // 드래그 시작 전 원래 위치

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>(); // CanvasGroup이 없다면 추가
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f; // 드래그 시작 시 살짝 투명하게
        canvasGroup.blocksRaycasts = false; // 드래그 중인 오브젝트가 다른 오브젝트 감지를 막지 않도록 함
        originalPosition = rectTransform.anchoredPosition; // 시작 위치 저장
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 마우스 위치를 따라 UI가 움직이도록 함 (Canvas 배율에 맞춰 보정)
        rectTransform.anchoredPosition += eventData.delta / transform.root.localScale.x;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f; // 드래그 종료 시 원래 투명도로
        canvasGroup.blocksRaycasts = true; // 레이캐스트 다시 활성화

        // 만약 드롭 영역에 놓이지 않았다면 원래 위치로 되돌리기
        if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponent<DropZone>() == null)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}