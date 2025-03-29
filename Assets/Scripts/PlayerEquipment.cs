using UnityEngine;
using System.Collections.Generic;

public class PlayerEquipment : MonoBehaviour
{
    public SpriteRenderer weaponRenderer;
    public SpriteRenderer armorRenderer;
    public SpriteRenderer armorLeftArmRenderer;  // ✅ Giáp tay trái
    public SpriteRenderer armorRightArmRenderer; // ✅ Giáp tay phải
    public SpriteRenderer helmetRenderer;
    public SpriteRenderer bootsLeftRenderer;
    public SpriteRenderer bootsRightRenderer;
    public SpriteRenderer hairRenderer;

    private Dictionary<ItemType, ItemData> equippedItems = new Dictionary<ItemType, ItemData>();

    public void UpdateEquipment(ItemData newItem)
{
    if (newItem == null) return;

    // Nếu trang bị là mũ, ẩn tóc
    if (newItem.itemType == ItemType.Helmet)
    {
        hairRenderer.gameObject.SetActive(false);  // Ẩn tóc khi trang bị mũ
    }
    else if (equippedItems.ContainsKey(ItemType.Helmet))
    {
        // Nếu đang có mũ, kiểm tra xem có trang bị tóc không
        if (!equippedItems.ContainsKey(ItemType.Hair))
        {
            hairRenderer.gameObject.SetActive(true);  // Hiển thị lại tóc nếu không còn mũ
        }
    }

    equippedItems[newItem.itemType] = newItem;

    // Cập nhật hình ảnh + màu sắc
    switch (newItem.itemType)
    {
        case ItemType.Weapon:
            weaponRenderer.sprite = newItem.icon;
            weaponRenderer.color = newItem.color;
            break;
        case ItemType.Armor:
            armorRenderer.sprite = newItem.icon;
            armorRenderer.color = newItem.color;
            armorLeftArmRenderer.sprite = newItem.iconLeft;  // ✅ Giáp tay trái
            armorRightArmRenderer.sprite = newItem.iconRight; // ✅ Giáp tay phải
            armorLeftArmRenderer.color = newItem.color;
            armorRightArmRenderer.color = newItem.color;
            break;
        case ItemType.Helmet:
            helmetRenderer.sprite = newItem.icon;
            helmetRenderer.color = newItem.color;
            break;
        case ItemType.Boots:
            bootsLeftRenderer.sprite = newItem.iconLeft;
            bootsRightRenderer.sprite = newItem.iconRight;
            bootsLeftRenderer.color = newItem.color;
            bootsRightRenderer.color = newItem.color;
            break;
        case ItemType.Hair:
            hairRenderer.sprite = newItem.icon;
            hairRenderer.color = newItem.color;
            break;
    }
}

public void RemoveEquipment(ItemType type)
{
    if (!equippedItems.ContainsKey(type)) return;

    equippedItems.Remove(type);

    // Kiểm tra lại khi gỡ mũ, hiển thị tóc nếu không có mũ
    if (type == ItemType.Helmet)
    {
        // Nếu không còn mũ, kiểm tra xem tóc có trang bị hay không
        if (!equippedItems.ContainsKey(ItemType.Hair))
        {
            hairRenderer.gameObject.SetActive(true);  // Hiển thị lại tóc
        }
    }

    // Xóa hình ảnh + reset màu sắc
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
            break;
        case ItemType.Boots:
            bootsLeftRenderer.sprite = null;
            bootsRightRenderer.sprite = null;
            bootsLeftRenderer.color = Color.white;
            bootsRightRenderer.color = Color.white;
            break;
        case ItemType.Hair:
            hairRenderer.sprite = null;
            hairRenderer.color = Color.white;
            break;
    }
}

}
