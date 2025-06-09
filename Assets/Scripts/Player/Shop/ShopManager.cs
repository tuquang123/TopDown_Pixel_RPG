using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public List<ItemData> shopItems; // Danh sách item được bán
    public ShopUI shopUI;

    private void Start()
    {
        shopUI.SetupShop(shopItems);
    }
}