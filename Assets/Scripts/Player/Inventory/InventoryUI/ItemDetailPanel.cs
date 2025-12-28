using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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
    public ItemIconHandler icon;
    public Image tier;
    public StatDisplayComponent statDisplayComponent;
    public TMP_Text weaponCategoryText;
    

    [SerializeField] private ConfirmPopup confirmPopupPrefab;
    private ConfirmPopup currentPopup;

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

        // 🔥 Toast thông báo bán thành công
        GameEvents.OnShowToast.Raise(
            $"Sold {currentItem.itemData.itemName} +{currentItem.upgradeLevel}, earned {goldEarned} gold!"
        );


        gameObject.SetActive(false);
    }


  public void ShowDetails(ItemInstance item, InventoryUI ui)
{
    statDisplayComponent.SetStats(item);

    currentItem = item;
    inventoryUI = ui;
    ItemData itemData = item.itemData;

    bool isEquipped = inventoryUI.equipmentUi.IsItemEquipped(item);

    // ===== General Info =====
    nameText.text = item.upgradeLevel >= 1
        ? $"{itemData.itemName} +{item.upgradeLevel}"
        : itemData.itemName;

    icon.SetupIcons(item);

    tier.color = ItemUtility.GetColorByTier(itemData.tier);
    tierText.text = itemData.tier.ToString();
    tierText.color = ItemUtility.GetColorByTier(itemData.tier);

    descriptionText.text = itemData.description;

    // ===== Weapon Category (CẬN / XA / NẶNG) =====
    if (itemData.itemType == ItemType.Weapon)
    {
        weaponCategoryText.gameObject.SetActive(true);

        switch (itemData.weaponCategory)
        {
            case WeaponCategory.Melee:
                weaponCategoryText.text = "Cận chiến";
                weaponCategoryText.color = new Color(0.85f, 0.85f, 0.85f);
                break;

            case WeaponCategory.Ranged:
                weaponCategoryText.text = "Đánh xa";
                weaponCategoryText.color = new Color(0.6f, 0.8f, 1f);
                break;

            case WeaponCategory.HeavyMelee:
                weaponCategoryText.text = "Cận nặng";
                weaponCategoryText.color = new Color(1f, 0.7f, 0.4f);
                break;
        }
    }
    else
    {
        weaponCategoryText.gameObject.SetActive(false);
    }

    // ===== Sell Button =====
    int sellPrice = CalculateSellPrice(currentItem);
    sellPriceText.text = $"Bán ({sellPrice} <sprite name=\"gold_icon\">)";
    sellButton.onClick.RemoveAllListeners();
    sellButton.onClick.AddListener(ShowSellConfirm);

    sellButton.gameObject.SetActive(!isEquipped);

    // ===== Upgrade Button =====
    int upgradeCost = itemData.baseUpgradeCost * (currentItem.upgradeLevel + 1);
    upgradeCostText.text = $"Nâng cấp ({upgradeCost} <sprite name=\"gold_icon\">)";
    upgradeButton.onClick.RemoveAllListeners();
    upgradeButton.onClick.AddListener(ShowUpgradeConfirm);

    upgradeButton.gameObject.SetActive(!isEquipped);

    // ===== Equip / Use Button =====
    equipButton.onClick.RemoveAllListeners();

    string statsText = "";

    if (itemData.itemType == ItemType.Consumable)
    {
        // ===== Consumable =====
        equipButton.GetComponentInChildren<TMP_Text>().text = "Dùng";
        equipButton.onClick.AddListener(ConsumeItem);
        upgradeButton.gameObject.SetActive(false);

        if (itemData.restoresHealth)
        {
            string value = itemData.percentageBased
                ? $"{itemData.healthRestoreAmount}%"
                : $"{itemData.healthRestoreAmount}";
            statsText += $"Hồi máu: {value}\n";
        }

        if (itemData.restoresMana)
        {
            string value = itemData.percentageBased
                ? $"{itemData.manaRestoreAmount}%"
                : $"{itemData.manaRestoreAmount}";
            statsText += $"Hồi mana: {value}\n";
        }
    }
    else
    {
        // ===== Equipment =====
        equipButton.GetComponentInChildren<TMP_Text>().text = "Trang bị";
        equipButton.onClick.AddListener(EquipItem);

        void AddStatLine(string label, ItemStatBonus bonus, float upgradePercent = 0.1f)
        {
            if (bonus == null || !bonus.HasValue) return;

            float flat = bonus.flat + (bonus.flat * upgradePercent * (item.upgradeLevel - 1));
            float percent = bonus.percent;

            bool showDecimal = label == "Tốc đánh";

            if (flat != 0)
                statsText += showDecimal
                    ? $"{label}: {flat:F1}\n"
                    : $"{label}: {Mathf.RoundToInt(flat)}\n";

            if (percent != 0)
                statsText += $"{label}: +{percent}%\n";
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
            GameEvents.OnShowToast.Raise($"Succes Upgrade Item! {currentItem.itemData.itemName} - {currentItem.upgradeLevel}" );
        }
        else
        {
            Debug.Log("Không đủ vàng để nâng cấp.");
            GameEvents.OnShowToast.Raise("Không đủ vàng để nâng cấp.");
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
    private void ShowUpgradeConfirm()
    {
        int nextLevel = currentItem.upgradeLevel + 1;
        int cost = currentItem.itemData.baseUpgradeCost * nextLevel;

        string title = "Nâng cấp trang bị";

        string message =
            $"{currentItem.itemData.itemName} " +
            $"+{currentItem.upgradeLevel} → +{nextLevel}\n\n" +
            BuildUpgradeStatPreview() +
            $"\nGiá: {cost} vàng";

        ShowConfirm(title, message, UpgradeItem);
    }


    private string BuildUpgradeStatPreview()
    {
        string text = "";

        void Add(string name, ItemStatBonus stat, float scale = 0.1f)
        {
            if (stat == null || !stat.HasValue) return;

            float current = stat.flat + stat.flat * scale * (currentItem.upgradeLevel - 1);
            float next = stat.flat + stat.flat * scale * currentItem.upgradeLevel;
            float diff = next - current;

            string curStr = current % 1 == 0 ? current.ToString("0") : current.ToString("0.0");
            string nextStr = next % 1 == 0 ? next.ToString("0") : next.ToString("0.0");
            string diffStr = diff % 1 == 0 ? diff.ToString("0") : diff.ToString("0.0");

            if (diff > 0)
                text += $"{name}: {curStr} → {nextStr} (+{diffStr})\n";
        }

        var data = currentItem.itemData;
        Add("Dame", data.attack);
        Add("Giáp", data.defense);
        Add("Máu", data.health);
        Add("Crit", data.critChance, 0.05f);
        Add("Tốc đánh", data.attackSpeed, 0.05f);

        return text;
    }


    private void ShowConfirm(string title, string message, Action onConfirm)
    {
        if (currentPopup != null)
            return;

        Canvas canvas = GetComponentInParent<Canvas>();
        currentPopup = Instantiate(confirmPopupPrefab, canvas.transform);

        currentPopup.OnClosed = () =>
        {
            currentPopup = null;
        };

        currentPopup.Show(title, message, onConfirm);
    }



   

    private void ShowSellConfirm()
    {
        int sellPrice = CalculateSellPrice(currentItem);

        string title = "Xác nhận bán";

        string message =
            $"{currentItem.itemData.itemName} +{currentItem.upgradeLevel}\n\n" +
            $"Giá: {sellPrice} vàng";

        ShowConfirm(title, message, SellItem);
    }



    public void Hide()
    {
        gameObject.SetActive(false);
    }
}