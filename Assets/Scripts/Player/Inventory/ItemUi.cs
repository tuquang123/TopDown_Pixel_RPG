using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public Image itemIcon;
    public Button equipButton;

    private ItemData itemData;
    private InventoryUI inventoryUI;

    public void Setup(ItemData data, InventoryUI ui)
    {
        itemData = data;
        inventoryUI = ui;

        itemIcon.sprite = itemData.icon;
        equipButton.gameObject.SetActive(false); // Ẩn nút "Trang bị" ban đầu

        GetComponent<Button>().onClick.AddListener(OnItemClicked);
        equipButton.onClick.AddListener(EquipItem);
    }


    private void OnItemClicked()
    {
        // Ẩn tất cả nút "Trang bị" trước khi bật cái mới
        foreach (Transform child in inventoryUI.itemContainer)
        {
            ItemUI itemUI = child.GetComponent<ItemUI>();
            if (itemUI != null)
            {
                itemUI.equipButton.gameObject.SetActive(false);
            }
        }

        equipButton.gameObject.SetActive(true); // Hiện nút "Trang bị" cho item được bấm
    }

    
    public void EquipItem()
    {
        // Ẩn nút "Trang bị" sau khi bấm
        equipButton.gameObject.SetActive(false);

        // Trang bị item
        //inventoryUI.equipmentManager.EquipItem(itemData, inventoryUI.playerStats);
        inventoryUI.equipmentUI.EquipItem(itemData);

        // Xóa item khỏi Inventory UI (CHỈ NẾU TRANG BỊ THÀNH CÔNG)
        if (inventoryUI.inventory.RemoveItem(itemData))
        {
            inventoryUI.UpdateInventoryUI();
        }
    }

}