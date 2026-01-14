using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopUI : BasePopup
{
    [Header("UI")]
    public GameObject itemUIPrefab;
    public Transform contentParent;
    public ShopDetailPopup detailPopup;

    [Header("Data")]
    public Inventory playerInventory;
    public InventoryUI inventoryUI;
    public List<ItemData> allShopItems = new();

    private readonly List<ShopItemUI> shopItemUIs = new();

    // ================= FILTER HIGHLIGHT =================
    [Header("Filter Buttons")]
    public List<FilterButtonUI> filterButtons = new();

    private FilterButtonUI currentFilter;
    // ====================================================

    #region SHOW

    public override void Show()
    {
        base.Show();

        SetupShop(allShopItems);
        detailPopup?.Setup(this);

        // Auto chọn filter All
        if (filterButtons != null && filterButtons.Count > 0)
            SelectFilter(filterButtons[0]);
    }

    #endregion

    #region FILTER BUTTONS

    private void SelectFilter(FilterButtonUI btn)
    {
        if (currentFilter == btn)
            return;

        if (currentFilter != null)
            currentFilter.SetSelected(false);

        currentFilter = btn;

        if (currentFilter != null)
            currentFilter.SetSelected(true);
    }

    public void OnFilterAll(FilterButtonUI btn)
    {
        SelectFilter(btn);
        FilterShop(null);
    }

    public void OnFilterWeapon(FilterButtonUI btn)
    {
        SelectFilter(btn);
        FilterShop(ItemType.Weapon);
    }

    public void OnFilterHelmet(FilterButtonUI btn)
    {
        SelectFilter(btn);
        FilterShop(ItemType.Helmet);
    }

    public void OnFilterArmor(FilterButtonUI btn)
    {
        SelectFilter(btn);
        FilterShop(ItemType.SpecialArmor);
    }

    public void OnFilterBoots(FilterButtonUI btn)
    {
        SelectFilter(btn);
        FilterShop(ItemType.Boots);
    }

    public void OnFilterBody(FilterButtonUI btn)
    {
        SelectFilter(btn);
        FilterShop(ItemType.Clother);
    }

    public void OnFilterPet(FilterButtonUI btn)
    {
        SelectFilter(btn);
        FilterShop(ItemType.Horse);
    }

    public void OnFilterHair(FilterButtonUI btn)
    {
        SelectFilter(btn);
        FilterShop(ItemType.Hair);
    }

    public void OnFilterCloak(FilterButtonUI btn)
    {
        SelectFilter(btn);
        FilterShop(ItemType.Cloak);
    }

    public void FilterShop(ItemType? type)
    {
        var filteredItems = ItemFilter.FilterByType(allShopItems, type);
        SetupShop(filteredItems);
    }

    #endregion

    #region SETUP SHOP

    private void ClearShop()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        shopItemUIs.Clear();
    }

    private bool IsItemOwned(ItemData data)
    {
        return playerInventory.items.Any(i => i.itemData.itemID == data.itemID);
    }

    public void SetupShop(List<ItemData> items)
    {
        ClearShop();

        if (items == null || items.Count == 0)
            return;

        foreach (var item in items)
        {
            if (item == null)
                continue;

            if (IsItemOwned(item))
                continue;

            var uiObj = Instantiate(itemUIPrefab, contentParent);
            var shopItemUI = uiObj.GetComponent<ShopItemUI>();

            var tempInstance = new ItemInstance(item);
            shopItemUI.Setup(tempInstance, this);

            shopItemUIs.Add(shopItemUI);
        }
    }

    #endregion

    #region BUY

    public void BuyItem(ItemInstance instance)
    {
        if (instance == null || instance.itemData == null)
            return;

        var data = instance.itemData;

        if (IsItemOwned(data))
            return;

        if (!CurrencyManager.Instance.SpendGold(data.price))
        {
            GameEvents.OnShowToast.Raise("Gold not enough!");
            return;
        }

        var newInstance = new ItemInstance(data);
        playerInventory.AddItem(newInstance);
        inventoryUI.UpdateInventoryUI();

        SetupShop(allShopItems);

        GameEvents.OnShowToast.Raise("Success purchase Item!");
    }

    #endregion
}
