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

    private ItemInstance itemData;
    private InventoryUI inventoryUI;
    public Button button;
    
    public void Setup(ItemInstance data, InventoryUI ui)
    {
        itemData = data;
        inventoryUI = ui;

        icon.SetupIcons(data);

        nameText.text = data.itemData.itemName;
        lvText.text = data.upgradeLevel > 0
            ? $"+{data.upgradeLevel}"
            : string.Empty;

        // ✅ ĐÚNG
        backgroundImage.sprite = CommonReferent.Instance.itemTierColorConfig.GetBackground(data.itemData.tier);

        selectedImage.gameObject.SetActive(false);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnItemClicked);
    }

    private void OnItemClicked()
    {
        inventoryUI.SelectItem(this);
        inventoryUI.itemDetailPanel.Hide();
        inventoryUI.itemDetailPanel.ShowDetails(itemData, inventoryUI);
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