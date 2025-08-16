using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EquipmentData
{
    public ItemType slotType;
    public ItemInstanceData equippedItem;
}

[System.Serializable]
public class ItemInstanceData
{
    public string itemID;     
    public int upgradeLevel;
    public int instanceID;
}

public class Inventory : Singleton<Inventory>
{
    public List<ItemInstance> items = new List<ItemInstance>();
    
    public void FromData(List<ItemInstanceData> data, ItemDatabase db)
    {
        items.Clear();
        foreach (var itemData in data)
        {
            ItemData refData = db.GetItemByID(itemData.itemID); 
            if (refData != null)
                items.Add(new ItemInstance(refData, itemData.upgradeLevel, itemData.instanceID));
        }
    }
    
    public List<ItemInstanceData> ToData()
    {
        List<ItemInstanceData> data = new List<ItemInstanceData>();
        foreach (var item in items)
        {
            data.Add(item.ToData());
        }
        return data;
    }

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