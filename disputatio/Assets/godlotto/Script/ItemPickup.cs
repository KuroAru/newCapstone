using UnityEngine;
using UnityEngine.EventSystems;
using Fungus;

public class ItemPickup : MonoBehaviour, IPointerClickHandler
{
    [Header("아이템 데이터")]
    public Item item;

    [Header("Fungus 연동 (선택사항)")]
    [Tooltip("비우면 FlowchartLocator(Variablemanager)를 사용합니다.")]
    [SerializeField] private Flowchart targetFlowchart;
    public string fungusVariableName;
    public string executeBlockName;

    public void OnPointerClick(PointerEventData eventData)
    {
        PickUp();
    }

    [ContextMenu("PickUp (Manual Test)")]
    public void PickUpDirect()
    {
        PickUp();
    }

    private void PickUp()
    {
        Flowchart fc = FlowchartLocator.Resolve(targetFlowchart);
        if (fc != null)
        {
            if (!string.IsNullOrEmpty(fungusVariableName))
            {
                fc.SetBooleanVariable(fungusVariableName, true);
                Debug.Log($"[ItemPickup] Fungus 변수 '{fungusVariableName}' → True");
            }

            if (!string.IsNullOrEmpty(executeBlockName))
            {
                fc.ExecuteBlock(executeBlockName);
                Debug.Log($"[ItemPickup] Fungus 블록 '{executeBlockName}' 실행");
            }
        }

        if (InventoryManager.instance != null && item != null)
        {
            InventoryManager.instance.AddItem(item);
            Debug.Log($"[ItemPickup] {item.name} 아이템을 인벤토리에 추가했습니다.");
        }
        else
        {
            Debug.LogWarning("[ItemPickup] InventoryManager.instance 또는 item이 null입니다!");
        }

        Destroy(gameObject);
    }
}
