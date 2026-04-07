using System.Collections.Generic;
using UnityEngine;
using Fungus;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("Inventory Data")]
    public List<Item> items = new List<Item>();
    public Item selectedItem;

    [Header("Inventory UI")]
    public GameObject inventoryUI_Background;
    public Transform slotsHolder;
    public GameObject slotPrefab;
    public int maxSlots = 12;
    [SerializeField] private Flowchart targetflowchart;

    public bool pressTab = false;

    private List<InventorySlot> slots = new List<InventorySlot>();
    private Animator animator;
    private bool isOpen = false;

    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;

        targetflowchart = ResolveFlowchart();
    }

    void Start()
    {
        animator = inventoryUI_Background.GetComponent<Animator>();
        inventoryUI_Background.SetActive(false);

        if (ResolveFlowchart() != null)
            pressTab = targetflowchart.GetBooleanVariable("pressTab");

        CreateSlots();
        UpdateUI();
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Tab))
            return;

        isOpen = !isOpen;
        if (isOpen)
        {
            pressTab = true;
            SyncPressTab();
            inventoryUI_Background.SetActive(true);
            animator.SetTrigger("Open");
        }
        else
        {
            pressTab = false;
            SyncPressTab();
            animator.SetTrigger("Close");

            if (selectedItem != null)
                DeselectItem();
        }
    }

    public void AddItem(Item item)
    {
        if (item == null)
            return;

        if (ResolveFlowchart() != null)
            ItemAcquisitionTracker.MarkAcquired(targetflowchart, item);

        if (items.Count >= maxSlots)
        {
            Debug.Log($"인벤토리가 가득 찼습니다! {item.itemName}을(를) 더 이상 추가할 수 없습니다.");
            return;
        }

        items.Add(item);
        UpdateUI();
    }

    public void RemoveItem(Item item)
    {
        if (item == null)
            return;

        items.Remove(item);

        if (selectedItem == item)
            DeselectItem();

        UpdateUI();
    }

    public void SelectItem(Item item)
    {
        if (selectedItem == item)
        {
            DeselectItem();
            return;
        }

        selectedItem = item;
        Debug.Log($"{item.itemName} 을(를) 손에 들었다.");
    }

    public void DeselectItem()
    {
        selectedItem = null;
        Debug.Log("손에 든 아이템을 내려놓았다.");
    }

    private void CreateSlots()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotsHolder);
            slots.Add(slotGO.GetComponent<InventorySlot>());
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Count)
                slots[i].AddItem(items[i]);
            else
                slots[i].ClearSlot();
        }
    }

    private void SyncPressTab()
    {
        if (ResolveFlowchart() != null)
            targetflowchart.SetBooleanVariable("pressTab", pressTab);
    }

    private Flowchart ResolveFlowchart()
    {
        if (targetflowchart == null)
            targetflowchart = FlowchartLocator.Resolve(null);

        return targetflowchart;
    }
}
