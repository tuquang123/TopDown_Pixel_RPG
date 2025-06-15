using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EquipmentUI : MonoBehaviour
{
    [System.Serializable]
    public class EquipmentSlotUI
    {
        public Image icon;
        public Image background;
        public Button button;
    }

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
        // Ánh xạ các loại item với slot UI
        slotMapping = new Dictionary<ItemType, EquipmentSlotUI>
        {
            { ItemType.Weapon, weaponSlot },
            { ItemType.Armor, armorSlot },
            { ItemType.Helmet, helmetSlot },
            { ItemType.Boots, bootsSlot },
            { ItemType.Horse, horseSlot }
        };

        // Gán sự kiện nút để gỡ trang bị
        foreach (var kvp in slotMapping)
        {
            ItemType type = kvp.Key;
            kvp.Value.button.onClick.AddListener(() => UnequipItem(type));
        }

        UpdateEquipmentUI();
    }

    public void EquipItem(ItemInstance itemInstance)
    {
        if (itemInstance == null || itemInstance.itemData == null) return;

        ItemData item = itemInstance.itemData;

        if (equipmentManager.equippedItems.ContainsKey(item.itemType))
        {
            UnequipItem(item.itemType);
        }

        equipmentManager.EquipItemData(item, playerStats);
        inventoryUI.inventory.RemoveItem(itemInstance);

        UpdateEquipmentUI();
        inventoryUI.UpdateInventoryUI();
    }

    public void UnequipItem(ItemType itemType)
    {
        if (!equipmentManager.equippedItems.TryGetValue(itemType, out ItemData removedItem)) return;

        equipmentManager.UnequipItem(itemType, playerStats);
        inventoryUI.inventory.AddItem(new ItemInstance(removedItem)); // Tạo ItemInstance mới

        UpdateEquipmentUI();
        inventoryUI.UpdateInventoryUI();
    }

    public void UpdateEquipmentUI()
    {
        foreach (var kvp in slotMapping)
        {
            ItemType type = kvp.Key;
            EquipmentSlotUI slotUI = kvp.Value;

            if (equipmentManager.equippedItems.TryGetValue(type, out ItemData item))
            {
                slotUI.icon.sprite = item.icon;
                slotUI.icon.color = Color.white;
                slotUI.background.color = ItemUtility.GetColorByTier(item.tier);
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