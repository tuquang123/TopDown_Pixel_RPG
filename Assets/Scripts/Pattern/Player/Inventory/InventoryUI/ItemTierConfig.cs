using UnityEngine;
using System;

[Serializable]
public class StatRange
{
    [Header("Price & Upgrade Cost")]
    public Vector2Int priceRange = new Vector2Int(100, 1000);
    public Vector2Int upgradeCostRange = new Vector2Int(50, 500);
    
    // Attack
    public Vector2 atkFlatRange = new Vector2(1, 3);
    public Vector2 atkPercentRange = new Vector2(0, 0); // nếu không cần % thì để 0

    // Defense
    public Vector2 defFlatRange = new Vector2(0, 1);
    public Vector2 defPercentRange = new Vector2(0, 0);

    // Health / Mana
    public Vector2 healthFlatRange = new Vector2(0, 10);
    public Vector2 healthPercentRange = new Vector2(0, 0);
    public Vector2 manaFlatRange = new Vector2(0, 5);
    public Vector2 manaPercentRange = new Vector2(0, 0);

    // Crit / Attack Speed / LifeSteal / Move Speed
    public Vector2 critFlatRange = new Vector2(0, 0); // thường để 0, dùng percent
    public Vector2 critPercentRange = new Vector2(0, 1); // 0%–1%

    public Vector2 attackSpeedFlatRange = new Vector2(0, 0);
    public Vector2 attackSpeedPercentRange = new Vector2(0, 2); // 0%–2%

    public Vector2 lifeStealFlatRange = new Vector2(0, 0);
    public Vector2 lifeStealPercentRange = new Vector2(0, 0); // bật nếu muốn

    public Vector2 speedFlatRange = new Vector2(0, 0.01f);
    public Vector2 speedPercentRange = new Vector2(0, 0); // nếu muốn % tăng tốc
}

[CreateAssetMenu(fileName = "ItemTierConfig", menuName = "Inventory/Item Tier Config")]
public class ItemTierConfig : ScriptableObject
{
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
}