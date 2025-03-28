using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EquipmentUI : MonoBehaviour
{
    public Equipment equipmentManager;
    public InventoryUI inventoryUI;
    public PlayerStats playerStats;

    // Tham chiếu các slot trang bị
    public Image weaponSlot;
    public Button weaponButton;
    public Image armorSlot;
    public Button armorButton;

    private Dictionary<ItemType, Image> slotMapping;
    private Dictionary<ItemType, Button> buttonMapping;

    private void Start()
    {
        slotMapping = new Dictionary<ItemType, Image>
        {
            { ItemType.Weapon, weaponSlot },
            { ItemType.Armor, armorSlot }
        };

        buttonMapping = new Dictionary<ItemType, Button>
        {
            { ItemType.Weapon, weaponButton },
            { ItemType.Armor, armorButton }
        };

        // Gán sự kiện gỡ trang bị khi bấm vào slot
        weaponButton.onClick.AddListener(() => UnequipItem(ItemType.Weapon));
        armorButton.onClick.AddListener(() => UnequipItem(ItemType.Armor));

        UpdateEquipmentUI();
    }

    public void EquipItem(ItemData item)
    {
        if (equipmentManager.equippedItems.ContainsKey(item.itemType))
        {
            UnequipItem(item.itemType); // Gỡ item cũ trước khi trang bị mới
        }

        equipmentManager.EquipItem(item, playerStats);
        UpdateEquipmentUI();
    }

    public void UnequipItem(ItemType itemType)
    {
        if (equipmentManager.equippedItems.ContainsKey(itemType))
        {
            ItemData removedItem = equipmentManager.equippedItems[itemType];

            // Gỡ item khỏi Equipment
            equipmentManager.UnequipItem(itemType, playerStats);

            // Chỉ thêm lại vào Inventory nếu gỡ bỏ thành công
            inventoryUI.inventory.AddItem(removedItem);

            // Cập nhật lại UI
            UpdateEquipmentUI();
            inventoryUI.UpdateInventoryUI();
        }
    }


    public void UpdateEquipmentUI()
    {
        foreach (var slot in slotMapping)
        {
            if (equipmentManager.equippedItems.ContainsKey(slot.Key))
            {
                slot.Value.sprite = equipmentManager.equippedItems[slot.Key].icon;
                slot.Value.color = Color.white;
                buttonMapping[slot.Key].gameObject.SetActive(true); // Hiện nút gỡ bỏ
            }
            else
            {
                slot.Value.sprite = null;
                slot.Value.color = new Color(1, 1, 1, 0); // Làm trong suốt nếu không có item
                buttonMapping[slot.Key].gameObject.SetActive(false); // Ẩn nút gỡ bỏ
            }
        }
    }
}
