using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public List<ItemData> shopItems; // Danh sách item được bán
    public ShopUI shopUI;

    private void Start()
    {
        // Gán danh sách shopItems vào CurrencyManager
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.shopItems = shopItems;
        }
        else
        {
            Debug.LogError("CurrencyManager.Instance is null in ShopManager.");
        }
    }
}