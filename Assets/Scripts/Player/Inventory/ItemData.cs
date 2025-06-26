using UnityEngine;
using Sirenix.OdinInspector;

public enum ItemType
{
    Weapon, Armor, Consumable, Helmet, Boots, Horse
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
            case ItemTier.Rare:      return new Color(0.2f, 0.4f, 1f);
            case ItemTier.Epic:      return new Color(0.6f, 0.2f, 0.8f);
            case ItemTier.Legendary: return new Color(1f, 0.6f, 0f);
            case ItemTier.Mythic:    return new Color(0.9f, 0.1f, 0.1f);
            default: return Color.white;
        }
    }
}

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

    [BoxGroup("Stats")]
    [LabelText("ATK")] public int attackPower;

    [BoxGroup("Stats")]
    [LabelText("DEF")] public int defense;

    [BoxGroup("Stats")]
    public int healthBonus;

    [BoxGroup("Stats")]
    public int manaBonus;

    [BoxGroup("Stats")]
    [Range(0f, 100f)] public float critChance;

    [BoxGroup("Stats")]
    [Range(0f, 10f)] public float attackSpeed;

    [BoxGroup("Stats")]
    [Range(0f, 100f)] public float lifeSteal;

    [BoxGroup("Stats")]
    [Range(0f, 100f)] public float moveSpeed;

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
