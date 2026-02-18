using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class InventoryUI : BasePopup
{
    [Header("Inventory UI")]
    public Transform itemContainer;
    Inventory inventory;
    public GameObject itemPrefab;
    public EquipmentUI equipmentUi;
    public ItemDetailPanel itemDetailPanel;
    [Header("Auto Equip")]
    public Button autoEquipButton;

    [Header("Filter Buttons")]
    public List<FilterButtonUI> filterButtons = new List<FilterButtonUI>();
    private FilterButtonUI currentFilter;
    [Header("Sell All")]
    public Button sellAllButton;
    [Header("Confirm Popup")]
    public ConfirmPopup confirmPopup;
    [Header("Unequip All")]
    public Button unequipAllButton;

    private ItemUI currentSelectedItem;
    
    public Inventory Inventory => inventory;

    #region FILTER LOGIC

    private void SelectFilter(FilterButtonUI btn)
    {
        if (currentFilter == btn)
            return;

        if (currentFilter != null)
            currentFilter.SetSelected(false);

        currentFilter = btn;
        currentFilter.SetSelected(true);
    }

    public void OnFilterAll(FilterButtonUI btn)
    {
        SelectFilter(btn);
        FilterInventory(null);
    }

    public void OnFilterWeapon(FilterButtonUI btn)
    {
        SelectFilter(btn);
        FilterInventory(ItemType.Weapon);
    }

    public void OnFilterHelmet(FilterButtonUI btn)
    {
        SelectFilter(btn);
        FilterInventory(ItemType.Helmet);
    }

    public void OnFilterArmor(FilterButtonUI btn)
    {
        SelectFilter(btn);
        FilterInventory(ItemType.SpecialArmor);
    }

    public void OnFilterBoots(FilterButtonUI btn)
    {
        SelectFilter(btn);
        FilterInventory(ItemType.Boots);
    }

    public void OnFilterBody(FilterButtonUI btn)
    {
        SelectFilter(btn);
        FilterInventory(ItemType.Clother);
    }

    public void OnFilterPet(FilterButtonUI btn)
    {
        SelectFilter(btn);
        FilterInventory(ItemType.Horse);
    }

    public void OnFilterHair(FilterButtonUI btn)
    {
        SelectFilter(btn);
        FilterInventory(ItemType.Hair);
    }

    public void OnFilterCloak(FilterButtonUI btn)
    {
        SelectFilter(btn);
        FilterInventory(ItemType.Cloak);
    }

    #endregion

    #region FILTER / INVENTORY

    public static class ItemFilter
    {
        public static List<ItemData> FilterByType(List<ItemData> items, ItemType? type)
        {
            if (items == null) return new List<ItemData>();
            if (type == null) return new List<ItemData>(items);
            return items.Where(i => i.itemType == type).ToList();
        }

        public static List<ItemInstance> FilterInventoryByType(List<ItemInstance> items, ItemType? type)
        {
            if (items == null) return new List<ItemInstance>();
            if (type == null) return new List<ItemInstance>(items);
            return items.Where(i => i.itemData.itemType == type).ToList();
        }
    }
    public void FilterInventory(ItemType? type)
    {
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        var filteredItems = ItemFilter
            .FilterInventoryByType(inventory.items, type)
            .OrderBy(i => i.itemData.tier)     // 🔥 cùi → vip
            .ThenBy(i => i.itemData.price)     // cùng tier → rẻ trước
            .ToList();

        foreach (ItemInstance item in filteredItems)
        {
            GameObject newItem = Instantiate(itemPrefab, itemContainer);
            newItem.GetComponent<ItemUI>().Setup(item, this);
        }

        itemDetailPanel.Hide();
        currentSelectedItem = null;
    }
    

    #endregion

    #region SELECT ITEM

    public void SelectItem(ItemUI newItem)
    {
        if (currentSelectedItem != null)
            currentSelectedItem.SetSelected(false);

        currentSelectedItem = newItem;
        currentSelectedItem.SetSelected(true);

        // Hiển thị detail panel
        itemDetailPanel.gameObject.SetActive(true);

    }

    #endregion

    #region SHOW / HIDE

    public override void Show()
    {
        base.Show();
        inventory = CommonReferent.Instance.playerPrefab.GetComponent<Inventory>();

        UpdateInventoryUI();

        // Auto chọn filter All khi mở
        if (filterButtons != null && filterButtons.Count > 0)
        {
            SelectFilter(filterButtons[0]);
            FilterInventory(null); // 🔥 QUAN TRỌNG
        }
    }


    public override void Hide()
    {
        base.Hide();
        itemDetailPanel.Hide(); 
    }

    public void UpdateInventoryUI()
    {
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        foreach (ItemInstance item in inventory.items)
        {
            GameObject newItem = Instantiate(itemPrefab, itemContainer);
            newItem.GetComponent<ItemUI>().Setup(item, this);
        }

        itemDetailPanel.Hide();
        currentSelectedItem = null;
    }

    #endregion
    private void Start()
    {
        if (autoEquipButton != null)
            autoEquipButton.onClick.AddListener(AutoEquipBestItems);

        if (sellAllButton != null)
            sellAllButton.onClick.AddListener(SellAllItems);

        if (unequipAllButton != null)
            unequipAllButton.onClick.AddListener(UnequipAllItems);
    }

    public void Close()
    {
        UIManager.Instance.HidePopupByType(PopupType.Inventory);
    }
    public void AutoEquipBestItems()
    {
        if (inventory == null) return;

        // Bước 1: Tính trước best item cho từng loại
        Dictionary<ItemType, ItemInstance> bestItems = new();

        foreach (var item in inventory.items)
        {
            ItemType type = item.itemData.itemType;

            if (!bestItems.ContainsKey(type))
            {
                bestItems[type] = item;
            }
            else
            {
                ItemInstance currentBest = bestItems[type];

                bool isBetter =
                    item.itemData.tier > currentBest.itemData.tier ||
                    (item.itemData.tier == currentBest.itemData.tier &&
                     item.itemData.price > currentBest.itemData.price);

                if (isBetter)
                    bestItems[type] = item;
            }
        }

        // Bước 2: Equip sau khi đã chọn xong hết
        foreach (var kvp in bestItems)
        {
            ItemType type = kvp.Key;
            ItemInstance bestItem = kvp.Value;

            ItemInstance currentEquipped = equipmentUi.GetEquippedItem(type);

            if (currentEquipped == null)
            {
                equipmentUi.EquipItem(bestItem);
            }
            else
            {
                bool isBetter =
                    bestItem.itemData.tier > currentEquipped.itemData.tier ||
                    (bestItem.itemData.tier == currentEquipped.itemData.tier &&
                     bestItem.itemData.price > currentEquipped.itemData.price);

                if (isBetter)
                {
                    equipmentUi.EquipItem(bestItem);
                }
            }
        }

        equipmentUi.UpdateEquipmentUI();
        UpdateInventoryUI();
    }
    public void SellAllItems()
    {
        if (inventory == null) return;

        int totalGold = 0;

        // Copy list để tránh lỗi khi remove trong lúc duyệt
        List<ItemInstance> itemsToSell = new List<ItemInstance>(inventory.items);

        foreach (var item in itemsToSell)
        {
            // Bỏ qua nếu item đang được trang bị
            if (equipmentUi.IsItemEquipped(item))
                continue;

            totalGold += item.itemData.price;

            inventory.RemoveItem(item);
        }

        if (totalGold > 0)
        {
            CurrencyManager.Instance.AddGold(totalGold);
            FloatingTextSpawner.Instance.SpawnText("+" + totalGold, Vector3.zero, Color.yellow);
        }

        equipmentUi.UpdateEquipmentUI();
        UpdateInventoryUI();
    }
    public void UnequipAllItems()
    {
        if (inventory == null) return;

        // Copy danh sách type để tránh modify dictionary khi đang duyệt
        var equippedTypes = new List<ItemType>(equipmentUi
            .GetAllEquippedTypes());

        foreach (var type in equippedTypes)
        {
            equipmentUi.UnequipItem(type);
        }

        equipmentUi.UpdateEquipmentUI();
        UpdateInventoryUI();
    }


}
