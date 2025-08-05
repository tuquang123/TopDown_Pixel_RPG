using UnityEngine;
using Sirenix.OdinInspector;
using System;

#region Support Types

[Serializable]
[InlineProperty]
[HideLabel]
public class ItemStatBonus
{
    [HorizontalGroup("Bonus", width: 0.5f)] [LabelText("Flat")] [LabelWidth(35)] [GUIColor(0.9f, 0.95f, 1f)]
    public float flat;

    [HorizontalGroup("Bonus", width: 0.5f)] [LabelText("%")] [LabelWidth(25)] [GUIColor(0.95f, 1f, 0.95f)]
    public float percent;

    public ItemStatBonus(float flat = 0, float percent = 0)
    {
        this.flat = flat;
        this.percent = percent;
    }

    public bool HasValue => flat != 0 || percent != 0;

    public override string ToString()
    {
        string result = "";
        if (flat != 0) result += $"+{flat}";
        if (percent != 0)
        {
            if (flat != 0) result += " + ";
            result += $"{percent}%";
        }

        return result;
    }
}

public enum ItemType
{
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

public enum ItemTier
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic
}

public static class ItemUtility
{
    public static Color GetColorByTier(ItemTier tier)
    {
        return tier switch
        {
            ItemTier.Common => new Color(0.8f, 0.8f, 0.8f),
            ItemTier.Uncommon => new Color(0.2f, 0.8f, 0.2f),
            ItemTier.Rare => new Color(0.2f, 0.4f, 1f),
            ItemTier.Epic => new Color(0.6f, 0.2f, 0.8f),
            ItemTier.Legendary => new Color(1f, 0.6f, 0f),
            ItemTier.Mythic => new Color(0.9f, 0.1f, 0.1f),
            _ => Color.white,
        };
    }
}

#endregion

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [BoxGroup("General Info")] [LabelWidth(100)]
    public string itemID;

    [BoxGroup("General Info")] [LabelWidth(100)]
    public string itemName;

    [BoxGroup("General Info")] [PreviewField(60, ObjectFieldAlignment.Left)] [HideLabel]
    public Sprite icon;

    [BoxGroup("General Info")] [LabelWidth(100)]
    public ItemType itemType;

    [BoxGroup("General Info")] [LabelWidth(100)]
    public ItemTier tier;

    [BoxGroup("General Info")] [LabelWidth(100)]
    public int price;

    [BoxGroup("General Info")] [MultiLineProperty(3)]
    public string description;

    [BoxGroup("Stats"), HideLabel] [FoldoutGroup("Stats/Battle Stats")] [LabelText("ATK")]
    public ItemStatBonus attack = new ItemStatBonus();

    [FoldoutGroup("Stats/Battle Stats")] [LabelText("DEF")]
    public ItemStatBonus defense = new ItemStatBonus();

    [FoldoutGroup("Stats/Resource Stats")] [LabelText("HP")]
    public ItemStatBonus health = new ItemStatBonus();

    [FoldoutGroup("Stats/Resource Stats")] [LabelText("Mana")]
    public ItemStatBonus mana = new ItemStatBonus();

    [FoldoutGroup("Stats/Special Stats")] [LabelText("Crit %")]
    public ItemStatBonus critChance = new ItemStatBonus();

    [FoldoutGroup("Stats/Special Stats")] [LabelText("Atk Speed")]
    public ItemStatBonus attackSpeed = new ItemStatBonus();

    [FoldoutGroup("Stats/Special Stats")] [LabelText("Life Steal")]
    public ItemStatBonus lifeSteal = new ItemStatBonus();

    [FoldoutGroup("Stats/Special Stats")] [LabelText("Move Speed")]
    public ItemStatBonus speed = new ItemStatBonus();

    [BoxGroup("Visuals")] [ColorPalette] public Color color;

    [BoxGroup("Visuals")] [PreviewField(60)]
    public Sprite iconLeft;

    [BoxGroup("Visuals")] [PreviewField(60)]
    public Sprite iconRight;

    private bool IsHorse => itemType == ItemType.Horse;

    [BoxGroup("Mount")] [ShowIf("@itemType == ItemType.Horse")]
    public HorseData horseData;

    [BoxGroup("Upgrade")] [LabelWidth(130)]
    public int baseUpgradeCost = 100;


    // RANDOMIZER
    [BoxGroup("Randomizer")] public ItemTierConfig tierConfig;

    [BoxGroup("Randomizer"), LabelText("Locks")]
    public bool lockAttack;

    [BoxGroup("Randomizer")] public bool lockDefense;
    [BoxGroup("Randomizer")] public bool lockHealth;
    [BoxGroup("Randomizer")] public bool lockMana;
    [BoxGroup("Randomizer")] public bool lockCrit;
    [BoxGroup("Randomizer")] public bool lockAttackSpeed;
    [BoxGroup("Randomizer")] public bool lockLifeSteal;
    [BoxGroup("Randomizer")] public bool lockSpeed;
    
    [BoxGroup("Randomizer")]
    [InfoBox("Nhấn để random lại cả flat và % theo tier. Bật lock để giữ stat cụ thể khi reroll.")]
    [Button("Randomize Stats")]
    public void RandomizeStats()
    {
        if (tierConfig == null)
        {
            Debug.LogWarning($"[{itemName}] Missing TierConfig. Cannot randomize.");
            return;
        }

        var range = tierConfig.GetRange(tier);

        // helper để làm tròn: flat thành int, percent giữ 1 chữ số
        float RoundPercent(float v) => Mathf.Round(v * 10f) / 10f;
        float RoundFlat(float v) => Mathf.Round(v); // nếu muốn int hẳn, dùng Mathf.RoundToInt và cast về float
        
        price = UnityEngine.Random.Range(range.priceRange.x, range.priceRange.y + 1);
        baseUpgradeCost = UnityEngine.Random.Range(range.upgradeCostRange.x, range.upgradeCostRange.y + 1);

        // ATK
        if (!lockAttack)
        {
            float atkFlat = UnityEngine.Random.Range(range.atkFlatRange.x, range.atkFlatRange.y);
            float atkPct = UnityEngine.Random.Range(range.atkPercentRange.x, range.atkPercentRange.y);
            attack = new ItemStatBonus(flat: RoundFlat(atkFlat), percent: RoundPercent(atkPct));
        }

        // DEF
        if (!lockDefense)
        {
            float defFlat = UnityEngine.Random.Range(range.defFlatRange.x, range.defFlatRange.y);
            float defPct = UnityEngine.Random.Range(range.defPercentRange.x, range.defPercentRange.y);
            defense = new ItemStatBonus(flat: RoundFlat(defFlat), percent: RoundPercent(defPct));
        }

        // HP
        if (!lockHealth)
        {
            float hpFlat = UnityEngine.Random.Range(range.healthFlatRange.x, range.healthFlatRange.y);
            float hpPct = UnityEngine.Random.Range(range.healthPercentRange.x, range.healthPercentRange.y);
            health = new ItemStatBonus(flat: RoundFlat(hpFlat), percent: RoundPercent(hpPct));
        }

        // Mana
        if (!lockMana)
        {
            float manaFlat = UnityEngine.Random.Range(range.manaFlatRange.x, range.manaFlatRange.y);
            float manaPct = UnityEngine.Random.Range(range.manaPercentRange.x, range.manaPercentRange.y);
            mana = new ItemStatBonus(flat: RoundFlat(manaFlat), percent: RoundPercent(manaPct));
        }

        // Crit
        if (!lockCrit)
        {
            float critFlat = UnityEngine.Random.Range(range.critFlatRange.x, range.critFlatRange.y);
            float critPct = UnityEngine.Random.Range(range.critPercentRange.x, range.critPercentRange.y);
            critChance = new ItemStatBonus(flat: RoundFlat(critFlat), percent: RoundPercent(critPct));
        }

        // Attack Speed
        if (!lockAttackSpeed)
        {
            float asFlat = UnityEngine.Random.Range(range.attackSpeedFlatRange.x, range.attackSpeedFlatRange.y);
            float asPct = UnityEngine.Random.Range(range.attackSpeedPercentRange.x, range.attackSpeedPercentRange.y);
            attackSpeed = new ItemStatBonus(flat: RoundFlat(asFlat), percent: RoundPercent(asPct));
        }

        // Life Steal
        if (!lockLifeSteal)
        {
            float lsFlat = UnityEngine.Random.Range(range.lifeStealFlatRange.x, range.lifeStealFlatRange.y);
            float lsPct = UnityEngine.Random.Range(range.lifeStealPercentRange.x, range.lifeStealPercentRange.y);
            lifeSteal = new ItemStatBonus(flat: RoundFlat(lsFlat), percent: RoundPercent(lsPct));
        }

        // Move Speed
        if (!lockSpeed)
        {
            float spdFlat = UnityEngine.Random.Range(range.speedFlatRange.x, range.speedFlatRange.y);
            float spdPct = UnityEngine.Random.Range(range.speedPercentRange.x, range.speedPercentRange.y);
            speed = new ItemStatBonus(flat: RoundFlat(spdFlat), percent: RoundPercent(spdPct));
        }


#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}