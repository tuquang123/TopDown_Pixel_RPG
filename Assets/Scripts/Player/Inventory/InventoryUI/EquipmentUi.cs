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
    
    public EquipmentSlotUI cloakSlot;
    public EquipmentSlotUI specialArmorSlot;
    public EquipmentSlotUI hairSlot;


    private Dictionary<ItemType, EquipmentSlotUI> slotMapping;

    private void Start()
    {
        slotMapping = new Dictionary<ItemType, EquipmentSlotUI>
        {
            { ItemType.Weapon, weaponSlot },
            { ItemType.Clother, armorSlot },
            { ItemType.Helmet, helmetSlot },
            { ItemType.Boots, bootsSlot },
            { ItemType.Horse, horseSlot },
            { ItemType.Cloak, cloakSlot },
            { ItemType.SpecialArmor, specialArmorSlot },
            { ItemType.Hair, hairSlot }
        };

        foreach (var kvp in slotMapping)
        {
            ItemType type = kvp.Key;
            EquipmentSlotUI slotUI = kvp.Value;

            ItemType capturedType = type; // avoid closure issue
            slotUI.button.onClick.AddListener(() => UnequipItem(capturedType));
            slotUI.iconButton.onClick.AddListener(() => ShowEquippedItemDetail(capturedType));
        }

        UpdateEquipmentUI();
        foreach (var kvp in slotMapping)
        {
            ItemType type = kvp.Key;
            EquipmentSlotUI slotUI = kvp.Value;

            ItemType capturedType = type;

            slotUI.button.onClick.AddListener(() => UnequipItem(capturedType));
            slotUI.iconButton.onClick.AddListener(() =>
            {
                SelectSlot(slotUI); // làm sáng slot được bấm
                ShowEquippedItemDetail(capturedType);
            });
        }

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
                slotUI.icon.SetupIcons(item);
                
                //slotUI.icon.color = Color.white;
                slotUI.iconDefault.gameObject.SetActive(false);
                slotUI.background.color = ItemUtility.GetColorByTier(item.itemData.tier);
                slotUI.button.gameObject.SetActive(true);
            }
            else
            {
                slotUI.icon.HideAllIcons();
                //slotUI.icon.color = new Color(1, 1, 1, 0); // Làm trong suốt
                slotUI.iconDefault.gameObject.SetActive(true);
                slotUI.background.color = new Color32(39, 39, 39, 255); // Màu #272727 full opacity
                slotUI.button.gameObject.SetActive(false);
            }
        }
    }
    private EquipmentSlotUI currentSelectedSlot;

    public void SelectSlot(EquipmentSlotUI newSlot)
    {
        if (currentSelectedSlot != null)
            currentSelectedSlot.SetSelected(false); // tắt sáng slot cũ

        currentSelectedSlot = newSlot;
        currentSelectedSlot.SetSelected(true); // bật sáng slot mới
    }

}