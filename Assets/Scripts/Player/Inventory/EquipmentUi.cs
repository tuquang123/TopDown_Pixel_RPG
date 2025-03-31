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
    public Image helmetSlot;
    public Button helmetButton;
    public Image bootsSlot;
    public Button bootsButton;

    private Dictionary<ItemType, Image> slotMapping;
    private Dictionary<ItemType, Button> buttonMapping;

    private void Start()
    {
        // Cập nhật slot và button mapping cho các item mới
        slotMapping = new Dictionary<ItemType, Image>
        {
            { ItemType.Weapon, weaponSlot },
            { ItemType.Armor, armorSlot },
            { ItemType.Helmet, helmetSlot },
            { ItemType.Boots, bootsSlot },
        };

        buttonMapping = new Dictionary<ItemType, Button>
        {
            { ItemType.Weapon, weaponButton },
            { ItemType.Armor, armorButton },
            { ItemType.Helmet, helmetButton },
            { ItemType.Boots, bootsButton },
        };

        // Gán sự kiện gỡ trang bị khi bấm vào slot
        weaponButton.onClick.AddListener(() => UnequipItem(ItemType.Weapon));
        armorButton.onClick.AddListener(() => UnequipItem(ItemType.Armor));
        helmetButton.onClick.AddListener(() => UnequipItem(ItemType.Helmet));
        bootsButton.onClick.AddListener(() => UnequipItem(ItemType.Boots));

        UpdateEquipmentUI();
    }

    public void EquipItem(ItemData item)
    {
        if (equipmentManager.equippedItems.ContainsKey(item.itemType))
        {
            UnequipItem(item.itemType);
        }

        equipmentManager.EquipItem(item, playerStats);

        // Xóa item khỏi inventory sau khi trang bị
        inventoryUI.inventory.RemoveItem(item);

        UpdateEquipmentUI();
        inventoryUI.UpdateInventoryUI(); // Cập nhật lại inventory UI
    }

    public void UnequipItem(ItemType itemType)
    {
        if (equipmentManager.equippedItems.ContainsKey(itemType))
        {
            ItemData removedItem = equipmentManager.equippedItems[itemType];

            // Gỡ item khỏi Equipment
            equipmentManager.UnequipItem(itemType, playerStats);

            // Trả item về inventory
            inventoryUI.inventory.AddItem(removedItem);

            // Cập nhật UI
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

