using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public Image itemIcon;
    public Image backgroundImage; 

    private ItemData itemData;
    private InventoryUI inventoryUI;

    public void Setup(ItemData data, InventoryUI ui)
    {
        itemData = data;
        inventoryUI = ui;

        itemIcon.sprite = itemData.icon;
        backgroundImage.color = GetColorByTier(itemData.tier);

        GetComponent<Button>().onClick.AddListener(OnItemClicked);
    }

    private void OnItemClicked()
    {
        inventoryUI.itemDetailPanel.Hide();
        inventoryUI.itemDetailPanel.ShowDetails(itemData, inventoryUI);
    }

    private Color GetColorByTier(ItemTier tier)
    {
        switch (tier)
        {
            case ItemTier.Common:    return new Color(0.8f, 0.8f, 0.8f); // xám
            case ItemTier.Rare:      return new Color(0.2f, 0.4f, 1f);   // xanh dương
            case ItemTier.Epic:      return new Color(0.6f, 0.2f, 0.8f); // tím
            case ItemTier.Legendary: return new Color(1f, 0.6f, 0f);     // cam
            default: return Color.white;
        }
    }
}