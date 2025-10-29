using UnityEngine;

public class InventoryUI : BasePopup
{
    public Transform itemContainer;
    public Inventory inventory;
    public GameObject itemPrefab;
    public EquipmentUI equipmentUi;

    public ItemDetailPanel itemDetailPanel;

    public override void Show()
    {
        base.Show();
        UpdateInventoryUI();
    }

    public override void Hide()
    {
        base.Hide();
        itemDetailPanel.Hide(); // Ẩn panel chi tiết nếu có
    }

    public void UpdateInventoryUI()
    {
        Debug.Log($"Reload Inventory UI - Số lượng item: {inventory.items.Count}");

        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (ItemInstance item in inventory.items)
        {
            Debug.Log($"Tạo item: {item.itemData.itemName}");
            GameObject newItem = Instantiate(itemPrefab, itemContainer);
            newItem.GetComponent<ItemUI>().Setup(item, this);
        }

        itemDetailPanel.Hide();
    }
    private ItemUI currentSelectedItem;
     
     public void SelectItem(ItemUI newItem)
     {
         if (currentSelectedItem != null)
             currentSelectedItem.SetSelected(false); // tắt chọn cũ
     
         currentSelectedItem = newItem;
         currentSelectedItem.SetSelected(true); // bật chọn mới
     }
}

