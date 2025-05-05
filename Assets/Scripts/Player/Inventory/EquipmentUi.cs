using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EquipmentUI : MonoBehaviour
{
    public Equipment equipmentManager;
    public InventoryUI inventoryUI;
    public PlayerStats playerStats;

    // Các slot UI
    public Image weaponSlot;
    public Button weaponButton;
    public Image armorSlot;
    public Button armorButton;
    public Image helmetSlot;
    public Button helmetButton;
    public Image bootsSlot;
    public Button bootsButton;
    public Image horseSlot;           // 🐎 Slot ngựa
    public Button horseButton;        // 🐎 Nút gỡ ngựa

    private Dictionary<ItemType, Image> slotMapping;
    private Dictionary<ItemType, Button> buttonMapping;

    private void Start()
    {
        // Ánh xạ slot và nút theo loại item
        slotMapping = new Dictionary<ItemType, Image>
        {
            { ItemType.Weapon, weaponSlot },
            { ItemType.Armor, armorSlot },
            { ItemType.Helmet, helmetSlot },
            { ItemType.Boots, bootsSlot },
            { ItemType.Horse, horseSlot } // 🐎
        };

        buttonMapping = new Dictionary<ItemType, Button>
        {
            { ItemType.Weapon, weaponButton },
            { ItemType.Armor, armorButton },
            { ItemType.Helmet, helmetButton },
            { ItemType.Boots, bootsButton },
            { ItemType.Horse, horseButton } // 🐎
        };

        // Gán sự kiện click để gỡ trang bị
        weaponButton.onClick.AddListener(() => UnequipItem(ItemType.Weapon));
        armorButton.onClick.AddListener(() => UnequipItem(ItemType.Armor));
        helmetButton.onClick.AddListener(() => UnequipItem(ItemType.Helmet));
        bootsButton.onClick.AddListener(() => UnequipItem(ItemType.Boots));
        horseButton.onClick.AddListener(() => UnequipItem(ItemType.Horse)); // 🐎

        UpdateEquipmentUI();
    }

    public void EquipItem(ItemData item)
    {
        if (equipmentManager.equippedItems.ContainsKey(item.itemType))
        {
            UnequipItem(item.itemType);
        }

        equipmentManager.EquipItem(item, playerStats);

        inventoryUI.inventory.RemoveItem(item);

        UpdateEquipmentUI();
        inventoryUI.UpdateInventoryUI();
    }

    public void UnequipItem(ItemType itemType)
    {
        if (!equipmentManager.equippedItems.ContainsKey(itemType)) return;

        ItemData removedItem = equipmentManager.equippedItems[itemType];

        equipmentManager.UnequipItem(itemType, playerStats);
        inventoryUI.inventory.AddItem(removedItem);

        UpdateEquipmentUI();
        inventoryUI.UpdateInventoryUI();
    }

    public void UpdateEquipmentUI()
    {
        foreach (var slot in slotMapping)
        {
            if (equipmentManager.equippedItems.ContainsKey(slot.Key))
            {
                slot.Value.sprite = equipmentManager.equippedItems[slot.Key].icon;
                slot.Value.color = Color.white;
                buttonMapping[slot.Key].gameObject.SetActive(true);
            }
            else
            {
                slot.Value.sprite = null;
                slot.Value.color = new Color(1, 1, 1, 0); // Làm trong suốt
                buttonMapping[slot.Key].gameObject.SetActive(false);
            }
        }
    }
}
