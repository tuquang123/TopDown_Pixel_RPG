using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public Image itemIcon;
    public Image backgroundImage;

    private ItemInstance itemData;
    private InventoryUI inventoryUI;

    public void Setup(ItemInstance data, InventoryUI ui)
    {
        itemData = data;
        inventoryUI = ui;

        itemIcon.sprite = data.itemData.icon;
        backgroundImage.color = ItemUtility.GetColorByTier(data.itemData.tier);

        GetComponent<Button>().onClick.AddListener(OnItemClicked);
    }

    private void OnItemClicked()
    {
        inventoryUI.itemDetailPanel.Hide();
        inventoryUI.itemDetailPanel.ShowDetails(itemData, inventoryUI);
    }
    
}