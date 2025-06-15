using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform itemContainer;
    public Inventory inventory;
    public EquipmentUI equipmentUI;
    public PlayerStats playerStats;
    public GameObject itemPrefab;

    public ItemDetailPanel itemDetailPanel;
    public EquipmentUI equipmentUi;

    private void Start()
    {
        UpdateInventoryUI();
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
}