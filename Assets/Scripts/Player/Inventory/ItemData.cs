using UnityEngine;

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
            case ItemTier.Common:    return new Color(0.8f, 0.8f, 0.8f); // Xám
            case ItemTier.Rare:      return new Color(0.2f, 0.4f, 1f);  // Xanh dương
            case ItemTier.Epic:      return new Color(0.6f, 0.2f, 0.8f); // Tím
            case ItemTier.Legendary: return new Color(1f, 0.6f, 0f);   // Vàng
            case ItemTier.Mythic:    return new Color(0.9f, 0.1f, 0.1f); // Đỏ tối ưu
            default: return Color.white;
        }
    }
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    
    public int price; 
    public ItemTier tier;
    [TextArea] public string description;

    public int attackPower;
    public int defense;
    public int healthBonus;
    public int manaBonus;

    public float critChance;
    public float attackSpeed;
    public float lifeSteal;
    public float moveSpeed;

    public Color color;

    public Sprite iconLeft;
    public Sprite iconRight;
    
    public int baseUpgradeCost = 100; 
}