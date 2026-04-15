using System.Collections.Generic;
using UnityEngine;
using Fungus;

public class InventoryManager : SingletonMonoBehaviour<InventoryManager>
{
    protected override bool PersistAcrossScenes => true;

    [System.Obsolete("Use Instance instead.")]
    public static InventoryManager instance => Instance;

    [Header("Inventory Data")]
    [SerializeField] private List<Item> items = new List<Item>();
    [SerializeField] private Item selectedItem;

    public IReadOnlyList<Item> Items => items;
    public Item SelectedItem => selectedItem;

    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryUI_Background;
    [SerializeField] private Transform slotsHolder;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private int maxSlots = 12;
    [SerializeField] private Flowchart targetflowchart;
    [SerializeField] private InventoryTooltipController tooltipController;

    [SerializeField] private bool pressTab = false;

    private List<InventorySlot> slots = new List<InventorySlot>();
    private Animator animator;
    private bool isOpen = false;

    protected override void OnSingletonAwake()
    {
        targetflowchart = ResolveFlowchart();
        var fallback = FindFirstObjectByType<InventoryTooltipController>(FindObjectsInactive.Include);
        if (tooltipController == null && fallback != null)
            GameLog.LogWarning($"[{nameof(InventoryManager)}] tooltipController resolved via FindFirstObjectByType — assign in Inspector for faster startup.");
        tooltipController = SelectTooltipController(tooltipController, fallback);
    }

    void OnEnable()
    {
        SaveManagerSignals.OnSaveReset += OnSaveReset;
        SaveManagerSignals.OnSavePointLoaded += OnSavePointLoaded;
    }

    void OnDisable()
    {
        SaveManagerSignals.OnSaveReset -= OnSaveReset;
        SaveManagerSignals.OnSavePointLoaded -= OnSavePointLoaded;
    }

    void Start()
    {
        animator = inventoryUI_Background.GetComponent<Animator>();
        inventoryUI_Background.SetActive(false);

        if (ResolveFlowchart() != null)
            pressTab = targetflowchart.GetBooleanVariable(FungusVariableKeys.PressTab);

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
            GameLog.Log($"인벤토리가 가득 찼습니다! {item.itemName}을(를) 더 이상 추가할 수 없습니다.");
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
        GameLog.Log($"{item.itemName} 을(를) 손에 들었다.");
    }

    public void DeselectItem()
    {
        selectedItem = null;
        GameLog.Log("손에 든 아이템을 내려놓았다.");
    }

    public void ShowTooltip(Item item, Vector2 screenPosition)
    {
        if (tooltipController == null || item == null)
            return;

        tooltipController.Show(item, screenPosition);
    }

    public void HideTooltip()
    {
        if (tooltipController == null)
            return;

        tooltipController.Hide();
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
            targetflowchart.SetBooleanVariable(FungusVariableKeys.PressTab, pressTab);
    }

    private void OnSaveReset()
    {
        items.Clear();
        if (selectedItem != null)
            DeselectItem();
        UpdateUI();
    }

    private void OnSavePointLoaded(string savePointKey)
    {
        RestoreInventoryFromFlowchart();
    }

    private void RestoreInventoryFromFlowchart()
    {
        Flowchart fc = FlowchartLocator.Find();
        if (fc == null) return;

        string raw = fc.GetStringVariable(FungusVariableKeys.InventoryItemIds);
        items.Clear();
        if (selectedItem != null)
            DeselectItem();

        if (!string.IsNullOrEmpty(raw))
        {
            Item[] allItems = Resources.FindObjectsOfTypeAll<Item>();
            foreach (string idStr in raw.Split(','))
            {
                if (int.TryParse(idStr, out int id))
                {
                    Item found = System.Array.Find(allItems, x => x.itemId == id);
                    if (found != null)
                        items.Add(found);
                    else
                        GameLog.LogWarning($"[InventoryManager] 복원 실패: itemId={id} 에 해당하는 Item을 찾을 수 없습니다.");
                }
            }
        }

        UpdateUI();
    }

    private Flowchart ResolveFlowchart()
    {
        if (targetflowchart == null)
            targetflowchart = FlowchartLocator.Resolve(null);

        return targetflowchart;
    }

    public static InventoryTooltipController SelectTooltipController(
        InventoryTooltipController assignedController,
        InventoryTooltipController discoveredController)
    {
        return assignedController != null ? assignedController : discoveredController;
    }
}
