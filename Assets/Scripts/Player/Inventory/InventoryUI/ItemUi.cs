using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    [Header("UI")]
    public Image backgroundImage;
    public Image selectedImage;
    public TMP_Text nameText;
    public TMP_Text lvText;
    public ItemIconHandler icon;
    
  

// NEW
    public GameObject lockIconLocked;   // icon khóa
    public GameObject lockIconUnlocked; // icon mở khóa
    private ItemInstance itemData;
    private InventoryUI inventoryUI;
    public Button button;

    public void SetupDisplayOnly(ItemInstance data)
    {
        Setup(data, null);

        selectedImage.gameObject.SetActive(false);
        if (button != null)
            button.interactable = false;
    }

    public void RefreshLockState()
    {
        lockIconLocked.SetActive(itemData.isLocked);
        lockIconUnlocked.SetActive(!itemData.isLocked);
    }
    public void Setup(ItemInstance data, InventoryUI ui)
    {
        itemData = data;
        inventoryUI = ui;
        lockIconLocked.SetActive(data.isLocked);
        lockIconUnlocked.SetActive(!data.isLocked);
        icon.SetupIcons(data);

        nameText.text = data.itemData.itemName;
        lvText.text = data.upgradeLevel-1 > 0
            ? $"+{data.upgradeLevel-1}"
            : string.Empty;

        // ✅ ĐÚNG
        backgroundImage.sprite = CommonReferent.Instance.itemTierColorConfig.GetBackground(data.itemData.tier);

        selectedImage.gameObject.SetActive(false);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnItemClicked);
    }

    private void OnItemClicked()
    {
        if (inventoryUI == null)
            return;

        inventoryUI.SelectItem(this);
        inventoryUI.itemDetailPanel.Hide();
        inventoryUI.itemDetailPanel.Show(itemData, inventoryUI);
    }

    public void SetSelected(bool isSelected)
    {
        selectedImage.gameObject.SetActive(isSelected);
    }

    public ItemInstance GetItemData()
    {
        return itemData;
    }
}
