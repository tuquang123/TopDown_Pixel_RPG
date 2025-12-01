using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryUI : BasePopup
{
    public Transform itemContainer;
    public Inventory inventory;
    public GameObject itemPrefab;
    public EquipmentUI equipmentUi;

    public ItemDetailPanel itemDetailPanel;

    public static class ItemFilter
    {
        // lọc theo ItemType, null = tất cả
        public static List<ItemData> FilterByType(List<ItemData> items, ItemType? type)
        {
            if (items == null) return new List<ItemData>();

            if (type == null) return new List<ItemData>(items);

            return items.Where(i => i.itemType == type).ToList();
        }

        // lọc inventory (ItemInstance)
        public static List<ItemInstance> FilterInventoryByType(List<ItemInstance> items, ItemType? type)
        {
            if (items == null) return new List<ItemInstance>();

            if (type == null) return new List<ItemInstance>(items);

            return items.Where(i => i.itemData.itemType == type).ToList();
        }
    }
    
    public void FilterInventory(ItemType? type)
    {
        Debug.Log($"Filter Inventory - Loại: {type}");

        // Xóa UI hiện tại
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        var filteredItems = ItemFilter.FilterInventoryByType(inventory.items, type);

        foreach (ItemInstance item in filteredItems)
        {
            GameObject newItem = Instantiate(itemPrefab, itemContainer);
            newItem.GetComponent<ItemUI>().Setup(item, this);
        }

        itemDetailPanel.Hide();
    }
    
    public void OnFilterWeapon() => FilterInventory(ItemType.Weapon);
    public void OnFilterHelmet() => FilterInventory(ItemType.Helmet);
    public void OnFilterArmor() => FilterInventory(ItemType.SpecialArmor);
    public void OnFilterBoots() => FilterInventory(ItemType.Boots);
    public void OnFilterBody() => FilterInventory(ItemType.Clother);
    public void OnFilterPet() => FilterInventory(ItemType.Horse);
    public void OnFilterHair() => FilterInventory(ItemType.Hair);
    public void OnFilterCloak() => FilterInventory(ItemType.Cloak);
    public void OnFilterAll() => FilterInventory(null);


    
    public override void Show()
    {
        base.Show();
        UpdateInventoryUI();
    }
    
    public override void Hide()
    {
        base.Hide();
        itemDetailPanel.Hide(); // Ẩn panel chi tiết nếu có
    }

    public void UpdateInventoryUI()
    {
        Debug.Log($"Reload Inventory UI - Số lượng item: {inventory.items.Count}");

        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (ItemInstance item in inventory.items)
        {
            Debug.Log($"Tạo item: {item.itemData.itemName}");
            GameObject newItem = Instantiate(itemPrefab, itemContainer);
            newItem.GetComponent<ItemUI>().Setup(item, this);
        }

        itemDetailPanel.Hide();
    }
    private ItemUI currentSelectedItem;
     
     public void SelectItem(ItemUI newItem)
     {
         if (currentSelectedItem != null)
             currentSelectedItem.SetSelected(false); // tắt chọn cũ
     
         currentSelectedItem = newItem;
         currentSelectedItem.SetSelected(true); // bật chọn mới
     }
}

