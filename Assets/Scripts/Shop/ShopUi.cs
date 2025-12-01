using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopUI : BasePopup
{
    public GameObject itemUIPrefab;
    public Transform contentParent;
    public Inventory playerInventory;
    public InventoryUI inventoryUI;
    private List<ShopItemUI> shopItemUIs = new();
    public ShopDetailPopup detailPopup; 
    public List<ItemData> allShopItems = new();
    
    public void FilterShop(ItemType? type)
    {
        // Xóa UI hiện tại
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        shopItemUIs.Clear();

        // Sử dụng ItemFilter
        var filteredItems = ItemFilter.FilterByType(allShopItems, type);

        // Build UI
        SetupShop(filteredItems);
    }

    
    public void OnFilterWeapon()   => FilterShop(ItemType.Weapon);
    public void OnFilterHelmet()   => FilterShop(ItemType.Helmet);
    public void OnFilterArmor()    => FilterShop(ItemType.SpecialArmor);
    public void OnFilterBoots()    => FilterShop(ItemType.Boots);
    public void OnFilterBody()     => FilterShop(ItemType.Clother);
    public void OnFilterPet()      => FilterShop(ItemType.Horse);
    public void OnFilterHair() => FilterShop(ItemType.Hair);
    public void OnFilterCloak() => FilterShop(ItemType.Cloak);

    // nút hiện tất cả
    public void OnFilterAll()  => FilterShop(null);

    public override void Show()
    {
        base.Show();
        
        SetupShop(allShopItems);
        
        RefreshShopUI();

        detailPopup?.Setup(this);
    }


    public void SetupShop(List<ItemData> items)
    {
        
        /*if (allShopItems == null || allShopItems.Count == 0)
            allShopItems = new List<ItemData>(items);  */
        
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        shopItemUIs.Clear();

        if (items == null || items.Count == 0)
        {
            Debug.LogWarning("Danh sách vật phẩm cửa hàng rỗng hoặc null.");
            return;
        }

        foreach (var item in items)
        {
            if (item == null) continue;
            var uiObj = Instantiate(itemUIPrefab, contentParent);
            var shopItemUI = uiObj.GetComponent<ShopItemUI>();
            
            var tempInstance = new ItemInstance(item);  
            shopItemUI.Setup(tempInstance, this);

            shopItemUIs.Add(shopItemUI);
        }

    }

    public void BuyItem(ItemInstance instance)
    {
        if (instance == null || instance.itemData == null)
        {
            Debug.LogError("ItemInstance hoặc ItemData null trong BuyItem.");
            return;
        }

        var data = instance.itemData;
        
        foreach (var invItem in playerInventory.items)
        {
            if (invItem.itemData.itemID == data.itemID)
            {
                Debug.Log($"{data.itemName} đã mua rồi.");
                return;
            }
        }

        if (CurrencyManager.Instance.SpendGold(data.price))
        {
            var newInstance = new ItemInstance(data);

            playerInventory.AddItem(newInstance);
            inventoryUI.UpdateInventoryUI();
            RefreshShopUI();

            Debug.Log($"Đã mua {data.itemName} với giá {data.price} vàng.");
            GameEvents.OnShowToast.Raise("Succes purchase Item!");
        }
        else
        {
            Debug.Log("Không đủ vàng để mua vật phẩm.");
            GameEvents.OnShowToast.Raise("Gold not enough!");
        }
    }

    

    private void RefreshShopUI()
    {
        foreach (var shopItemUI in shopItemUIs)
        {
            shopItemUI.RefreshState();
        }
    }
}
