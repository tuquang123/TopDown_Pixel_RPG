using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public Image backgroundImage;
    
    public TMP_Text name;

    public ItemIconHandler icon;

    private ItemInstance itemData;
    private InventoryUI inventoryUI;

    public void Setup(ItemInstance data, InventoryUI ui)
    {
        itemData = data;
        inventoryUI = ui;

        icon.SetupIcons(data);
        
        name.text = data.itemData.itemName;

        // Background màu theo tier
        backgroundImage.color = ItemUtility.GetColorByTier(data.itemData.tier);

        // Click để show detail
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(OnItemClicked);
    }


    private void OnItemClicked()
    {
        inventoryUI.itemDetailPanel.Hide();
        inventoryUI.itemDetailPanel.ShowDetails(itemData, inventoryUI);
    }
    
}