using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public Dictionary<ItemType, ItemData> equippedItems = new ();
    private Dictionary<ItemType, MultiStatModifier> statModifiers = new();
    [SerializeField] private PlayerEquipment playerEquipment;
    [SerializeField] private PlayerEquipment playerEquipmentHourse;
    
    /*public void EquipItem(ItemData item, PlayerStats playerStats)
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
        
        playerEquipmentHourse?.UpdateEquipment(item);
        
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
            
            playerEquipmentHourse?.RemoveEquipment(type);

            Debug.Log($"Gỡ bỏ {removedItem.itemName}, + value: {playerStats.attack.Value}  kiểm tra inventory!");
        }
    }*/
    
    
    public void UnequipItem(ItemType type, PlayerStats playerStats)
    {
        if (equippedItems.ContainsKey(type))
        {
            ItemData removedItem = equippedItems[type];

            if (statModifiers.TryGetValue(type, out MultiStatModifier multiMod))
            {
                foreach (var modifier in multiMod.modifiers)
                {
                    playerStats.RemoveStatModifier(modifier);
                }
                statModifiers.Remove(type);
            }

            equippedItems.Remove(type);

            playerEquipment?.RemoveEquipment(type);
            playerEquipmentHourse?.RemoveEquipment(type);

            Debug.Log($"Gỡ bỏ {removedItem.itemName}. Stats đã được gỡ.");
        }
    }
    public void EquipItem(ItemData item, PlayerStats playerStats)
    {
        if (item == null || item.itemType == ItemType.Consumable)
            return;

        if (equippedItems.ContainsKey(item.itemType))
        {
            UnequipItem(item.itemType, playerStats);
        }

        List<StatModifier> modifiers = new List<StatModifier>();

        if (item.attackPower != 0)    modifiers.Add(new StatModifier(StatType.Attack, item.attackPower));
        if (item.defense != 0)        modifiers.Add(new StatModifier(StatType.Defense, item.defense));
        if (item.healthBonus != 0)    modifiers.Add(new StatModifier(StatType.MaxHealth, item.healthBonus));
        if (item.manaBonus != 0)      modifiers.Add(new StatModifier(StatType.MaxMana, item.manaBonus));
        if (item.critChance != 0)     modifiers.Add(new StatModifier(StatType.CritChance, item.critChance));
        if (item.attackSpeed != 0)    modifiers.Add(new StatModifier(StatType.AttackSpeed, item.attackSpeed));
        if (item.lifeSteal != 0)      modifiers.Add(new StatModifier(StatType.LifeSteal, item.lifeSteal));
        if (item.moveSpeed != 0)      modifiers.Add(new StatModifier(StatType.Speed, item.moveSpeed));

        foreach (var modifier in modifiers)
        {
            playerStats.ApplyStatModifier(modifier);
        }

        equippedItems[item.itemType] = item;
        statModifiers[item.itemType] = new MultiStatModifier(modifiers);

        playerEquipment?.UpdateEquipment(item);
        playerEquipmentHourse?.UpdateEquipment(item);

        Debug.Log($"Đã trang bị {item.itemName} với {modifiers.Count} chỉ số.");
    }

    private StatType ConvertToStatType(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Weapon => StatType.Attack,
            ItemType.Armor => StatType.MaxHealth,
            ItemType.Helmet => StatType.Defense,
            ItemType.Boots => StatType.Speed,
            ItemType.Horse => StatType.Speed,
            _ => throw new System.ArgumentException("Loại item không hợp lệ")
        };
    }
}
