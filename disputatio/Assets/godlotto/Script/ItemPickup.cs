using UnityEngine;
using UnityEngine.EventSystems;
using Fungus;

public class ItemPickup : MonoBehaviour, IPointerClickHandler
{
    [Header("아이템 데이터")]
    public Item item;

    [Header("Fungus 연동 (선택사항)")]
    public Flowchart targetFlowchart;   // 이벤트를 보낼 Flowchart
    public string fungusVariableName;   // True로 바꿀 변수 이름 (기존 기능)
    public string executeBlockName;     // ★ 실행할 블록의 이름 (새로 추가됨)

    // Unity UI 클릭 이벤트로 호출됨
    public void OnPointerClick(PointerEventData eventData)
    {
        PickUp();
    }

    // Fungus나 다른 스크립트에서도 호출할 수 있는 공개 메서드
    [ContextMenu("PickUp (Manual Test)")]
    public void PickUpDirect()
    {
        PickUp();
    }

    // 실제 동작 로직
    private void PickUp()
    {
        // 1. Fungus 연동 처리
        if (targetFlowchart != null)
        {
            // A. 변수 설정 (기존 기능)
            if (!string.IsNullOrEmpty(fungusVariableName))
            {
                targetFlowchart.SetBooleanVariable(fungusVariableName, true);
                Debug.Log($"[ItemPickup] Fungus 변수 '{fungusVariableName}' → True");
            }

            // B. ★ 특정 블록 실행 (추가된 기능)
            if (!string.IsNullOrEmpty(executeBlockName))
            {
                targetFlowchart.ExecuteBlock(executeBlockName);
                Debug.Log($"[ItemPickup] Fungus 블록 '{executeBlockName}' 실행");
            }
        }

        // 2. 인벤토리 추가 (습득 마스크는 AddItem 내부에서 처리)
        if (InventoryManager.instance != null && item != null)
        {
            InventoryManager.instance.AddItem(item);
            Debug.Log($"[ItemPickup] {item.name} 아이템을 인벤토리에 추가했습니다.");
        }
        else
        {
            Debug.LogWarning("[ItemPickup] InventoryManager.instance 또는 item이 null입니다!");
        }

        // 3. 오브젝트 제거 (씬에서 사라지게)
        Destroy(gameObject);
    }
}