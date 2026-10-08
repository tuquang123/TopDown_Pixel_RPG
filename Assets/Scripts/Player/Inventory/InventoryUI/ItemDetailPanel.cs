using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class ItemDetailPanel : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    //public TMP_Text statText;
    public TMP_Text tierText;
    public TMP_Text weaponCategoryText;
    public TMP_Text upgradeCostText;
    public TMP_Text sellPriceText;

    public Button equipButton;
    public Button upgradeButton;
    public Button sellButton;

    public ItemIconHandler icon;
    public Image tierBackground;
    public StatDisplayComponent statDisplayComponent;

    [Header("Animation (BasePopup Style)")]
    public float fadeDuration = 0.2f;
    public float scaleDuration = 0.25f;

    [Header("Confirm Popup")]
    [SerializeField] private ConfirmPopup confirmPopupPrefab;

    private CanvasGroup canvasGroup;
    private Tween fadeTween;
    private Tween scaleTween;

    private ConfirmPopup currentPopup;
    private ItemInstance currentItem;
    private InventoryUI inventoryUI;
    public Button lockButton;

    [Header("Lock Icons")]
    public Image lockIconLocked;
    public Image lockIconUnlocked;
    // ================= UNITY =================

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += RefreshLanguage;
        }
    }

    private void OnDisable()
    {
        fadeTween?.Kill();
        scaleTween?.Kill();

        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= RefreshLanguage;
        }
    }

    private void RefreshLanguage()
    {
        if (currentItem != null && inventoryUI != null)
        {
            RefreshUI();
        }
    }

    // ================= SHOW / HIDE =================

    public void Show(ItemInstance item, InventoryUI ui)
    {
        currentItem = item;
        inventoryUI = ui;

        RefreshUI();

        gameObject.SetActive(true);
        PlayShowAnimation();
    }

    public void Hide()
    {
        PlayHideAnimation();
    }

    private void PlayShowAnimation()
    {
        fadeTween?.Kill();
        scaleTween?.Kill();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        transform.localScale = Vector3.zero;

        fadeTween = canvasGroup
            .DOFade(1f, fadeDuration)
            .SetUpdate(true);

        scaleTween = transform
            .DOScale(Vector3.one, scaleDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            });
    }

    private void PlayHideAnimation()
    {
        fadeTween?.Kill();
        scaleTween?.Kill();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        fadeTween = canvasGroup
            .DOFade(0f, fadeDuration * 0.75f)
            .SetUpdate(true);

        scaleTween = transform
            .DOScale(Vector3.zero, scaleDuration * 0.8f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }

    // ================= UI REFRESH =================

    private void RefreshUI()
    {
        ItemData data = currentItem.itemData;
        bool isEquipped = inventoryUI.equipmentUi.IsItemEquipped(currentItem);
        ItemInstance equippedItem =
            inventoryUI.equipmentUi.GetEquippedItem(currentItem.itemData.itemType);

        // Icon + Name
        icon.SetupIcons(currentItem);
        nameText.text = currentItem.upgradeLevel - 1 > 0
            ? $"{data.itemName} +{currentItem.upgradeLevel -1}"
            : data.itemName;

        // Tier
        tierBackground.sprite =
            CommonReferent.Instance.itemTierColorConfig.GetBackground(data.tier);
        tierBackground.color = Color.white;

        tierText.text = ItemUtility.GetLocalizedTier(data.tier);
        tierText.color = ItemUtility.GetColorByTier(data.tier);

        // Description
        descriptionText.text = data.description;

        // Weapon Category
        if (data.itemType == ItemType.Weapon)
        {
            weaponCategoryText.gameObject.SetActive(true);
            switch (data.weaponCategory)
            {
                case WeaponCategory.Melee:
                    weaponCategoryText.text = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("melee") : "Melee";
                    weaponCategoryText.color = Color.white;
                    break;
                case WeaponCategory.Ranged:
                    weaponCategoryText.text = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("ranged") : "Ranged";
                    weaponCategoryText.color = new Color(0.6f, 0.8f, 1f);
                    break;
                case WeaponCategory.HeavyMelee:
                    weaponCategoryText.text = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("heavy_melee") : "Heavy Melee";
                    weaponCategoryText.color = new Color(1f, 0.7f, 0.4f);
                    break;
            }
        }
        else
        {
            weaponCategoryText.gameObject.SetActive(false);
        }

        // Stats
        var equipped =
            inventoryUI.equipmentUi.GetEquippedItem(currentItem.itemData.itemType);

        if (isEquipped)
        {
            // show the stat that will be lost if unequipped
            statDisplayComponent.SetUnequipStats(currentItem);
        }
        else
        {
            statDisplayComponent.SetCompareStats(currentItem, equipped);
        }

        // Buttons
        SetupButtons(currentItem, data, isEquipped);
        RefreshLockVisual();
    }

    // ================= BUTTONS =================

    private void SetupButtons(ItemInstance item, ItemData data, bool isEquipped)
    {
        equipButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.RemoveAllListeners();
        sellButton.onClick.RemoveAllListeners();

        string equipText = LanguageManager.Instance != null
            ? LanguageManager.Instance.GetTranslation(data.itemType == ItemType.Consumable ? "use" : "equip")
            : "Equip";

        if (data.itemType == ItemType.Consumable)
        {
            equipButton.GetComponentInChildren<TMP_Text>().text = equipText;
            equipButton.onClick.AddListener(ConsumeItem);

            upgradeButton.gameObject.SetActive(false);
            sellButton.gameObject.SetActive(true);
        }
        else
        {
            equipButton.GetComponentInChildren<TMP_Text>().text = equipText;
            equipButton.onClick.AddListener(EquipItem);

            // Upgrade và Sell luôn hiện, kể cả khi item đang được mặc
            upgradeButton.gameObject.SetActive(true);
            sellButton.gameObject.SetActive(true);

            int upgradeCost = CalculateUpgradeCost(item);
            string upgradeWord = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("upgrade") : "Upgrade";
            upgradeCostText.text = $"{upgradeWord} ({upgradeCost} <sprite name=\"gold_icon\">)";
            upgradeButton.onClick.AddListener(ShowUpgradeConfirm);
        }

        int sellPrice = CalculateSellPrice(item);
        string sellWord = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("sell") : "Sell";
        sellPriceText.text = $"{sellWord} ({sellPrice} <sprite name=\"gold_icon\">)";
        sellButton.onClick.AddListener(ShowSellConfirm);
        lockButton.onClick.RemoveAllListeners();
        lockButton.onClick.AddListener(ToggleLock);
    }

    private void RefreshLockVisual()
    {
        lockIconLocked.gameObject.SetActive(currentItem.isLocked);
        lockIconUnlocked.gameObject.SetActive(!currentItem.isLocked);
    }

    // ================= ACTIONS =================

    public void EquipItem()
    {
        if (currentItem == null) return;

        float beforePower = PlayerStats.Instance.CurrentPower;

        inventoryUI.equipmentUi.EquipItem(currentItem);
        QuestManager.Instance?.ReportProgressByObjectiveName("EquipItem", 1);

        PlayerStats.Instance.CalculatePower();

        float afterPower = PlayerStats.Instance.CurrentPower;
        ShowPowerDiff(beforePower, afterPower);

        inventoryUI.equipmentUi.UpdateEquipmentUI();
        inventoryUI.UpdateInventoryUI();
    }

    private void ShowPowerDiff(float before, float after)
    {
        float diff = after - before;
        if (Mathf.Approximately(diff, 0)) return;

        string text;

        if (diff > 0)
            text = $"<color=#00FF00>+{diff:N0} Power</color>";
        else
            text = $"<color=#FF4D4D>{diff:N0} Power</color>";

        // FIX: toast Power hiện nửa thời gian so với các toast khác
        if (ToastUI.Instance != null)
            ToastUI.Instance.ShowToast(text, ToastUI.Instance.DefaultShowDuration * 0.5f);
        else
            GameEvents.OnShowToast.Raise(text); // fallback nếu ToastUI chưa sẵn sàng
    }

    private void ConsumeItem()
    {
        PlayerStats.Instance?.Consume(currentItem.itemData);
        inventoryUI.Inventory.RemoveItem(currentItem);
        QuestManager.Instance.ReportProgressByItemUse(currentItem.itemData.itemID);
        inventoryUI.UpdateInventoryUI();
        Hide();
    }

    private void UpgradeItem()
    {
        int cost = CalculateUpgradeCost(currentItem);

        if (!CurrencyManager.Instance.SpendGold(cost))
        {
            GameEvents.OnShowToast.Raise(LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("not_enough_gold") : "Not enough Gold");
            return;
        }

        currentItem.upgradeLevel++;
        QuestManager.Instance?.ReportItemUpgrade(currentItem.upgradeLevel);
        GameEvents.OnShowToast.Raise(LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("upgrade_successful") : "Upgrade successful!");
        RefreshUI();
    }

    private void SellItem()
    {
        int gold = CalculateSellPrice(currentItem);

        if (inventoryUI.equipmentUi.IsItemEquipped(currentItem))
        {
            inventoryUI.equipmentUi.UnequipItem(currentItem.itemData.itemType);
        }

        CurrencyManager.Instance.AddGold(gold);

        inventoryUI.Inventory.RemoveItem(currentItem);
        inventoryUI.UpdateInventoryUI();

        string soldText = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("sold_successfully") : "Sold successfully";
        GameEvents.OnShowToast.Raise($"{soldText}: {currentItem.itemData.itemName}");
        Hide();
    }

    // ================= CONFIRM =================

    private void ShowUpgradeConfirm()
    {
        int next = currentItem.upgradeLevel + 1;
        int cost = CalculateUpgradeCost(currentItem);

        string statText = BuildUpgradeStatText(currentItem);
        string upgradeText = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("upgrade") : "Upgrade";
        string priceText = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("price") : "Price";
        string goldText = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("gold") : "Gold";

        UIManager.Instance.ShowPopupByType(PopupType.ItemConfirm);

        if (UIManager.Instance.TryGetPopup(PopupType.ItemConfirm, out var popup)
            && popup is ConfirmPopup confirm)
        {
            confirm.Show(
                upgradeText,
                $"{currentItem.itemData.itemName} +{currentItem.upgradeLevel - 1} -> +{next - 1}\n" +
                statText +
                $"\n\n{priceText}: {cost} {goldText}",
                UpgradeItem
            );
        }
    }

    private string BuildUpgradeStatText(ItemInstance item)
    {
        var d = item.itemData;
        int curLv = item.upgradeLevel;
        int nextLv = curLv + 1;

        string text = "";

        AppendStat(ref text, "Attack",     d.attack,      curLv, nextLv);
        AppendStat(ref text, "Defense",    d.defense,     curLv, nextLv);
        AppendStat(ref text, "Speed",      d.speed,       curLv, nextLv);
        AppendStat(ref text, "Crit",       d.critChance,  curLv, nextLv, true);
        AppendStat(ref text, "LifeSteal",  d.lifeSteal,   curLv, nextLv, true);
        AppendStat(ref text, "Atk Speed",  d.attackSpeed, curLv, nextLv);
        AppendStat(ref text, "HP",         d.health,      curLv, nextLv);
        AppendStat(ref text, "Mana",       d.mana,        curLv, nextLv);

        return text;
    }

    private void AppendStat(
        ref string text,
        string label,
        ItemStatBonus bonus,
        int curLv,
        int nextLv,
        bool isPercent = false
    )
    {
        if (bonus == null || !bonus.HasValue) return;

        float cur = 0;
        float next = 0;

        if (Mathf.Abs(bonus.flat) > 0.01f)
        {
            cur = Equipment.ItemStatCalculator.GetUpgradedValue(bonus.flat, curLv);
            next = Equipment.ItemStatCalculator.GetUpgradedValue(bonus.flat, nextLv);
        }
        else if (Mathf.Abs(bonus.percent) > 0.01f)
        {
            cur = Equipment.ItemStatCalculator.GetUpgradedValue(bonus.percent, curLv);
            next = Equipment.ItemStatCalculator.GetUpgradedValue(bonus.percent, nextLv);
        }

        if (Mathf.Approximately(cur, next)) return;

        float add = next - cur;

        string suffix = isPercent ? "%" : "";

        string translatedLabel = TranslateStatLabel(label);
        text +=
            $"\n{translatedLabel}: {Format(cur)}{suffix} -> {Format(next)}{suffix} " +
            $"<color=#00FF00>(+{Format(add)}{suffix})</color>";
    }

    private string Format(float value)
    {
        return value % 1 == 0
            ? value.ToString("0")
            : value.ToString("0.0");
    }

    private string TranslateStatLabel(string label)
    {
        if (LanguageManager.Instance == null) return label;

        return label switch
        {
            "Attack" => LanguageManager.Instance.GetTranslation("attack"),
            "Defense" => LanguageManager.Instance.GetTranslation("defense"),
            "Speed" => LanguageManager.Instance.GetTranslation("speed"),
            "Crit" => LanguageManager.Instance.GetTranslation("crit"),
            "LifeSteal" => LanguageManager.Instance.GetTranslation("life_steal"),
            "Atk Speed" => LanguageManager.Instance.GetTranslation("attack_speed"),
            "HP" => LanguageManager.Instance.GetTranslation("hp"),
            "Mana" => LanguageManager.Instance.GetTranslation("mana"),
            _ => label
        };
    }

    private void ShowSellConfirm()
    {
        int price = CalculateSellPrice(currentItem);
        string confirmText = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("confirm") : "Confirm";
        string priceText = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("price") : "Price";
        string goldText = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("gold") : "Gold";

        UIManager.Instance.ShowPopupByType(PopupType.ItemConfirm);

        if (UIManager.Instance.TryGetPopup(PopupType.ItemConfirm, out var popup)
            && popup is ConfirmPopup confirm)
        {
            confirm.Show(
                confirmText,
                $"{currentItem.itemData.itemName} +{currentItem.upgradeLevel}\n{priceText}: {price} {goldText}",
                SellItem
            );
        }
    }

    private void ShowConfirm(string title, string message, Action onConfirm)
    {
        if (currentPopup != null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        currentPopup = Instantiate(confirmPopupPrefab, canvas.transform);
        currentPopup.OnClosed = () => currentPopup = null;
        currentPopup.Show(title, message, onConfirm);
    }

    // ================= HELPERS =================

    private int CalculateSellPrice(ItemInstance item)
    {
        int baseValue = item.itemData.baseUpgradeCost;
        float multi = 0.6f + item.upgradeLevel * 0.2f;
        return Mathf.RoundToInt(baseValue * multi);
    }

    private int CalculateUpgradeCost(ItemInstance item)
    {
        int baseValue = Mathf.Max(1, item.itemData.baseUpgradeCost);
        int nextLevel = Mathf.Max(2, item.upgradeLevel + 1);
        float step = nextLevel - 1;
        return Mathf.Max(1, Mathf.RoundToInt(baseValue * (1f + Mathf.Pow(step, 1.08f) * 0.38f)));
    }

    private void ToggleLock()
    {
        currentItem.isLocked = !currentItem.isLocked;

        string key = currentItem.isLocked ? "item_locked" : "item_unlocked";
        string fallback = currentItem.isLocked ? "Item locked" : "Item unlocked";
        GameEvents.OnShowToast.Raise(
            LanguageManager.Instance != null
                ? LanguageManager.Instance.GetTranslation(key)
                : fallback);

        RefreshLockVisual();
        inventoryUI.RefreshCurrentSelectedItemLock();
    }
}