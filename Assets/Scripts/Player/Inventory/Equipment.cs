using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    
    public Dictionary<ItemType, ItemInstance> equippedItems = new();
    [SerializeField] private PlayerEquipment playerEquipment;
    [SerializeField] private PlayerEquipment playerEquipmentHourse;
    
    public void EquipItem(ItemInstance instance, PlayerStats stats)
    {
        if (equippedItems.ContainsKey(instance.itemData.itemType))
        {
            UnequipItem(instance.itemData.itemType, stats);
        }

        equippedItems[instance.itemData.itemType] = instance;
        stats.ApplyItemStats(instance.itemData, instance.upgradeLevel);
    }

    public ItemInstance UnequipItem(ItemType type, PlayerStats stats)
    {
        if (!equippedItems.TryGetValue(type, out ItemInstance instance)) return null;

        stats.RemoveItemStats(instance.itemData, instance.upgradeLevel);
        equippedItems.Remove(type);
        return instance;
    }
}