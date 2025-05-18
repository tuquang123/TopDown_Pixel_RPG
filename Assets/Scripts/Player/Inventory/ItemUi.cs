using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public Image itemIcon;
    private ItemData itemData;
    private InventoryUI inventoryUI;

    public void Setup(ItemData data, InventoryUI ui)
    {
        itemData = data;
        inventoryUI = ui;

        itemIcon.sprite = itemData.icon;

        GetComponent<Button>().onClick.AddListener(OnItemClicked);
    }

    private void OnItemClicked()
    {
        // Ẩn các panel khác (nếu có)
        inventoryUI.itemDetailPanel.Hide();

        // Hiện panel chi tiết + nút trang bị
        inventoryUI.itemDetailPanel.ShowDetails(itemData, inventoryUI);
    }
}
