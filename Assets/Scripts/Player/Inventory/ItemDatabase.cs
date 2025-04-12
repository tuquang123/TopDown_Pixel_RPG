using System.Collections.Generic;
using UnityEngine;

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