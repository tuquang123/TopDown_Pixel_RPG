using System.Collections.Generic;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public GameObject itemUIPrefab;
    public Transform contentParent;
    public Inventory playerInventory;

    public void SetupShop(List<ItemData> items)
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject); // Xoá UI cũ
        }

        foreach (var item in items)
        {
            var uiObj = Instantiate(itemUIPrefab, contentParent);
            var shopItemUI = uiObj.GetComponent<ShopItemUI>();
            shopItemUI.Setup(item, this);
        }
    }

    public void BuyItem(ItemData item)
    {
        CurrencyManager.Instance.SpendGold(item.price);
        
        playerInventory.AddItem(item);
        Debug.Log($"Đã mua {item.itemName}");
    }
}