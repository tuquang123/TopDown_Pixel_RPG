using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform itemContainer;
    public Inventory inventory;
    public Equipment equipment;
    public EquipmentUI equipmentUi;
    public PlayerStats playerStats;
    public GameObject itemPrefab; 

    public ItemDetailPanel itemDetailPanel; // ← kéo prefab panel detail vào trong Inspector
    
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

        foreach (ItemData item in inventory.items)
        {
            Debug.Log($"Tạo item: {item.itemName}");
            GameObject newItem = Instantiate(itemPrefab, itemContainer);
            newItem.GetComponent<ItemUI>().Setup(item, this);
        }

        itemDetailPanel.Hide();
    }

}
