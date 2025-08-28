using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public Dictionary<ItemType, ItemInstance> equippedItems = new();
    private Dictionary<ItemInstance, List<StatModifier>> itemModifiers = new();

    [SerializeField] private PlayerEquipment playerEquipment;
    [SerializeField] private PlayerEquipment playerEquipmentHourse;
    public InventoryUI inventoryUI;


    public List<EquipmentData> ToData()
    {
        var data = new List<EquipmentData>();

        foreach (var kvp in equippedItems)
        {
            EquipmentData eData = new EquipmentData();
            eData.slotType = kvp.Key;

            if (kvp.Value != null)
                eData.equippedItem = kvp.Value.ToData();

            data.Add(eData);
        }

        return data;
    }

    public void FromData(List<EquipmentData> data, ItemDatabase db, PlayerStats stats)
    {
        equippedItems.Clear();

        foreach (var eData in data)
        {
            ItemInstance instance = null;

            if (eData.equippedItem != null)
            {
                ItemData refData = db.GetItemByID(eData.equippedItem.itemID);
                if (refData != null)
                {
                    instance = new ItemInstance(refData, eData.equippedItem.upgradeLevel);
                }
            }

            equippedItems[eData.slotType] = instance;

            if (instance != null)
            {
                playerEquipment?.UpdateEquipment(instance.itemData);
                playerEquipmentHourse?.UpdateEquipment(instance.itemData);
                //ApplyItemStats(instance, stats);
            }
        }
    }

    public void ReapplyEquipmentStats(PlayerStats stats)
    {
        //stats.ResetToBaseStats();

        foreach (var kvp in equippedItems)
        {
            var item = kvp.Value;
            if (item != null)
            {
                ApplyItemStats(item, stats);
            }
        }
    }


    public void EquipItem(ItemInstance instance, PlayerStats stats)
    {
        if (instance == null || instance.itemData == null) return;

        ItemType type = instance.itemData.itemType;
        if (type == ItemType.Consumable)
        {
            stats.Consume(instance.itemData);

            // Trừ khỏi inventory
            inventoryUI.inventory.RemoveItem(instance);
            inventoryUI.UpdateInventoryUI();
            return;
        }
        
        if (equippedItems.ContainsKey(instance.itemData.itemType))
        {
            UnequipItem(instance.itemData.itemType, stats);
        }

        playerEquipment?.UpdateEquipment(instance.itemData);
        playerEquipmentHourse?.UpdateEquipment(instance.itemData);

        equippedItems[instance.itemData.itemType] = instance;

        ApplyItemStats(instance, stats);
    }

    public ItemInstance UnequipItem(ItemType type, PlayerStats stats)
    {
        if (!equippedItems.TryGetValue(type, out ItemInstance instance)) return null;

        playerEquipment?.RemoveEquipment(type);
        playerEquipmentHourse?.RemoveEquipment(type);

        RemoveItemStats(instance, stats);
        equippedItems.Remove(type);

        return instance;
    }

    private void ApplyItemStats(ItemInstance instance, PlayerStats stats)
    {
        if (instance.itemData == null) return;

        float upgradePercent = 0.1f * (instance.upgradeLevel - 1);
        List<StatModifier> appliedMods = new();

        void AddStat(ItemStatBonus bonus, StatType type)
        {
            if (bonus == null) return;

            if (bonus.flat != 0)
            {
                float upgradedFlat = bonus.flat + bonus.flat * upgradePercent;
                var mod = new StatModifier(type, upgradedFlat, StatModType.Flat);
                stats.ApplyStatModifier(mod);
                appliedMods.Add(mod);
            }

            if (bonus.percent != 0)
            {
                var mod = new StatModifier(type, bonus.percent, StatModType.Percent);
                stats.ApplyStatModifier(mod);
                appliedMods.Add(mod);
            }
        }

        AddStat(instance.itemData.health, StatType.MaxHealth);
        AddStat(instance.itemData.mana, StatType.MaxMana);
        AddStat(instance.itemData.attack, StatType.Attack);
        AddStat(instance.itemData.defense, StatType.Defense);
        AddStat(instance.itemData.speed, StatType.Speed);
        AddStat(instance.itemData.critChance, StatType.CritChance);
        AddStat(instance.itemData.attackSpeed, StatType.AttackSpeed);
        AddStat(instance.itemData.lifeSteal, StatType.LifeSteal);

        itemModifiers[instance] = appliedMods;
    }

    private void RemoveItemStats(ItemInstance instance, PlayerStats stats)
    {
        if (instance == null) return;
        if (!itemModifiers.TryGetValue(instance, out var mods)) return;

        foreach (var mod in mods)
        {
            stats.RemoveStatModifier(mod);
        }

        itemModifiers.Remove(instance);
    }
}