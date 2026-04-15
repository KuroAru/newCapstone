using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    [Header("필요한 아이템")]
    public Item requiredItem;

    [Header("상호작용 오브젝트")]
    public GameObject filterCardObject;
    public GameObject rotateRightButtonObject;
    public GameObject rotateLeftButtonObject;

    [Header("사용 횟수 설정")]
    public int maxUses = 2;
    private int currentUses = 0;

    void Start()
    {
        // 시작 시 버튼들을 화면에서 숨깁니다.
        if (rotateRightButtonObject != null) rotateRightButtonObject.SetActive(false);
        if (rotateLeftButtonObject != null) rotateLeftButtonObject.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 드롭된 아이템이 올바른지 확인
        if (InventorySlot.draggedItem == requiredItem)
        {
            if (currentUses < maxUses)
            {
                // 사용 횟수를 1 증가시킵니다.
                currentUses++;
                GameLog.Log(requiredItem.itemName + " 아이템을 사용했습니다. (" + currentUses + "/" + maxUses + ")");

                if (filterCardObject != null)
                {
                    filterCardObject.SetActive(true);
                    var rt = filterCardObject.GetComponent<RectTransform>();
                    if (rt != null)
                        rt.anchoredPosition = Vector2.zero;
                    filterCardObject.transform.localScale = Vector3.one;
                }

                if (rotateRightButtonObject != null) rotateRightButtonObject.SetActive(true);
                if (rotateLeftButtonObject != null) rotateLeftButtonObject.SetActive(true);

                if (currentUses >= maxUses)
                {
                    GameLog.Log("마지막 사용! 인벤토리에서 아이템을 제거합니다.");
                    if (InventoryManager.instance != null)
                        InventoryManager.instance.RemoveItem(requiredItem);
                }
            }
            else
            {
                GameLog.Log(requiredItem.itemName + " 아이템은 더 이상 사용할 수 없습니다.");
            }
        }
    }
}