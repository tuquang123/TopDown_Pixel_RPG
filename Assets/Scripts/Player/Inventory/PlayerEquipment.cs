using UnityEngine;
using System.Collections.Generic;

public class PlayerEquipment : MonoBehaviour
{
    public SpriteRenderer weaponRenderer;
    public SpriteRenderer armorRenderer;
    public SpriteRenderer armorLeftArmRenderer;  
    public SpriteRenderer armorRightArmRenderer; 
    public SpriteRenderer helmetRenderer;
    public SpriteRenderer bootsLeftRenderer;
    public SpriteRenderer bootsRightRenderer;
    public SpriteRenderer hair;
    
    public GameObject hourse;
    public GameObject player;

    private Dictionary<ItemType, ItemData> equippedItems = new Dictionary<ItemType, ItemData>();

    public void UpdateEquipment(ItemData newItem)
{
    if (newItem == null) return;
    
    equippedItems[newItem.itemType] = newItem;
    
    switch (newItem.itemType)
    {
        case ItemType.Weapon:
            weaponRenderer.sprite = newItem.icon;
            weaponRenderer.color = newItem.color;
            break;
        case ItemType.Armor:
            armorRenderer.sprite = newItem.icon;
            armorRenderer.color = newItem.color;
            armorLeftArmRenderer.sprite = newItem.iconLeft; 
            armorRightArmRenderer.sprite = newItem.iconRight; 
            armorLeftArmRenderer.color = newItem.color;
            armorRightArmRenderer.color = newItem.color;
            break;
        case ItemType.Helmet:
            helmetRenderer.sprite = newItem.icon;
            helmetRenderer.color = newItem.color;
            hair.gameObject.SetActive(false); 
            break;
        case ItemType.Boots:
            bootsLeftRenderer.sprite = newItem.iconLeft;
            bootsRightRenderer.sprite = newItem.iconRight;
            bootsLeftRenderer.color = newItem.color;
            bootsRightRenderer.color = newItem.color;
            break;
        case ItemType.Horse:
            hourse?.SetActive(true);
            player?.SetActive(false);
            GameEvents.OnUpdateAnimation.Raise();
            break;
    }
}
    public void RemoveEquipment(ItemType type)
    {
        if (!equippedItems.ContainsKey(type)) return;

        equippedItems.Remove(type);
        
        switch (type)
        {
            case ItemType.Weapon:
                weaponRenderer.sprite = null;
                weaponRenderer.color = Color.white;
                break;
            case ItemType.Armor:
                armorRenderer.sprite = null;
                armorLeftArmRenderer.sprite = null;
                armorRightArmRenderer.sprite = null;
                armorRenderer.color = Color.white;
                armorLeftArmRenderer.color = Color.white;
                armorRightArmRenderer.color = Color.white;
                break;
            case ItemType.Helmet:
                helmetRenderer.sprite = null;
                helmetRenderer.color = Color.white;
                hair.gameObject.SetActive(true); 
                break;
            case ItemType.Boots:
                bootsLeftRenderer.sprite = null;
                bootsRightRenderer.sprite = null;
                bootsLeftRenderer.color = Color.white;
                bootsRightRenderer.color = Color.white;
                break;
            case ItemType.Horse:
                hourse?.SetActive(false);
                player?.SetActive(true);
                GameEvents.OnUpdateAnimation.Raise();
                break;
        }
    }

}
