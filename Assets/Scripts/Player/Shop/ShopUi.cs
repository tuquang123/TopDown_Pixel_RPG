using System.Collections.Generic;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public GameObject itemUIPrefab;
    public Transform contentParent;
    public Inventory playerInventory;
    public InventoryUI inventoryUI;

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
        // Nếu đã có trong inventory thì không mua lại
        if (playerInventory.HasItem(item))
        {
            Debug.Log($"{item.itemName} đã mua rồi.");
            return;
        }
        
        CurrencyManager.Instance.SpendGold(item.price);
        playerInventory.AddItem(item);
        inventoryUI.UpdateInventoryUI(); // cập nhật UI nếu có
        Debug.Log($"Đã mua {item.itemName}");
    }

}