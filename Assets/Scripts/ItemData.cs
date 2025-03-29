using System.Collections.Generic;
using UnityEngine;

public enum ItemType { Weapon, Armor, Consumable,
    Helmet,
    Boots,
    Hair,
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemID;  // ID duy nhất cho mỗi item
    public string itemName;
    public Sprite icon;
    public Sprite iconRight;
    public Sprite iconLeft;
    public ItemType itemType;
    public int value;
    public Color color =  Color.white;
}

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> allItems = new List<ItemData>();
    
    private Dictionary<string, ItemData> itemDictionary = new Dictionary<string, ItemData>();

    private void OnEnable()
    {
        UpdateDatabase();
    }

    public void UpdateDatabase()
    {
        itemDictionary.Clear();
        foreach (var item in allItems)
        {
            if (!itemDictionary.ContainsKey(item.itemID))
            {
                itemDictionary.Add(item.itemID, item);
            }
        }
    }

    public ItemData GetItemByID(string itemID)
    {
        if (itemDictionary.TryGetValue(itemID, out var item))
        {
            return item;
        }
        return null;
    }

    public ItemData GetItemByName(string itemName)
    {
        return allItems.Find(item => item.itemName == itemName);
    }

    public List<ItemData> GetItemsByType(ItemType type)
    {
        return allItems.FindAll(item => item.itemType == type);
    }
}

