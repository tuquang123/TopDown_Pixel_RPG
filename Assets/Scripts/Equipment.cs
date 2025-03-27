using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public Dictionary<ItemType, ItemData> equippedItems = new Dictionary<ItemType, ItemData>();

    public void EquipItem(ItemData item, PlayerStats playerStats)
    {
        if (item.itemType == ItemType.Consumable) return; // Không thể trang bị thuốc

        if (equippedItems.ContainsKey(item.itemType))
        {
            UnequipItem(item.itemType, playerStats);
        }

        equippedItems[item.itemType] = item;
        ApplyStats(item, playerStats);
        Debug.Log($"Đã trang bị {item.itemName}.");
    }

    public void UnequipItem(ItemType type, PlayerStats playerStats)
    {
        if (equippedItems.ContainsKey(type))
        {
            RemoveStats(equippedItems[type], playerStats);
            Debug.Log($"Đã gỡ bỏ {equippedItems[type].itemName}.");
            equippedItems.Remove(type);
        }
    }

    private void ApplyStats(ItemData item, PlayerStats playerStats)
    {
        switch (item.itemType)
        {
            case ItemType.Weapon:
                playerStats.damage += item.value;
                break;
            case ItemType.Armor:
                playerStats.maxHealth += item.value;
                break;
        }
    }

    private void RemoveStats(ItemData item, PlayerStats playerStats)
    {
        switch (item.itemType)
        {
            case ItemType.Weapon:
                playerStats.damage -= item.value;
                break;
            case ItemType.Armor:
                playerStats.maxHealth -= item.value;
                break;
        }
    }
}
