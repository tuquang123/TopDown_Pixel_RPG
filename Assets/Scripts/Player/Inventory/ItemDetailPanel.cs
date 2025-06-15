using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDetailPanel : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text statText;
    public Button equipButton;
    public Button upgradeButton;
    public TMP_Text upgradeCostText;
    public Button sellButton;
    public TMP_Text sellPriceText;

    private ItemInstance currentItem;
    private InventoryUI inventoryUI;
    public PlayerStats playerStats;

    private int CalculateSellPrice(ItemInstance item)
    {
        int baseValue = item.itemData.baseUpgradeCost;
        float upgradeMultiplier = 0.6f + (item.upgradeLevel * 0.2f);
        return Mathf.RoundToInt(baseValue * upgradeMultiplier);
    }

    private void SellItem()
    {
        int goldEarned = CalculateSellPrice(currentItem);
        CurrencyManager.Instance.AddGold(goldEarned);

        if (inventoryUI.inventory.RemoveItem(currentItem))
        {
            inventoryUI.UpdateInventoryUI();
        }

        Debug.Log($"Đã bán {currentItem.itemData.itemName} +{currentItem.upgradeLevel} với giá {goldEarned} vàng.");
        gameObject.SetActive(false);
    }

    public void ShowDetails(ItemInstance item, InventoryUI ui)
    {
        currentItem = item;
        inventoryUI = ui;

        ItemData itemData = item.itemData;
        nameText.text = itemData.itemName;
        descriptionText.text = itemData.description;
        
        int sellPrice = CalculateSellPrice(currentItem);
        sellPriceText.text = $"Bán ({sellPrice} vàng)";

        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(SellItem);

        int cost = itemData.baseUpgradeCost * currentItem.upgradeLevel;
        upgradeCostText.text = $"Nâng cấp ({cost} vàng)";

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(UpgradeItem);
        
        string stats = "";
        if (itemData.attackPower != 0)
            stats += $"Dame: {itemData.attackPower + Mathf.RoundToInt(itemData.attackPower * 0.1f * (item.upgradeLevel - 1))}\n";
        if (itemData.defense != 0)
            stats += $"Giáp: {itemData.defense + Mathf.RoundToInt(itemData.defense * 0.1f * (item.upgradeLevel - 1))}\n";
        if (itemData.healthBonus != 0)
            stats += $"Máu: {itemData.healthBonus + Mathf.RoundToInt(itemData.healthBonus * 0.1f * (item.upgradeLevel - 1))}\n";
        if (itemData.manaBonus != 0)
            stats += $"Mana: {itemData.manaBonus + Mathf.RoundToInt(itemData.manaBonus * 0.1f * (item.upgradeLevel - 1))}\n";
        if (itemData.critChance != 0)
            stats += $"Crit: {itemData.critChance + 1f * (item.upgradeLevel - 1)}%\n";
        if (itemData.moveSpeed != 0)
            stats += $"Speed: {itemData.moveSpeed + 0.05f * (item.upgradeLevel - 1)}\n";
        if (itemData.attackSpeed != 0)
            stats += $"Speed Attack: {(itemData.attackSpeed + 0.05f * (item.upgradeLevel - 1)):F1}\n";
        if (itemData.lifeSteal != 0)
            stats += $"Life Steal: {itemData.lifeSteal + 0.5f * (item.upgradeLevel - 1)}%\n";
        
        nameText.text = $"{itemData.itemName} +{item.upgradeLevel}";
        statText.text = stats.TrimEnd();

        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(EquipItem);

        gameObject.SetActive(true);
    }

    private void UpgradeItem()
    {
        int upgradeCost = currentItem.itemData.baseUpgradeCost * currentItem.upgradeLevel;

        if (CurrencyManager.Instance.SpendGold(upgradeCost))
        {
            currentItem.upgradeLevel++;
            Debug.Log($"Nâng cấp thành công! Cấp độ mới: {currentItem.upgradeLevel}");
            ShowDetails(currentItem, inventoryUI); // Làm mới UI
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
            inventoryUI.equipmentUi.EquipItem(currentItem); // Truyền ItemInstance

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