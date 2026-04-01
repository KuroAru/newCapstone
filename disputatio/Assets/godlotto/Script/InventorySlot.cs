using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 드래그 앤 드롭을 위해 반드시 필요합니다!
using Fungus;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI Components")]
    public Image icon;
    private Item item;

    [Header("Drag & Drop")]
    private static GameObject dragIcon; // 드래그 시 마우스를 따라다닐 아이콘 (static으로 하나만 존재)
    public static Item draggedItem;     // 현재 드래그 중인 아이템 (static으로 공유)

    public void AddItem(Item newItem)
    {
        item = newItem;
        icon.sprite = item.icon;
        icon.enabled = true;

        Button button = GetComponent<Button>();
        if (button != null) button.interactable = true;
    }

    public void ClearSlot()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = false;

        Button button = GetComponent<Button>();
        if (button != null) button.interactable = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item != null)
        {
            // 현재 드래그 중인 아이템 저장
            draggedItem = item;

            // 마우스를 따라다닐 드래그 아이콘 생성
            dragIcon = new GameObject("DragIcon");
            dragIcon.transform.SetParent(transform.root, false); 
            dragIcon.transform.SetAsLastSibling();
            
            var image = dragIcon.AddComponent<Image>();
            image.sprite = icon.sprite;
            image.raycastTarget = false; // 다른 UI 클릭 방해 X

            dragIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 아이콘이 있다면 파괴합니다.
        if (dragIcon != null)
        {
            Destroy(dragIcon);
        }

        // 1. UI 드롭존이 먼저 처리했는지 확인합니다 (기존 기능).
        if (eventData.pointerEnter != null && eventData.pointerEnter.GetComponent<DropZone>() != null)
        {
            // DropZone.cs가 OnDrop을 실행할 것이므로, 여기서는 아무것도 할 필요가 없습니다.
            draggedItem = null;
            return;
        }

        // 2. UI가 아니라면, 2D 월드에 드롭했는지 확인합니다.
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPosition, Vector2.zero);

        if (hit.collider != null)
        {
            WorldItemDropZone worldDropZone = hit.collider.GetComponent<WorldItemDropZone>();
            if (worldDropZone != null)
                worldDropZone.TryApplyDroppedItem(draggedItem);
        }

        // 드롭에 성공했든 실패했든 static 변수를 초기화합니다.
        draggedItem = null;
    }

    public void OnSlotClicked()
    {
        if (item != null)
        {
            InventoryManager.instance.SelectItem(item);
        }
    }
}
