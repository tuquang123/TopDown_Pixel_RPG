using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EquipmentUI : MonoBehaviour
{
    Equipment equipmentManager;
    public InventoryUI inventoryUI;
    PlayerStats playerStats;

    public EquipmentSlotUI weaponSlot;
    public EquipmentSlotUI armorSlot;
    public EquipmentSlotUI helmetSlot;
    public EquipmentSlotUI bootsSlot;
    public EquipmentSlotUI horseSlot;
    
    public EquipmentSlotUI cloakSlot;
    public EquipmentSlotUI specialArmorSlot;
    public EquipmentSlotUI hairSlot;

    private Dictionary<ItemType, EquipmentSlotUI> slotMapping;
    private bool isInitialized;

    public List<ItemType> GetAllEquippedTypes()
    {
        EnsureInitialized();
        return new List<ItemType>(equipmentManager.equippedItems.Keys);
    }

    private void Start()
    {
        EnsureInitialized();
    }

    /// <summary>
    /// Đảm bảo equipmentManager/playerStats/slotMapping đã sẵn sàng,
    /// bất kể object này có bị inactive (chưa từng gọi Start) hay không.
    /// </summary>
    private void EnsureInitialized()
    {
        if (isInitialized) return;

        playerStats = CommonReferent.Instance.playerPrefab.GetComponent<PlayerStats>();
        equipmentManager = CommonReferent.Instance.playerPrefab.GetComponent<Equipment>();

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

        UpdateEquipmentUI();

        foreach (var kvp in slotMapping)
        {
            ItemType type = kvp.Key;
            EquipmentSlotUI slotUI = kvp.Value;

            ItemType capturedType = type;

            slotUI.button.onClick.AddListener(() => UnequipItem(capturedType));
            slotUI.iconButton.onClick.AddListener(() =>
            {
                SelectSlot(slotUI); // highlight the clicked slot
                ShowEquippedItemDetail(capturedType);
            });
        }

        isInitialized = true;
    }

    public bool IsItemEquipped(ItemInstance item)
    {
        EnsureInitialized();
        if (item == null) return false;

        foreach (var equipped in equipmentManager.equippedItems.Values)
        {
            if (equipped != null && equipped.instanceID == item.instanceID)
                return true;
        }
        return false;
    }

    private void ShowEquippedItemDetail(ItemType type)
    {
        if (!equipmentManager.equippedItems.TryGetValue(type, out ItemInstance instance)) return;

        inventoryUI.itemDetailPanel.Show(instance, inventoryUI);

        inventoryUI.itemDetailPanel.equipButton.onClick.RemoveAllListeners();
        inventoryUI.itemDetailPanel.equipButton.onClick.AddListener(() =>
        {
            UnequipItem(type);
            inventoryUI.itemDetailPanel.Hide();
        });

        inventoryUI.itemDetailPanel.equipButton.GetComponentInChildren<TMPro.TMP_Text>().text =
            LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("unequip") : "Unequip";
    }
    
    public void EquipItem(ItemInstance itemInstance)
    {
        EnsureInitialized();

        if (itemInstance == null || itemInstance.itemData == null) return;

        ItemType type = itemInstance.itemData.itemType;

        GameEvents.OnEquipItemRange.Raise(itemInstance.itemData.weaponCategory);

        if (equipmentManager.equippedItems.ContainsKey(type))
        {
            UnequipItem(type);
        }

        equipmentManager.EquipItem(itemInstance, playerStats);
        inventoryUI.Inventory.RemoveItem(itemInstance);

        UpdateEquipmentUI();
        inventoryUI.UpdateInventoryUI();
    }
    
    public void UnequipItem(ItemType itemType)
    {
        EnsureInitialized();

        float beforePower = PlayerStats.Instance.CurrentPower;

        ItemInstance unequipped = equipmentManager.UnequipItem(itemType, playerStats);
        if (unequipped == null) return;

        inventoryUI.Inventory.AddItem(unequipped);

        PlayerStats.Instance.CalculatePower();

        float afterPower = PlayerStats.Instance.CurrentPower;
        ShowPowerDiff(beforePower, afterPower);

        UpdateEquipmentUI();
        inventoryUI.UpdateInventoryUI();
    }
    
    public void UpdateEquipmentUI()
    {
        if (slotMapping == null) return;

        foreach (var kvp in slotMapping)
        {
            ItemType type = kvp.Key;
            EquipmentSlotUI slotUI = kvp.Value;

            if (equipmentManager.equippedItems.TryGetValue(type, out ItemInstance item))
            {
                slotUI.icon.SetupIcons(item);
                slotUI.iconDefault.gameObject.SetActive(false);

                slotUI.background.sprite = CommonReferent.Instance.itemTierColorConfig.GetBackground(item.itemData.tier);
                slotUI.background.color = Color.white; // REQUIRED, otherwise the sprite will be transparent

                slotUI.button.gameObject.SetActive(true);
            }
            else
            {
                slotUI.icon.HideAllIcons();
                slotUI.iconDefault.gameObject.SetActive(true);
                slotUI.background.sprite = null;
                slotUI.background.color = new Color(1, 1, 1, 0); // transparent
                slotUI.button.gameObject.SetActive(false);
            }
        }
    }
    
    private EquipmentSlotUI currentSelectedSlot;

    public void SelectSlot(EquipmentSlotUI newSlot)
    {
        if (currentSelectedSlot != null)
            currentSelectedSlot.SetSelected(false); // turn off the old slot's highlight

        currentSelectedSlot = newSlot;
        currentSelectedSlot.SetSelected(true); // turn on the new slot's highlight
    }

    public ItemInstance GetEquippedItem(ItemType type)
    {
        EnsureInitialized();
        equipmentManager.equippedItems.TryGetValue(type, out var item);
        return item;
    }

    private void ShowPowerDiff(float before, float after)
    {
        float diff = after - before;
        if (Mathf.Approximately(diff, 0)) return;

        string text;

        if (diff > 0)
            text = $"<color=#00FF00>+{diff:N0} {(LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("power") : "Power")}</color>";
        else
            text = $"<color=#FF4D4D>{diff:N0} {(LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("power") : "Power")}</color>";

        GameEvents.OnShowToast.Raise(text);
    }
}
