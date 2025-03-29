using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public Dictionary<ItemType, ItemData> equippedItems = new Dictionary<ItemType, ItemData>();

    private PlayerEquipment playerEquipment;

    private void Start()
    {
        playerEquipment = FindObjectOfType<PlayerEquipment>(); // Tìm PlayerEquipment
    }

    public void EquipItem(ItemData item, PlayerStats playerStats)
    {
        if (item == null || item.itemType == ItemType.Consumable)
            return;

        // Nếu đã có trang bị cùng loại, gỡ bỏ trang bị cũ
        if (equippedItems.ContainsKey(item.itemType))
        {
            UnequipItem(item.itemType, playerStats);
        }

        equippedItems[item.itemType] = item;
        ApplyStats(item, playerStats);

        // ✅ Cập nhật hình ảnh trang bị
        playerEquipment?.UpdateEquipment(item);

        Debug.Log($"Đã trang bị {item.itemName}.");
    }

    public void UnequipItem(ItemType type, PlayerStats playerStats)
    {
        if (equippedItems.ContainsKey(type))
        {
            ItemData removedItem = equippedItems[type];

            RemoveStats(removedItem, playerStats);
            equippedItems.Remove(type);

            // ✅ Gỡ bỏ hình ảnh trang bị
            playerEquipment?.RemoveEquipment(type);

            Debug.Log($"Gỡ bỏ {removedItem.itemName}, kiểm tra inventory!");
        }
    }
    private void ApplyStats(ItemData item, PlayerStats playerStats)
    {
        switch (item.itemType)
        {
            case ItemType.Weapon:
                playerStats.damage += item.value;
                break;
            case ItemType.Armor:
                playerStats.maxHealth += item.value;
                break;
            case ItemType.Helmet:
                playerStats.armor += item.value;  // Cộng giá trị phòng thủ cho mũ
                break;
            case ItemType.Boots:
                playerStats.speed += item.value;  // Cộng giá trị tốc độ cho giày
                break;
            case ItemType.Hair:
                // Tóc không thay đổi stats, chỉ thay đổi hình ảnh
                break;
        }
    }

    private void RemoveStats(ItemData item, PlayerStats playerStats)
    {
        switch (item.itemType)
        {
            case ItemType.Weapon:
                playerStats.damage -= item.value;
                break;
            case ItemType.Armor:
                playerStats.maxHealth -= item.value;
                break;
            case ItemType.Helmet:
                playerStats.armor -= item.value;  // Trừ giá trị phòng thủ khi gỡ mũ
                break;
            case ItemType.Boots:
                playerStats.speed -= item.value;  // Trừ giá trị tốc độ khi gỡ giày
                break;
            case ItemType.Hair:
                // Tóc không thay đổi stats, chỉ thay đổi hình ảnh
                break;
        }
    }
}
