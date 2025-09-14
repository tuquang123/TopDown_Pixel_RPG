using System.Collections.Generic;
using UnityEngine;

public class ShopUI : BasePopup
{
    public GameObject itemUIPrefab;
    public Transform contentParent;
    public Inventory playerInventory;
    public InventoryUI inventoryUI;
    private List<ShopItemUI> shopItemUIs = new();

    public override void Show()
    {
        base.Show();
        RefreshShopUI(); 
    }

    public void SetupShop(List<ItemData> items)
    {
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
