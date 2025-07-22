using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EquipmentUI : MonoBehaviour
{
    public Equipment equipmentManager;
    public InventoryUI inventoryUI;
    public PlayerStats playerStats;

    public EquipmentSlotUI weaponSlot;
    public EquipmentSlotUI armorSlot;
    public EquipmentSlotUI helmetSlot;
    public EquipmentSlotUI bootsSlot;
    public EquipmentSlotUI horseSlot;

    private Dictionary<ItemType, EquipmentSlotUI> slotMapping;

    private void Start()
    {
        slotMapping = new Dictionary<ItemType, EquipmentSlotUI>
        {
            { ItemType.Weapon, weaponSlot },
            { ItemType.Armor, armorSlot },
            { ItemType.Helmet, helmetSlot },
            { ItemType.Boots, bootsSlot },
            { ItemType.Horse, horseSlot }
        };
        
        foreach (var kvp in slotMapping)
        {
            ItemType type = kvp.Key;
            EquipmentSlotUI slotUI = kvp.Value;

            slotUI.button.onClick.AddListener(() => UnequipItem(type));
            slotUI.iconButton.onClick.AddListener(() => ShowEquippedItemDetail(type));
        }

        UpdateEquipmentUI();
    }
    
    public bool IsItemEquipped(ItemInstance item)
    {
        foreach (var equipped in equipmentManager.equippedItems.Values)
        {
            if (equipped == item)
                return true;
        }
        return false;
    }

    
    private void ShowEquippedItemDetail(ItemType type)
    {
        if (!equipmentManager.equippedItems.TryGetValue(type, out ItemInstance instance)) return;

        inventoryUI.itemDetailPanel.ShowDetails(instance, inventoryUI);

        inventoryUI.itemDetailPanel.equipButton.onClick.RemoveAllListeners();
        inventoryUI.itemDetailPanel.equipButton.onClick.AddListener(() =>
        {
            UnequipItem(type);
            inventoryUI.itemDetailPanel.Hide();
        });

        inventoryUI.itemDetailPanel.equipButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Gỡ trang bị";
    }
    
    public void EquipItem(ItemInstance itemInstance)
    {
        if (itemInstance == null || itemInstance.itemData == null) return;

        ItemType type = itemInstance.itemData.itemType;

        if (equipmentManager.equippedItems.ContainsKey(type))
        {
            UnequipItem(type);
        }

        equipmentManager.EquipItem(itemInstance, playerStats);
        inventoryUI.inventory.RemoveItem(itemInstance);

        UpdateEquipmentUI();
        inventoryUI.UpdateInventoryUI();
    }
    
    public void UnequipItem(ItemType itemType)
    {
        ItemInstance unequipped = equipmentManager.UnequipItem(itemType, playerStats);
        if (unequipped == null) return;

        inventoryUI.inventory.AddItem(unequipped);
        UpdateEquipmentUI();
        inventoryUI.UpdateInventoryUI();
    }
    
    public void UpdateEquipmentUI()
    {
        foreach (var kvp in slotMapping)
        {
            ItemType type = kvp.Key;
            EquipmentSlotUI slotUI = kvp.Value;

            if (equipmentManager.equippedItems.TryGetValue(type, out ItemInstance item))
            {
                slotUI.icon.sprite = item.itemData.icon;
                slotUI.icon.color = Color.white;
                slotUI.background.color = ItemUtility.GetColorByTier(item.itemData.tier);
                slotUI.button.gameObject.SetActive(true);
            }
            else
            {
                slotUI.icon.sprite = null;
                slotUI.icon.color = new Color(1, 1, 1, 0); // Làm trong suốt
                slotUI.background.color = new Color(1, 1, 1, 0); // Làm mờ nền
                slotUI.button.gameObject.SetActive(false);
            }
        }
    }
}