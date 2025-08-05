using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

// Bộ lọc hiển thị dễ chọn
public enum FilterItemType
{
    Any,
    Weapon,
    Clother,
    Consumable,
    Helmet,
    Boots,
    Horse,
    SpecialArmor,
    Cloak,
    Hair
}

public enum FilterItemTier
{
    Any,
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic
}

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("Core")]
    public List<ItemData> allItems = new List<ItemData>();

    [Header("Default Config")]
    public ItemTierConfig defaultTierConfig;

    [Header("Loot Rarity Weights")]
    public float commonWeight = 50f;
    public float uncommonWeight = 25f;
    public float rareWeight = 15f;
    public float epicWeight = 7f;
    public float legendaryWeight = 3f;
    public float mythicWeight = 1f;
    
    [BoxGroup("Batch Randomizer")]
    [LabelText("Random Price")]
    public bool randomizePrice = true;

    [BoxGroup("Batch Randomizer")]
    [LabelText("Random Upgrade Cost")]
    public bool randomizeUpgradeCost = true;

    private Dictionary<string, ItemData> itemDictionary = new Dictionary<string, ItemData>();

    private void OnEnable()
    {
        UpdateDatabase();
    }

    [Button("Update Database (validate IDs)")]
    public void UpdateDatabase()
    {
        itemDictionary.Clear();
        var duplicates = new HashSet<string>();

        foreach (var item in allItems)
        {
            if (string.IsNullOrEmpty(item.itemID))
            {
                Debug.LogWarning($"Item '{item.name}' có itemID rỗng.");
                continue;
            }

            if (itemDictionary.ContainsKey(item.itemID))
            {
                duplicates.Add(item.itemID);
                continue;
            }

            itemDictionary.Add(item.itemID, item);
        }

        if (duplicates.Count > 0)
        {
            Debug.LogWarning($"Trùng itemID: {string.Join(", ", duplicates)}. Mỗi itemID nên duy nhất.");
        }
    }

    public ItemData GetItemByID(string itemID)
    {
        if (itemDictionary.TryGetValue(itemID, out var item))
            return item;
        return null;
    }

    public ItemData GetItemByName(string itemName)
    {
        return allItems.Find(item => item.itemName == itemName);
    }

    public List<ItemData> GetItemsByType(ItemType type)
    {
        return allItems.FindAll(i => i.itemType == type);
    }

    public List<ItemData> GetItemsByTier(ItemTier tier)
    {
        return allItems.FindAll(i => i.tier == tier);
    }

    public List<ItemData> Filter(System.Func<ItemData, bool> predicate)
    {
        return allItems.Where(predicate).ToList();
    }

    // ===================== BATCH RANDOMIZER UI =====================
    [BoxGroup("Batch Randomizer")]
    [LabelText("Filter Type")]
    public FilterItemType filterType = FilterItemType.Any;

    [BoxGroup("Batch Randomizer")]
    [LabelText("Filter Tier")]
    public FilterItemTier filterTier = FilterItemTier.Any;

    [BoxGroup("Batch Randomizer")]
    [LabelText("Override Locks")]
    public bool overrideLocks;

    [BoxGroup("Batch Randomizer")]
    [InfoBox("Randomize Matching: Reroll chỉ item khớp filter.\nRandomize All: Reroll tất cả.\nOverride Locks: Bỏ qua các lock stat.")]
    [Button("Randomize Matching Items")]
    public void RandomizeMatching()
    {
        if (defaultTierConfig == null)
        {
            Debug.LogWarning("Missing defaultTierConfig; items without tierConfig sẽ không thể random đúng.");
        }

        var list = allItems.AsEnumerable();

        if (filterType != FilterItemType.Any)
        {
            if (System.Enum.TryParse<ItemType>(filterType.ToString(), out var parsedType))
                list = list.Where(i => i.itemType == parsedType);
        }

        if (filterTier != FilterItemTier.Any)
        {
            if (System.Enum.TryParse<ItemTier>(filterTier.ToString(), out var parsedTier))
                list = list.Where(i => i.tier == parsedTier);
        }

        int count = list.Count();
        if (count == 0)
        {
            Debug.LogWarning("Không có item nào khớp filter.");
            return;
        }

        BatchRandomize(list, overrideLocks);
        UpdateDatabase();
        Debug.Log($"Randomized {count} items theo filter.");
    }

    [BoxGroup("Batch Randomizer")]
    [Button("Randomize All Items")]
    public void RandomizeAll()
    {
        if (defaultTierConfig == null)
        {
            Debug.LogWarning("Missing defaultTierConfig; items without tierConfig sẽ không thể random đúng.");
        }

        BatchRandomize(allItems, overrideLocks);
        UpdateDatabase();
        Debug.Log($"Randomized toàn bộ {allItems.Count} items.");
    }

    [BoxGroup("Batch Randomizer")]
    [Button("Log Sample Loot Picks (10)")]
    public void LogSampleLoot()
    {
        for (int i = 0; i < 10; i++)
        {
            var loot = GetRandomLoot(filterType == FilterItemType.Any ? null : 
                System.Enum.TryParse<ItemType>(filterType.ToString(), out var parsedType) ? parsedType : (ItemType?)null);

            if (loot != null)
                Debug.Log($"[{i}] {loot.itemName} ({loot.tier}) ATK: {loot.attack} DEF: {loot.defense}");
        }
    }

    // ===================== Core batch logic =====================
    public void BatchRandomize(IEnumerable<ItemData> items, bool overrideLocks = false)
    {
        foreach (var item in items)
        {
            if (item == null) continue;

            if (item.tierConfig == null && defaultTierConfig != null)
                item.tierConfig = defaultTierConfig;

            if (item.tierConfig == null)
            {
                Debug.LogWarning($"Item '{item.itemName}' không có tierConfig để randomize.");
                continue;
            }

            // save original locks
            bool prevAttack = item.lockAttack;
            bool prevDefense = item.lockDefense;
            bool prevHealth = item.lockHealth;
            bool prevMana = item.lockMana;
            bool prevCrit = item.lockCrit;
            bool prevAS = item.lockAttackSpeed;
            bool prevLS = item.lockLifeSteal;
            bool prevSpeed = item.lockSpeed;

            if (overrideLocks)
            {
                item.lockAttack = item.lockDefense = item.lockHealth = item.lockMana =
                    item.lockCrit = item.lockAttackSpeed = item.lockLifeSteal = item.lockSpeed = false;
            }

            item.RandomizeStats();

            if (overrideLocks)
            {
                item.lockAttack = prevAttack;
                item.lockDefense = prevDefense;
                item.lockHealth = prevHealth;
                item.lockMana = prevMana;
                item.lockCrit = prevCrit;
                item.lockAttackSpeed = prevAS;
                item.lockLifeSteal = prevLS;
                item.lockSpeed = prevSpeed;
            }
        }
    }

    // ===================== Loot pick =====================
    public ItemData GetRandomLoot(ItemType? typeFilter = null)
    {
        var pool = new List<ItemData>();

        void AddWithWeight(ItemTier tier, float weight)
        {
            var list = allItems.Where(i => i.tier == tier);
            if (typeFilter.HasValue)
                list = list.Where(i => i.itemType == typeFilter.Value);

            foreach (var item in list)
            {
                int count = Mathf.CeilToInt(weight);
                for (int j = 0; j < count; j++)
                    pool.Add(item);
            }
        }

        AddWithWeight(ItemTier.Common, commonWeight);
        AddWithWeight(ItemTier.Uncommon, uncommonWeight);
        AddWithWeight(ItemTier.Rare, rareWeight);
        AddWithWeight(ItemTier.Epic, epicWeight);
        AddWithWeight(ItemTier.Legendary, legendaryWeight);
        AddWithWeight(ItemTier.Mythic, mythicWeight);

        if (pool.Count == 0)
            return null;

        return pool[Random.Range(0, pool.Count)];
    }
}
