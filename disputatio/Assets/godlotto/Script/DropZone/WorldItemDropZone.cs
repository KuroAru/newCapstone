using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Fungus;

public class WorldItemDropZone : MonoBehaviour, IDropHandler
{
    [Header("필요한 아이템")]
    public Item requiredItem;

    [Header("성공 시 실행될 이벤트")]
    public UnityEvent onUnlock;

    [Header("Fungus 연동")]
    [Tooltip("비우면 FlowchartLocator(Variablemanager)를 사용합니다.")]
    public Flowchart flowchart;
    [Tooltip("이 Bool이 true면 대사 중으로 보고 드롭을 막습니다.")]
    public string dialogBoolName = "isTalking";

    public void OnDrop(PointerEventData eventData)
    {
        TryApplyDroppedItem(InventorySlot.draggedItem);
    }

    /// <summary>
    /// UI(IDropHandler)·인벤 슬롯(월드 레이캐스트) 공통: 올바른 아이템이고 대사 중이 아니면 소비합니다.
    /// </summary>
    /// <returns>실제로 사용 처리(이벤트·제거·비활성)를 했으면 true</returns>
    public bool TryApplyDroppedItem(Item dropped)
    {
        if (!CanUseWhileDialog())
            return false;

        if (dropped == null)
            return false;

        if (dropped != requiredItem)
        {
            Debug.Log("잘못된 아이템입니다.");
            return false;
        }

        Debug.Log($"올바른 아이템({requiredItem.itemName})을 사용했습니다!");

        onUnlock.Invoke();

        if (InventoryManager.instance != null)
            InventoryManager.instance.RemoveItem(requiredItem);

        enabled = false;
        return true;
    }

    private bool CanUseWhileDialog()
    {
        Flowchart fc = FlowchartLocator.Resolve(flowchart);
        if (fc == null || string.IsNullOrEmpty(dialogBoolName))
            return true;

        if (fc.GetBooleanVariable(dialogBoolName))
        {
            Debug.Log("대사가 진행 중이라 아이템을 사용할 수 없습니다.");
            return false;
        }

        return true;
    }
}
