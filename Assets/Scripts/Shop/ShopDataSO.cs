// ShopDataSO.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopDataSO", menuName = "Shop/ShopDataSO")]
public class ShopDataSO : ScriptableObject
{
    [Header("Shop Items")]
    public List<ItemData> allItems = new();

    /// <summary>Lấy tất cả item theo type, trả về rỗng nếu không có.</summary>
    public List<ItemData> GetItemsByType(ItemType type)
    {
        var result = new List<ItemData>();
        foreach (var item in allItems)
        {
            if (item != null && item.itemType == type)
                result.Add(item);
        }
        return result;
    }

    /// <summary>Lấy toàn bộ item không lọc.</summary>
    public List<ItemData> GetAllItems() => new(allItems);

#if UNITY_EDITOR
    [ContextMenu("Sort By Tier Then Price")]
    private void EditorSort()
    {
        allItems.Sort((a, b) =>
        {
            int t = a.tier.CompareTo(b.tier);
            return t != 0 ? t : a.price.CompareTo(b.price);
        });
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}