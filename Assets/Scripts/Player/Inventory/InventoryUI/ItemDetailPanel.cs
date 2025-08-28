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
    public Image icon;

    private ItemInstance currentItem;
    private InventoryUI inventoryUI;

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
        bool isEquipped = inventoryUI.equipmentUi.IsItemEquipped(item);

        ItemData itemData = item.itemData;
        nameText.text = itemData.itemName;
        icon.sprite = itemData.icon;
        descriptionText.text = itemData.description;

        int sellPrice = CalculateSellPrice(currentItem);
        sellPriceText.text = $"Bán ({sellPrice} vàng)";

        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(SellItem);

        int cost = itemData.baseUpgradeCost * currentItem.upgradeLevel;
        upgradeCostText.text = $"Nâng cấp ({cost} vàng)";

        sellButton.gameObject.SetActive(!isEquipped);
        upgradeButton.gameObject.SetActive(!isEquipped);

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(UpgradeItem);

        // Stats hiển thị (chỉ cho trang bị, consumable thì ko cần)
        string stats = "";
        void AddStatLine(string label, ItemStatBonus bonus, float upgradePercent = 0.1f, string suffix = "")
        {
            if (bonus == null || (!bonus.HasValue)) return;

            float flat = bonus.flat + (bonus.flat * upgradePercent * (item.upgradeLevel - 1));
            float percent = bonus.percent;

            if (flat != 0)
                stats += $"{label}: {Mathf.RoundToInt(flat)}{suffix}\n";
            if (percent != 0)
                stats += $"{label}: +{percent}%{suffix}\n";
        }

        // Nếu là trang bị
        if (itemData.itemType != ItemType.Consumable)
        {
            AddStatLine("Dame", itemData.attack);
            AddStatLine("Giáp", itemData.defense);
            AddStatLine("Máu", itemData.health);
            AddStatLine("Mana", itemData.mana);
            AddStatLine("Crit", itemData.critChance, 0.05f);     
            AddStatLine("Speed", itemData.speed, 0.05f);
            AddStatLine("Tốc đánh", itemData.attackSpeed, 0.05f);
            AddStatLine("Hút máu", itemData.lifeSteal, 0.05f);
        }

        nameText.text = $"{itemData.itemName} +{item.upgradeLevel}";
        statText.text = stats.TrimEnd();

        equipButton.onClick.RemoveAllListeners();

        // Nếu là consumable thì đổi nút -> "Dùng"
        if (itemData.itemType == ItemType.Consumable)
        {
            equipButton.GetComponentInChildren<TMP_Text>().text = "Dùng";
            equipButton.onClick.AddListener(ConsumeItem);

            // Consumable thì không upgrade được
            upgradeButton.gameObject.SetActive(false);
        }
        else
        {
            equipButton.GetComponentInChildren<TMP_Text>().text = "Trang bị";
            equipButton.onClick.AddListener(EquipItem);
        }

        gameObject.SetActive(true);
    }

    private void ConsumeItem()
    {
        if (currentItem == null) return;

        var playerStats = PlayerStats.Instance;
        if (playerStats != null)
        {
            playerStats.Consume(currentItem.itemData);
        }

        if (inventoryUI.inventory.RemoveItem(currentItem))
        {
            inventoryUI.UpdateInventoryUI();
        }

        Debug.Log($"Đã dùng {currentItem.itemData.itemName}");
        gameObject.SetActive(false);
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
            inventoryUI.equipmentUi.EquipItem(currentItem); 

            if (inventoryUI.inventory.RemoveItem(currentItem))
            {
                inventoryUI.UpdateInventoryUI();
            }

            gameObject.SetActive(false); 
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
