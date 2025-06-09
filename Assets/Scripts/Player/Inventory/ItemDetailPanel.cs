using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class ItemDetailPanel : MonoBehaviour
{
    public TMP_Text nameText;         // ← TextMeshPro
    public TMP_Text descriptionText;
    public TMP_Text statText;
    public Button equipButton;
    
    public Button upgradeButton;
    public TMP_Text upgradeCostText;
    
    private ItemData currentItem;
    private InventoryUI inventoryUI;
    public PlayerStats playerStats;

    public void ShowDetails(ItemData item, InventoryUI ui)
    {
        currentItem = item;
        inventoryUI = ui;

        nameText.text = item.itemName;
        descriptionText.text = item.description;
        
        int cost = currentItem.baseUpgradeCost * currentItem.upgradeLevel;
        upgradeCostText.text = $"Nâng cấp ({cost} vàng)";

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(UpgradeItem);
        
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
            stats += $"Speed Attack: {item.attackSpeed}\n";
        if (item.lifeSteal != 0)
            stats += $"Life Steal: {item.lifeSteal}%\n";
        
        nameText.text = $"{item.itemName} +{item.upgradeLevel}";
        
        statText.text = stats.TrimEnd(); // Loại bỏ dòng trống cuối cùng nếu có

        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(EquipItem);

        gameObject.SetActive(true);
    }
    
    private void UpgradeItem()
    {
        int upgradeCost = currentItem.baseUpgradeCost * currentItem.upgradeLevel;

        if (CurrencyManager.Instance.Gold >= upgradeCost)
        {
            CurrencyManager.Instance.SpendGold(upgradeCost);
            currentItem.upgradeLevel++;

            // Tăng chỉ số theo tỷ lệ hoặc cố định
            currentItem.attackPower += Mathf.RoundToInt(currentItem.attackPower * 0.1f);
            currentItem.defense += Mathf.RoundToInt(currentItem.defense * 0.1f);
            currentItem.healthBonus += Mathf.RoundToInt(currentItem.healthBonus * 0.1f);
            currentItem.manaBonus += Mathf.RoundToInt(currentItem.manaBonus * 0.1f);

            currentItem.critChance += 1f;
            currentItem.attackSpeed += 0.05f;
            currentItem.lifeSteal += 0.5f;
            currentItem.moveSpeed += 0.05f;

            Debug.Log($"Nâng cấp thành công! Cấp độ mới: {currentItem.upgradeLevel}");

            ShowDetails(currentItem, inventoryUI); // refresh UI
        }
        else
        {
            Debug.Log("Không đủ vàng để nâng cấp.");
        }
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