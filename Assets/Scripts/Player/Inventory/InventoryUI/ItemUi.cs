using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public Image backgroundImage;
    public Image selectedImage; // ảnh viền sáng / border chọn
    public TMP_Text name;
    public ItemIconHandler icon;
    public TMP_Text lv;

    private ItemInstance itemData;
    private InventoryUI inventoryUI;

    public void Setup(ItemInstance data, InventoryUI ui)
    {
        itemData = data;
        inventoryUI = ui;

        icon.SetupIcons(data);
        name.text = data.itemData.itemName;
        lv.text = data.upgradeLevel > 0 ? $" {data.upgradeLevel}" : "";

        backgroundImage.color = ItemUtility.GetColorByTier(data.itemData.tier);
        selectedImage.gameObject.SetActive(false); // ẩn viền khi load

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(OnItemClicked);
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

    public ItemInstance GetItemData() => itemData;
}