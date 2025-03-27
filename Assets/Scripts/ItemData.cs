using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public enum ItemType { Weapon, Armor, Consumable }

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemID;  // ID duy nhất cho mỗi item
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    public int value; // Sát thương, phòng thủ, hồi máu
}


[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> allItems;  // Danh sách tất cả item
    public List<ItemData> weapons;   // Nhóm vũ khí
    public List<ItemData> armors;    // Nhóm giáp
    public List<ItemData> consumables;  // Nhóm thuốc

    public ItemData GetItemByID(string itemID)
    {
        return allItems.Find(item => item.itemID == itemID);
    }

    public ItemData GetItemByName(string itemName)
    {
        return allItems.Find(item => item.itemName == itemName);
    }
}

