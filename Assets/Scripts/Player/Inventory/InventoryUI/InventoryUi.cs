using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class InventoryUI : BasePopup
{
    [Header("Inventory UI")] public Transform itemContainer;
    Inventory inventory;
    public GameObject itemPrefab;
    public EquipmentUI equipmentUi;
    public ItemDetailPanel itemDetailPanel;
    [Header("Auto Equip")] public Button autoEquipButton;

    [Header("Filter Buttons")] public List<FilterButtonUI> filterButtons = new List<FilterButtonUI>();
    private FilterButtonUI currentFilter;
    [Header("Sell All")] public Button sellAllButton;
    [Header("Confirm Popup")] public ConfirmPopup confirmPopup;
    [Header("Unequip All")] public Button unequipAllButton;

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
            .OrderBy(i => i.itemData.tier) // 🔥 cùi → vip
            .ThenBy(i => i.itemData.price) // cùng tier → rẻ trước
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
        {
            autoEquipButton.onClick.AddListener(AutoEquipBestItems);
            autoEquipText.text = "Auto Equip";
        }

        if (sellAllButton != null)
        {
            sellAllButton.onClick.AddListener(OnClickSellAll);
            sellAllText.text = "Sell All";
        }

        if (unequipAllButton != null)
        {
            unequipAllButton.onClick.AddListener(UnequipAllItems);
            unequipAllText.text = "Unequip All";
        }
    }

    public void Close()
    {
        UIManager.Instance.HidePopupByType(PopupType.Inventory);
    }

    public void AutoEquipBestItems()
    {
        if (inventory == null) return;

        float beforePower = PlayerStats.Instance.CurrentPower;

        Dictionary<ItemType, ItemInstance> bestItems = new();

        foreach (var item in inventory.items)
        {
            if (item.isLocked) continue;

            ItemType type = item.itemData.itemType;

            if (!bestItems.ContainsKey(type))
                bestItems[type] = item;
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
                    equipmentUi.EquipItem(bestItem);
            }
        }

        PlayerStats.Instance.CalculatePower();

        float afterPower = PlayerStats.Instance.CurrentPower;
        float diff = afterPower - beforePower;

        if (diff != 0)
        {
            string text;

            if (diff > 0)
            {
                text = $"<color=#00FF00>+{diff:N0} Chiến lực</color>";
            }
            else
            {
                text = $"<color=#FF4D4D>{diff:N0} Chiến lực</color>";
            }

            GameEvents.OnShowToast.Raise(text);
        }

        equipmentUi.UpdateEquipmentUI();
        UpdateInventoryUI();
    }
    
    public void OnClickSellAll()
    {
        if (inventory == null) return;

        int totalGold = 0;
        int itemCount = 0;

        foreach (var item in inventory.items)
        {
            if (equipmentUi.IsItemEquipped(item))
                continue;

            if (item.isLocked)
                continue;

            itemCount++;
            totalGold += CalculateSellPrice(item);
        }
        if (itemCount == 0)
        {
            GameEvents.OnShowToast.Raise("Không có đồ để bán");
            return;
        }
        UIManager.Instance.ShowPopupByType(PopupType.ItemConfirm);

        if (UIManager.Instance.TryGetPopup(PopupType.ItemConfirm, out var popup)
            && popup is ConfirmPopup confirm)
        {
            confirm.Show(
                "Sell All",
                $"Bán {itemCount} món đồ\nNhận <color=#FFD700>{totalGold:N0} vàng</color>?",
                SellAllItems
            );
        }
    }

    private void SellAllItems()
    {
        if (inventory == null) return;

        int totalGold = 0;
        List<ItemInstance> itemsToRemove = new List<ItemInstance>();

        foreach (var item in inventory.items)
        {
            if (equipmentUi.IsItemEquipped(item))
                continue;

            if (item.isLocked)
                continue;

            totalGold += CalculateSellPrice(item);
            itemsToRemove.Add(item);
        }

        if (itemsToRemove.Count == 0)
        {
            GameEvents.OnShowToast.Raise("Không có đồ để bán");
            return;
        }

        foreach (var item in itemsToRemove)
        {
            inventory.items.Remove(item);
        }

        CurrencyManager.Instance.AddGold(totalGold);

        GameEvents.OnShowToast.Raise($"Bán thành công\n+{totalGold:N0} vàng");

        UpdateInventoryUI();
    }

    private int CalculateSellPrice(ItemInstance item)
    {
        int baseValue = item.itemData.baseUpgradeCost;
        float multi = 0.6f + item.upgradeLevel * 0.2f;
        return Mathf.RoundToInt(baseValue * multi);
    }
    public void UnequipAllItems()
    {
        if (inventory == null) return;

        float beforePower = PlayerStats.Instance.CurrentPower;

        var equippedTypes = new List<ItemType>(
            equipmentUi.GetAllEquippedTypes()
        );

        foreach (var type in equippedTypes)
        {
            equipmentUi.UnequipItem(type);
        }

        PlayerStats.Instance.CalculatePower();

        float afterPower = PlayerStats.Instance.CurrentPower;
        float diff = afterPower - beforePower;

        if (diff != 0)
        {
            string text = diff > 0
                ? $"<color=#00FF88>+{diff:N0} Chiến lực</color>"
                : $"<color=#FF5555>{diff:N0} Chiến lực</color>";

            GameEvents.OnShowToast.Raise(text);
        }

        equipmentUi.UpdateEquipmentUI();
        UpdateInventoryUI();
    }

    public void RefreshCurrentSelectedItemLock()
    {
        if (currentSelectedItem != null)
        {
            currentSelectedItem.RefreshLockState();
        }
    }
    [SerializeField] private TMPro.TMP_Text autoEquipText;
    [SerializeField] private TMPro.TMP_Text sellAllText;
    [SerializeField] private TMPro.TMP_Text unequipAllText;
    
}
