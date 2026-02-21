using System;
using System.Collections.Generic;
using System.Linq;
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
    public bool isLocked;
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
                items.Add(new ItemInstance(
                    refData,
                    itemData.upgradeLevel,
                    itemData.instanceID,
                    itemData.isLocked   // thêm dòng này
                ));
        }
        
        OnInventoryChanged?.Invoke();
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
    
    public bool HasItem(ItemInstance item)
    {
        return items.Contains(item);
    }

    // Xóa item khỏi túi đồ
    public event Action OnInventoryChanged;

    public void AddItem(ItemInstance item)
    {
        if (item == null || item.itemData == null) return;
        items.Add(item);
        OnInventoryChanged?.Invoke();
    }

    public bool RemoveItem(ItemInstance item)
    {
        if (items.Remove(item))
        {
            OnInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }
    public bool IsEmpty()
    {
        return items == null || items.Count == 0;
    }
    
    public ItemInstance FindFirstConsumable(bool requireHealth, bool requireMana)
    {
        foreach (var item in items)
        {
            if (item.itemData.itemType == ItemType.Consumable)
            {
                if (requireHealth && item.itemData.restoresHealth) return item;
                if (requireMana && item.itemData.restoresMana) return item;
            }
        }
        return null;
    }
    // Inventory.cs
    public int GetItemCount(ItemData targetItemData)
    {
        int count = 0;
        foreach (var item in items) 
        {
            if (item.itemData == targetItemData)
            {
                count++;
            }
        }
        return count;
    }
   
}