using System.Collections.Generic;
using System.Linq;

public static class ItemFilter
{
    /// <summary>
    /// Lọc danh sách item theo ItemType.
    /// Nếu type là null, trả về tất cả.
    /// </summary>
    public static List<ItemData> FilterByType(List<ItemData> items, ItemType? type)
    {
        if (items == null) return new List<ItemData>();

        if (type == null)
            return new List<ItemData>(items); // clone danh sách
        else
            return items.Where(i => i.itemType == type).ToList();
    }

    /// <summary>
    /// Có thể mở rộng để filter theo nhiều điều kiện cùng lúc
    /// ví dụ: level, price, rarity...
    /// </summary>
    public static List<ItemData> FilterAdvanced(List<ItemData> items, ItemType? type = null, int? minLevel = null, int? maxPrice = null)
    {
        if (items == null) return new List<ItemData>();

        var query = items.AsEnumerable();

        if (type != null)
            query = query.Where(i => i.itemType == type);

        if (maxPrice != null)
            query = query.Where(i => i.price <= maxPrice.Value);

        return query.ToList();
    }
}