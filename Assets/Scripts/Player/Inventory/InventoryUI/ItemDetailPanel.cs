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
    public TMP_Text tierText;
    public Image icon;
    public Image tier;

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
        ItemData itemData = item.itemData;

        bool isEquipped = inventoryUI.equipmentUi.IsItemEquipped(item);

        // ==== General UI ====
        if (item.upgradeLevel >= 1)
            nameText.text = $"{itemData.itemName} +{item.upgradeLevel}";
        else
            nameText.text = itemData.itemName;

        icon.sprite = itemData.icon;
        tier.color =  ItemUtility.GetColorByTier(item.itemData.tier);
        tierText.text = $"{item.itemData.tier}";
        descriptionText.text = itemData.description;

        // ==== Sell Button ====
        int sellPrice = CalculateSellPrice(currentItem);
        sellPriceText.text = $"Bán ({sellPrice} vàng)";
        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(SellItem);
        sellButton.gameObject.SetActive(!isEquipped);

        // ==== Upgrade Button ====
        int upgradeCost = currentItem.itemData.baseUpgradeCost * (currentItem.upgradeLevel + 1);
        upgradeCostText.text = $"Nâng cấp ({upgradeCost} vàng)";
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(UpgradeItem);
        upgradeButton.gameObject.SetActive(!isEquipped);

        // ==== Equip/Use Button ====
        equipButton.onClick.RemoveAllListeners();

        // ==== Stats display ====
        string statsText = "";

        if (itemData.itemType == ItemType.Consumable)
        {
            // ---- Consumable ----
            equipButton.GetComponentInChildren<TMP_Text>().text = "Dùng";
            equipButton.onClick.AddListener(ConsumeItem);

            upgradeButton.gameObject.SetActive(false); // Consumable không nâng cấp được

            if (itemData.restoresHealth)
            {
                string hpValue = itemData.percentageBased
                    ? $"{itemData.healthRestoreAmount}%"
                    : $"{itemData.healthRestoreAmount}";
                statsText += $"Hồi máu: {hpValue}\n";
            }

            if (itemData.restoresMana)
            {
                string mpValue = itemData.percentageBased
                    ? $"{itemData.manaRestoreAmount}%"
                    : $"{itemData.manaRestoreAmount}";
                statsText += $"Hồi mana: {mpValue}\n";
            }
        }
        else
        {
            // ---- Equipment ----
            equipButton.GetComponentInChildren<TMP_Text>().text = "Trang bị";
            equipButton.onClick.AddListener(EquipItem);

            void AddStatLine(string label, ItemStatBonus bonus, float upgradePercent = 0.1f, string suffix = "")
            {
                if (bonus == null || !bonus.HasValue) return;

                float flat = bonus.flat + (bonus.flat * upgradePercent * (item.upgradeLevel - 1));
                float percent = bonus.percent;

                bool showDecimal = (label == "Tốc đánh" || label == "Tốc phép"); // chỉ 2 stat này giữ 1 số lẻ

                if (flat != 0)
                    statsText += showDecimal
                        ? $"{label}: {flat:F1}{suffix}\n"
                        : $"{label}: {Mathf.RoundToInt(flat)}{suffix}\n";

                if (percent != 0)
                    statsText += showDecimal
                        ? $"{label}: +{percent:F1}%{suffix}\n"
                        : $"{label}: +{percent}%{suffix}\n";
            }


            AddStatLine("Dame", itemData.attack);
            AddStatLine("Giáp", itemData.defense);
            AddStatLine("Máu", itemData.health);
            AddStatLine("Mana", itemData.mana);
            AddStatLine("Crit", itemData.critChance, 0.05f);
            AddStatLine("Speed", itemData.speed, 0.05f);
            AddStatLine("Tốc đánh", itemData.attackSpeed, 0.05f);
            AddStatLine("Hút máu", itemData.lifeSteal, 0.05f);
        }

        // Gán stats cuối cùng
        statText.text = statsText.TrimEnd();

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
        int upgradeCost = currentItem.itemData.baseUpgradeCost * currentItem.upgradeLevel + 1;

        if (CurrencyManager.Instance.SpendGold(upgradeCost))
        {
            currentItem.upgradeLevel++;
            Debug.Log($"Nâng cấp thành công! Cấp độ mới: {currentItem.upgradeLevel}");
            ShowDetails(currentItem, inventoryUI); 
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