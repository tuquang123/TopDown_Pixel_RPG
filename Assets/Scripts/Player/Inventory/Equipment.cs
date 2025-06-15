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

        if (equippedItems.ContainsKey(item.itemType))
        {
            UnequipItem(item.itemType, playerStats);
        }

        List<StatModifier> modifiers = new List<StatModifier>();

        if (item.attackPower != 0)
            modifiers.Add(new StatModifier(StatType.Attack, item.attackPower));
        if (item.defense != 0)
            modifiers.Add(new StatModifier(StatType.Defense, item.defense));
        if (item.healthBonus != 0)
            modifiers.Add(new StatModifier(StatType.MaxHealth, item.healthBonus));
        if (item.manaBonus != 0)
            modifiers.Add(new StatModifier(StatType.MaxMana, item.manaBonus));
        if (item.critChance != 0)
            modifiers.Add(new StatModifier(StatType.CritChance, item.critChance));
        if (item.attackSpeed != 0)
            modifiers.Add(new StatModifier(StatType.AttackSpeed, item.attackSpeed));
        if (item.lifeSteal != 0)
            modifiers.Add(new StatModifier(StatType.LifeSteal, item.lifeSteal));
        if (item.moveSpeed != 0)
            modifiers.Add(new StatModifier(StatType.Speed, item.moveSpeed));

        foreach (var modifier in modifiers)
        {
            playerStats.ApplyStatModifier(modifier);
        }

        equippedItems[item.itemType] = item;
        statModifiers[item.itemType] = modifiers;
        ;

        playerEquipment?.UpdateEquipment(item);
        playerEquipmentHourse?.UpdateEquipment(item);

        Debug.Log($"Đã trang bị {item.itemName} với {modifiers.Count} chỉ số stat.");
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