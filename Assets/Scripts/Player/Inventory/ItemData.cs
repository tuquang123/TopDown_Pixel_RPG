using UnityEngine;

public enum ItemType 
{ 
    Weapon, Armor, Consumable,Helmet,Boots,Horse
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

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    
    public int price; 

    public ItemTier tier;         // TIER
    [TextArea] public string description; // MÔ TẢ

    public int attackPower;
    public int defense;
    public int healthBonus;
    public int manaBonus;

    public float critChance;
    public float attackSpeed;
    public float lifeSteal;
    public float moveSpeed;

    public Color color;           // Màu cho vũ khí/giáp

    // Chỉ dùng cho đồ có sprite riêng tay/trái/phải
    public Sprite iconLeft;
    public Sprite iconRight;
    
    [HideInInspector] public int upgradeLevel = 1;  // mặc định cấp 1
    public int baseUpgradeCost = 100;              // giá cơ bản (sửa tuỳ ý)

}


