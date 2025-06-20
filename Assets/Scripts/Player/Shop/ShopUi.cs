using System.Collections.Generic;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public GameObject itemUIPrefab;
    public Transform contentParent;
    public Inventory playerInventory;
    public InventoryUI inventoryUI;
    private List<ShopItemUI> shopItemUIs = new List<ShopItemUI>();

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
            shopItemUI.Setup(item, this);
            shopItemUIs.Add(shopItemUI);
        }
    }

    public void BuyItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogError("ItemData is null in BuyItem.");
            return;
        }

        ItemInstance itemInstance = new ItemInstance(item);

        foreach (var invItem in playerInventory.items)
        {
            if (invItem.itemData.itemID == item.itemID)
            {
                Debug.Log($"{item.itemName} đã mua rồi.");
                return;
            }
        }

        if (CurrencyManager.Instance.SpendGold(item.price))
        {
            playerInventory.AddItem(itemInstance);
            inventoryUI.UpdateInventoryUI();
            RefreshShopUI();
            Debug.Log($"Đã mua {item.itemName} với giá {item.price} vàng.");
            GameEvents.OnShowToast.Raise("Succes purchar Item!");
        }
        else
        {
            Debug.Log("Không đủ vàng để mua vật phẩm.");
            GameEvents.OnShowToast.Raise("gold not enough!");
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