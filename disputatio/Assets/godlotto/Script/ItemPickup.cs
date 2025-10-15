using UnityEngine;
using UnityEngine.EventSystems;
using Fungus; // Fungus를 사용하기 위해 이 줄을 추가합니다.

public class ItemPickup : MonoBehaviour, IPointerClickHandler
{
    public Item item;

    [Header("Fungus 연동 (선택사항)")]
    public Flowchart targetFlowchart; // 이벤트를 보낼 Flowchart
    public string fungusVariableName; // True로 바꿀 변수 이름

    public void OnPointerClick(PointerEventData eventData)
    {
        PickUp();
    }

    void PickUp()
    {
        // 1. Fungus 변수를 True로 설정합니다.
        if (targetFlowchart != null && !string.IsNullOrEmpty(fungusVariableName))
        {
            targetFlowchart.SetBooleanVariable(fungusVariableName, true);
            Debug.Log($"Fungus 변수 '{fungusVariableName}'을(를) True로 설정했습니다.");
        }
        
        // 2. 인벤토리에 아이템을 추가합니다.
        InventoryManager.instance.AddItem(item);
        
        // 3. 오브젝트를 파괴합니다.
        Destroy(gameObject);
    }
}