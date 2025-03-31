using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public Dictionary<ItemType, ItemData> equippedItems = new Dictionary<ItemType, ItemData>();
    private Dictionary<ItemType, StatModifier> statModifiers = new Dictionary<ItemType, StatModifier>(); // Lưu modifiers để gỡ bỏ chính xác
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

        // Tạo modifier và lưu lại
        StatModifier modifier = new StatModifier(ConvertToStatType(item.itemType), item.value);
        statModifiers[item.itemType] = modifier;

        equippedItems[item.itemType] = item;
        playerStats.ApplyStatModifier(modifier); // Áp dụng modifier

        // ✅ Cập nhật hình ảnh trang bị
        playerEquipment?.UpdateEquipment(item);

        Debug.Log($"Đã trang bị {item.itemName}. + value: {playerStats.attack.Value}");
    }

    public void UnequipItem(ItemType type, PlayerStats playerStats)
    {
        if (equippedItems.ContainsKey(type))
        {
            ItemData removedItem = equippedItems[type];

            // Xóa đúng modifier đã lưu
            if (statModifiers.TryGetValue(type, out StatModifier modifier))
            {
                playerStats.RemoveStatModifier(modifier);
                statModifiers.Remove(type);
            }

            equippedItems.Remove(type);

            // ✅ Gỡ bỏ hình ảnh trang bị
            playerEquipment?.RemoveEquipment(type);

            Debug.Log($"Gỡ bỏ {removedItem.itemName}, + value: {playerStats.attack.Value}  kiểm tra inventory!");
        }
    }

    private StatType ConvertToStatType(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Weapon => StatType.Attack,
            ItemType.Armor => StatType.MaxHealth,
            ItemType.Helmet => StatType.Defense,
            ItemType.Boots => StatType.Speed,
            _ => throw new System.ArgumentException("Loại item không hợp lệ")
        };
    }
}
