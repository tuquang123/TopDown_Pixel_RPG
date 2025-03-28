using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public Inventory inventory; // Tham chiếu đến hệ thống Inventory
    public Equipment equipmentManager; // Tham chiếu đến hệ thống trang bị

    public GameObject itemPrefab; // Prefab của item trong UI
    public Transform itemContainer; // Nơi chứa danh sách item
    public PlayerStats playerStats;
    public EquipmentUI equipmentUI; 

    private void Start()
    {
        UpdateInventoryUI();
    }

    // Cập nhật giao diện Inventory
    public void UpdateInventoryUI()
    {
        // Xóa hết item cũ trước khi cập nhật
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        // Tạo UI mới cho từng item
        foreach (var item in inventory.items)
        {
            GameObject newItem = Instantiate(itemPrefab, itemContainer);
            ItemUI itemUI = newItem.GetComponent<ItemUI>();
            itemUI.Setup(item, this);
        }
    }

    // Gọi khi bấm "Trang bị"
    public void EquipItem(ItemData item)
    {
        equipmentManager.EquipItem(item, playerStats);
        equipmentUI.EquipItem(item); // Cập nhật UI Equipment
        inventory.RemoveItem(item);
        UpdateInventoryUI();
    }

    

}