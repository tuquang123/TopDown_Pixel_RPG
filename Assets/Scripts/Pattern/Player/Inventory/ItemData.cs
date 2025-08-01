using UnityEngine;
using Sirenix.OdinInspector;
using System;

#region Support Types

[Serializable]
[InlineProperty]
[HideLabel]
public class ItemStatBonus
{
    [HorizontalGroup("Bonus", width: 0.5f)]
    [LabelText("Flat")]
    [LabelWidth(35)]
    [GUIColor(0.9f, 0.95f, 1f)]
    public float flat;

    [HorizontalGroup("Bonus", width: 0.5f)]
    [LabelText("%")]
    [LabelWidth(25)]
    [GUIColor(0.95f, 1f, 0.95f)]
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
    Weapon, Clother, Consumable, Helmet, Boots, Horse ,SpecialArmor, Cloak, Hair
}

public enum ItemTier
{
    Common, Uncommon, Rare, Epic, Legendary, Mythic
}

public static class ItemUtility
{
    public static Color GetColorByTier(ItemTier tier)
    {
        switch (tier)
        {
            case ItemTier.Common:    return new Color(0.8f, 0.8f, 0.8f);
            case ItemTier.Uncommon:  return new Color(0.2f, 0.8f, 0.2f);
            case ItemTier.Rare:      return new Color(0.2f, 0.4f, 1f);
            case ItemTier.Epic:      return new Color(0.6f, 0.2f, 0.8f);
            case ItemTier.Legendary: return new Color(1f, 0.6f, 0f);
            case ItemTier.Mythic:    return new Color(0.9f, 0.1f, 0.1f);
            default: return Color.white;
        }
    }
}

#endregion

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [BoxGroup("General Info")]
    [LabelWidth(100)] public string itemID;

    [BoxGroup("General Info")]
    [LabelWidth(100)] public string itemName;

    [BoxGroup("General Info")]
    [PreviewField(60, ObjectFieldAlignment.Left)]
    [HideLabel]
    public Sprite icon;

    [BoxGroup("General Info")]
    [LabelWidth(100)] public ItemType itemType;

    [BoxGroup("General Info")]
    [LabelWidth(100)] public ItemTier tier;

    [BoxGroup("General Info")]
    [LabelWidth(100)] public int price;

    [BoxGroup("General Info")]
    [MultiLineProperty(3)]
    public string description;

    [BoxGroup("Stats"), HideLabel]
    [FoldoutGroup("Stats/Battle Stats")]
    [LabelText("ATK")] public ItemStatBonus attack;

    [FoldoutGroup("Stats/Battle Stats")]
    [LabelText("DEF")] public ItemStatBonus defense;

    [FoldoutGroup("Stats/Resource Stats")]
    [LabelText("HP")] public ItemStatBonus health;

    [FoldoutGroup("Stats/Resource Stats")]
    [LabelText("Mana")] public ItemStatBonus mana;

    [FoldoutGroup("Stats/Special Stats")]
    [LabelText("Crit %")] public ItemStatBonus critChance;

    [FoldoutGroup("Stats/Special Stats")]
    [LabelText("Atk Speed")] public ItemStatBonus attackSpeed;

    [FoldoutGroup("Stats/Special Stats")]
    [LabelText("Life Steal")] public ItemStatBonus lifeSteal;

    [FoldoutGroup("Stats/Special Stats")]
    [LabelText("Move Speed")] public ItemStatBonus speed;

    [BoxGroup("Visuals")]
    [ColorPalette]
    public Color color;

    [BoxGroup("Visuals")]
    [PreviewField(60)]
    public Sprite iconLeft;

    [BoxGroup("Visuals")]
    [PreviewField(60)]
    public Sprite iconRight;

    [BoxGroup("Upgrade")]
    [LabelWidth(130)]
    public int baseUpgradeCost = 100;
}
