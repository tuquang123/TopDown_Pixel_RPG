using System.Collections.Generic;
using UnityEngine;

public class Inventory : Singleton<Inventory>
{
    public List<ItemInstance> items = new List<ItemInstance>();

    // Thêm item vào túi đồ
    public void AddItem(ItemInstance item)
    {
        if (item == null || item.itemData == null) return;
        items.Add(item);
        Debug.Log($"Đã thêm {item.itemData.itemName} vào túi đồ.");
    }

    public bool HasItem(ItemInstance item)
    {
        return items.Contains(item);
    }

    // Xóa item khỏi túi đồ
    public bool RemoveItem(ItemInstance item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            return true;
        }
        return false;
    }
}