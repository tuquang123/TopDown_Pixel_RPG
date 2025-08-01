using UnityEngine;
using System.Collections.Generic;

public class PlayerEquipment : MonoBehaviour
{
    public SpriteRenderer weaponRenderer;
    public SpriteRenderer clotherRenderer;
    public SpriteRenderer clotherLeftArmRenderer;  
    public SpriteRenderer clotherRightArmRenderer; 
    public SpriteRenderer helmetRenderer;
    public SpriteRenderer bootsLeftRenderer;
    public SpriteRenderer bootsRightRenderer;
    public SpriteRenderer hair;
    public SpriteRenderer cloakRenderer;
    public SpriteRenderer specialArmorRenderer;
    public SpriteRenderer armorLeftRenderer;
    public SpriteRenderer armorRightRenderer;
    //public SpriteRenderer hairRenderer;
    
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
        case ItemType.Clother:
            clotherRenderer.sprite = newItem.icon;
            clotherRenderer.color = newItem.color;
            clotherLeftArmRenderer.sprite = newItem.iconLeft; 
            clotherRightArmRenderer.sprite = newItem.iconRight; 
            clotherLeftArmRenderer.color = newItem.color;
            clotherRightArmRenderer.color = newItem.color;
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

            if (hourse != null)
            {
                var horseRenderer = hourse.GetComponent<HorseRenderer>();
                if (horseRenderer != null && newItem.horseData != null)
                {
                    horseRenderer.ApplyHorseData(newItem.horseData);
                }
            }

            GameEvents.OnUpdateAnimation.Raise();
            break;

        
        case ItemType.Cloak:
            cloakRenderer.sprite = newItem.icon;
            cloakRenderer.color = newItem.color;
            break;
        case ItemType.SpecialArmor:
            specialArmorRenderer.sprite = newItem.icon;
            specialArmorRenderer.color = newItem.color;
            armorRightRenderer.sprite = newItem.iconRight; 
            armorLeftRenderer.sprite = newItem.iconLeft; 
            armorRightRenderer.color = newItem.color;
            armorLeftRenderer.color = newItem.color;
            break;
        case ItemType.Hair:
            hair.sprite = newItem.icon;
            hair.color = newItem.color;
            helmetRenderer.gameObject.SetActive(false); 
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
            case ItemType.Clother:
                clotherRenderer.sprite = null;
                clotherLeftArmRenderer.sprite = null;
                clotherRightArmRenderer.sprite = null;
                clotherRenderer.color = Color.white;
                clotherLeftArmRenderer.color = Color.white;
                clotherRightArmRenderer.color = Color.white;
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
            
            case ItemType.Cloak:
                cloakRenderer.sprite = null;
                cloakRenderer.color = Color.white;
                break;
            case ItemType.SpecialArmor:
                specialArmorRenderer.sprite = null;
                specialArmorRenderer.color = Color.white;
                armorRightRenderer.sprite = null; 
                armorLeftRenderer.sprite =null; 
                armorRightRenderer.color = Color.white;
                armorLeftRenderer.color = Color.white;
                break;
            case ItemType.Hair:
                hair.sprite = null;
                hair.color = Color.white;
                helmetRenderer.gameObject.SetActive(true); 
                break;

        }
    }

}
