using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public Dictionary<ItemType, ItemData> equippedItems = new Dictionary<ItemType, ItemData>();
    private Dictionary<ItemType, List<StatModifier>> statModifiers = new Dictionary<ItemType, List<StatModifier>>();
    [SerializeField] private PlayerEquipment playerEquipment;
    [SerializeField] private PlayerEquipment playerEquipmentHourse;

    public void EquipItemData(ItemData item, PlayerStats playerStats)
    {
        if (item == null || item.itemType == ItemType.Consumable)
            return;

        // Gỡ trang bị cũ nếu có
        if (equippedItems.ContainsKey(item.itemType))
        {
            UnequipItem(item.itemType, playerStats);
        }

        List<StatModifier> modifiers = new List<StatModifier>();

        // Helper function nội bộ
        void AddBonus(ItemStatBonus bonus, StatType type)
        {
            if (bonus == null) return;
            if (bonus.flat != 0)
                modifiers.Add(new StatModifier(type, bonus.flat, StatModType.Flat));
            if (bonus.percent != 0)
                modifiers.Add(new StatModifier(type, bonus.percent, StatModType.Percent));
        }

        // Add từng stat
        AddBonus(item.health, StatType.MaxHealth);
        AddBonus(item.mana, StatType.MaxMana);
        AddBonus(item.attack, StatType.Attack);
        AddBonus(item.defense, StatType.Defense);
        AddBonus(item.speed, StatType.Speed);
        AddBonus(item.critChance, StatType.CritChance);
        AddBonus(item.attackSpeed, StatType.AttackSpeed);
        AddBonus(item.lifeSteal, StatType.LifeSteal);

        // Apply stat modifier vào player
        foreach (var modifier in modifiers)
        {
            playerStats.ApplyStatModifier(modifier);
        }

        equippedItems[item.itemType] = item;
        statModifiers[item.itemType] = modifiers;

        playerEquipment?.UpdateEquipment(item);
        playerEquipmentHourse?.UpdateEquipment(item);

        Debug.Log($"Đã trang bị {item.itemName} với {modifiers.Count} stat modifiers.");
    }


    public void UnequipItem(ItemType type, PlayerStats playerStats)
    {
        if (equippedItems.ContainsKey(type))
        {
            ItemData removedItem = equippedItems[type];

            if (statModifiers.TryGetValue(type, out List<StatModifier> modifiers))
            {
                foreach (var modifier in modifiers)
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
}