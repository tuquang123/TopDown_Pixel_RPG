using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopUI : BasePopup
{
    [Header("UI")]
    public GameObject itemUIPrefab;
    public Transform contentParent;
    public ShopDetailPopup detailPopup;
    private ItemType? currentFilterType = null;
    private bool isFirstShow = true;

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
        detailPopup?.Setup(this);

        if (isFirstShow)
        {
            isFirstShow = false;

            SelectFilter(filterButtons[0]);
            FilterShop(null); // 🔥 CHUẨN
        }
        else
        {
            FilterShop(currentFilterType);
        }
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
        SelectFilter(btn);          // (1)
        currentFilterType = null;   // (2) ALL = null
        FilterShop(null);           // (3)
    }


    public void OnFilterWeapon(FilterButtonUI btn)
    {
        SelectFilter(btn);                  // (1) Highlight nút
        currentFilterType = ItemType.Weapon; // (2) Lưu filter
        FilterShop(currentFilterType);       // (3) Apply filter
    }


    public void OnFilterHelmet(FilterButtonUI btn)
    {
        SelectFilter(btn);
        currentFilterType = ItemType.Helmet;
        FilterShop(currentFilterType);
    }

    public void OnFilterBoots(FilterButtonUI btn)
    {
        SelectFilter(btn);
        currentFilterType = ItemType.Boots;
        FilterShop(currentFilterType);
    }
    public void OnFilterArmor(FilterButtonUI btn)
    {
        SelectFilter(btn);
        currentFilterType = ItemType.SpecialArmor;
        FilterShop(currentFilterType);
    }

    public void OnFilterBody(FilterButtonUI btn)
    {
        SelectFilter(btn);
        currentFilterType = ItemType.Clother;
        FilterShop(currentFilterType);
    }
    public void OnFilterPet(FilterButtonUI btn)
    {
        SelectFilter(btn);
        currentFilterType = ItemType.Horse;
        FilterShop(currentFilterType);
    }

    public void OnFilterHair(FilterButtonUI btn)
    {
        SelectFilter(btn);
        currentFilterType = ItemType.Hair;
        FilterShop(currentFilterType);
    }

    public void OnFilterCloak(FilterButtonUI btn)
    {
        SelectFilter(btn);
        currentFilterType = ItemType.Cloak;
        FilterShop(currentFilterType);
    }

    public void FilterShop(ItemType? type)
    {
        currentFilterType = type; // 🔥 CHỐT STATE TẠI ĐÂY

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

        playerInventory.AddItem(new ItemInstance(data));
        inventoryUI.UpdateInventoryUI();

        // 🔥 GIỮ FILTER
        FilterShop(currentFilterType);

        GameEvents.OnShowToast.Raise("Success purchase Item!");
    }


    #endregion
}
