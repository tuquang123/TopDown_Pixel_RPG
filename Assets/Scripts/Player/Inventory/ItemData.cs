using UnityEngine;

public enum ItemType { Weapon, Armor, Consumable,Helmet,Boots,
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemID;  // ID duy nhất cho mỗi item
    public string itemName;
    public Sprite icon;
    public Sprite iconRight;
    public Sprite iconLeft;
    public ItemType itemType;
    public int value;
    public Color color =  Color.white;
}

