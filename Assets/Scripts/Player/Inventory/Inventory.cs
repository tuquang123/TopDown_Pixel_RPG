using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<ItemData> items = new List<ItemData>();

    // Thêm item vào túi đồ
    public void AddItem(ItemData item)
    {
        if (item == null) return; // Kiểm tra item null
        items.Add(item);
        Debug.Log($"Đã thêm {item.itemName} vào túi đồ.");
    }
    
    public bool HasItem(ItemData item)
    {
        return items.Contains(item);
    }


    // Xóa item khỏi túi đồ
    public bool RemoveItem(ItemData item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            return true;
        }
        return false;
    }
}