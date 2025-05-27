using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class ItemDetailPanel : MonoBehaviour
{
    public TMP_Text nameText;         // ← TextMeshPro
    public TMP_Text descriptionText;
    public TMP_Text statText;
    public Button equipButton;

    private ItemData currentItem;
    private InventoryUI inventoryUI;
    public PlayerStats playerStats;

    public void ShowDetails(ItemData item, InventoryUI ui)
    {
        currentItem = item;
        inventoryUI = ui;

        nameText.text = item.itemName;
        descriptionText.text = item.description;

        string stats = "";

        if (item.attackPower != 0)
            stats += $"Dame: {item.attackPower}\n";
        if (item.defense != 0)
            stats += $"Giáp: {item.defense}\n";
        if (item.healthBonus != 0)
            stats += $"Máu: {item.healthBonus}\n";
        if (item.manaBonus != 0)
            stats += $"Mana: {item.manaBonus}\n";
        if (item.critChance != 0)
            stats += $"Crit: {item.critChance}%\n";
        if (item.moveSpeed != 0)
            stats += $"Speed: {item.moveSpeed}\n";
        if (item.attackSpeed != 0)
            stats += $"Tốc đánh: {item.attackSpeed}\n";
        if (item.lifeSteal != 0)
            stats += $"Hút máu: {item.lifeSteal}%\n";

        statText.text = stats.TrimEnd(); // Loại bỏ dòng trống cuối cùng nếu có

        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(EquipItem);

        gameObject.SetActive(true);
    }


    private void EquipItem()
    {
        if (inventoryUI.equipmentUi != null && currentItem != null)
        {
            inventoryUI.equipmentUi.EquipItem(currentItem);

            if (inventoryUI.inventory.RemoveItem(currentItem))
            {
                inventoryUI.UpdateInventoryUI();
            }

            gameObject.SetActive(false); // Ẩn panel sau khi trang bị
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}