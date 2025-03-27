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

    // Xóa item khỏi túi đồ
    public void RemoveItem(ItemData item)
    {
        if (item == null || !items.Contains(item)) return; // Kiểm tra item null và tồn tại trong túi đồ
        items.Remove(item);
        Debug.Log($"Đã xóa {item.itemName} khỏi túi đồ.");
    }

    // Sử dụng item
    public void UseItem(ItemData item, PlayerStats playerStats)
    {
        if (item == null || !items.Contains(item)) return; // Kiểm tra item null và tồn tại trong túi đồ

        switch (item.itemType)
        {
            case ItemType.Consumable:
                playerStats.Heal(item.value);
                break;

            case ItemType.Weapon:
                // Xử lý nếu item là vũ khí (ví dụ: tăng sát thương)
                Debug.Log($"Sử dụng vũ khí {item.itemName}");
                break;

            case ItemType.Armor:
                // Xử lý nếu item là giáp (ví dụ: tăng máu tối đa)
                Debug.Log($"Sử dụng giáp {item.itemName}");
                break;

            default:
                Debug.Log($"Item {item.itemName} không thể sử dụng.");
                break;
        }

        RemoveItem(item); // Sau khi sử dụng xong, xóa item khỏi túi đồ
    }
}