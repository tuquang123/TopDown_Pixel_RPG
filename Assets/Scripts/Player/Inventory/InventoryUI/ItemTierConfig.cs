using UnityEngine;
using System;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class StatRange
{
    [Header("Price & Upgrade Cost")]
    public Vector2Int priceRange = new Vector2Int(100, 1000);
    public Vector2Int upgradeCostRange = new Vector2Int(50, 500);

    [Header("Attack")]
    public Vector2 atkFlatRange = new Vector2(1, 3);
    public Vector2 atkPercentRange = new Vector2(0, 0); // % Bonus ATK

    [Header("Defense")]
    public Vector2 defFlatRange = new Vector2(0, 1);
    public Vector2 defPercentRange = new Vector2(0, 0); // % Bonus DEF

    [Header("Health / Mana")]
    public Vector2 healthFlatRange = new Vector2(0, 10);
    public Vector2 healthPercentRange = new Vector2(0, 0); // % Max HP

    public Vector2 manaFlatRange = new Vector2(0, 5);
    public Vector2 manaPercentRange = new Vector2(0, 0); // % Max MP

    [Header("Crit / Speed / Lifesteal")]
    public Vector2 critFlatRange = new Vector2(0, 0); // thường không dùng flat
    public Vector2 critPercentRange = new Vector2(0, 1); // 0%–1%

    public Vector2 attackSpeedFlatRange = new Vector2(0, 0);
    public Vector2 attackSpeedPercentRange = new Vector2(0, 0); // 0%–2%

    public Vector2 lifeStealFlatRange = new Vector2(0, 0);
    public Vector2 lifeStealPercentRange = new Vector2(0, 0); // 0%–x%

    [Header("Move Speed")]
    public Vector2 speedFlatRange = new Vector2(0, 0.01f);
    public Vector2 speedPercentRange = new Vector2(0, 0); // nếu cần dùng %
}

[CreateAssetMenu(fileName = "ItemTierConfig", menuName = "Inventory/Item Tier Config")]
public class ItemTierConfig : ScriptableObject
{
    [Header("Stat Ranges Per Tier")]
    public StatRange common;
    public StatRange uncommon;
    public StatRange rare;
    public StatRange epic;
    public StatRange legendary;
    public StatRange mythic;

    public StatRange GetRange(ItemTier tier)
    {
        return tier switch
        {
            ItemTier.Common => common,
            ItemTier.Uncommon => uncommon,
            ItemTier.Rare => rare,
            ItemTier.Epic => epic,
            ItemTier.Legendary => legendary,
            ItemTier.Mythic => mythic,
            _ => common,
        };
    }
    
    public static StatRange CreateBaseRange(float multiplier)
    {
        return new StatRange
        {
            priceRange = new Vector2Int((int)(100 * multiplier), (int)(500 * multiplier)),
            upgradeCostRange = new Vector2Int((int)(50 * multiplier), (int)(300 * multiplier)),

            atkFlatRange = new Vector2(10, 100) * multiplier,
            atkPercentRange = Vector2.zero,

            defFlatRange = new Vector2(1, 20) * multiplier,
            defPercentRange = Vector2.zero,

            healthFlatRange = new Vector2(100, 1000) * multiplier,
            healthPercentRange = Vector2.zero,

            manaFlatRange = new Vector2(50, 300) * multiplier,
            manaPercentRange = Vector2.zero,

            critFlatRange = new Vector2(0, 20) * multiplier,
            critPercentRange = Vector2.zero,

            attackSpeedFlatRange = new Vector2(0, .5f) * multiplier,
            attackSpeedPercentRange = Vector2.zero,

            lifeStealFlatRange = new Vector2(0, 10) * multiplier,
            lifeStealPercentRange = Vector2.zero,

            speedFlatRange = new Vector2(0f, 1f) * multiplier,
            speedPercentRange = Vector2.zero,
        };
    }
    
    [Button("Reset All Tiers To Default")]
    [ContextMenu("Reset All Tiers To Default")]
    public void ResetAllTiersToDefault()
    {
        common    = CreateBaseRange(1f);
        uncommon  = CreateBaseRange(1.2f);
        rare      = CreateBaseRange(1.5f);
        epic      = CreateBaseRange(2f);
        legendary = CreateBaseRange(2.5f);
        mythic    = CreateBaseRange(3f);

#if UNITY_EDITOR
        EditorUtility.SetDirty(this); 
#endif
    }
}
