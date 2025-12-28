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

    #region SHOW / FILTER

    public override void Show()
    {
        base.Show();
        SetupShop(allShopItems);
        detailPopup?.Setup(this);
    }

    public void OnFilterAll()        => FilterShop(null);
    public void OnFilterWeapon()     => FilterShop(ItemType.Weapon);
    public void OnFilterHelmet()     => FilterShop(ItemType.Helmet);
    public void OnFilterArmor()      => FilterShop(ItemType.SpecialArmor);
    public void OnFilterBoots()      => FilterShop(ItemType.Boots);
    public void OnFilterBody()       => FilterShop(ItemType.Clother);
    public void OnFilterPet()        => FilterShop(ItemType.Horse);
    public void OnFilterHair()       => FilterShop(ItemType.Hair);
    public void OnFilterCloak()      => FilterShop(ItemType.Cloak);

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

            // 🔥 ĐÃ MUA → KHÔNG HIỆN
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

        // đã sở hữu
        if (IsItemOwned(data))
            return;

        // không đủ vàng
        if (!CurrencyManager.Instance.SpendGold(data.price))
        {
            GameEvents.OnShowToast.Raise("Gold not enough!");
            return;
        }

        // thêm vào inventory
        var newInstance = new ItemInstance(data);
        playerInventory.AddItem(newInstance);
        inventoryUI.UpdateInventoryUI();

        // 🔥 BUILD LẠI SHOP NGAY
        SetupShop(allShopItems);

        GameEvents.OnShowToast.Raise("Success purchase Item!");
    }

    #endregion
}
