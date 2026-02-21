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

    private void OnDisable()
    {
        fadeTween?.Kill();
        scaleTween?.Kill();
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

        tierText.text = data.tier.ToString();
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
                    weaponCategoryText.text = "Cận chiến";
                    weaponCategoryText.color = Color.white;
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

        // Stats
        var equipped =
            inventoryUI.equipmentUi.GetEquippedItem(currentItem.itemData.itemType);

        if (equipped != null)
            statDisplayComponent.SetCompareStats(currentItem, equipped);
        else
            statDisplayComponent.SetStats(currentItem);

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

        if (data.itemType == ItemType.Consumable)
        {
            equipButton.GetComponentInChildren<TMP_Text>().text = "Dùng";
            equipButton.onClick.AddListener(ConsumeItem);

            upgradeButton.gameObject.SetActive(false);
            sellButton.gameObject.SetActive(true);
        }
        else
        {
            equipButton.GetComponentInChildren<TMP_Text>().text = "Trang bị";
            equipButton.onClick.AddListener(EquipItem);

            upgradeButton.gameObject.SetActive(!isEquipped);
            sellButton.gameObject.SetActive(!isEquipped);

            int upgradeCost = data.baseUpgradeCost * (item.upgradeLevel + 1);
            upgradeCostText.text = $"Nâng cấp ({upgradeCost} <sprite name=\"gold_icon\">)";
            upgradeButton.onClick.AddListener(ShowUpgradeConfirm);
        }

        int sellPrice = CalculateSellPrice(item);
        sellPriceText.text = $"Bán ({sellPrice} <sprite name=\"gold_icon\">)";
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

    private void EquipItem()
    {
        inventoryUI.equipmentUi.EquipItem(currentItem);
        inventoryUI.Inventory.RemoveItem(currentItem);
        inventoryUI.UpdateInventoryUI();
        Hide();
    }

    private void ConsumeItem()
    {
        PlayerStats.Instance?.Consume(currentItem.itemData);
        inventoryUI.Inventory.RemoveItem(currentItem);
        inventoryUI.UpdateInventoryUI();
        Hide();
    }

    private void UpgradeItem()
    {
        int cost = currentItem.itemData.baseUpgradeCost * (currentItem.upgradeLevel + 1);

        if (!CurrencyManager.Instance.SpendGold(cost))
        {
            GameEvents.OnShowToast.Raise("Không đủ vàng");
            return;
        }

        currentItem.upgradeLevel++;
        GameEvents.OnShowToast.Raise("Nâng cấp thành công!");
        RefreshUI();
    }

    private void SellItem()
    {
        int gold = CalculateSellPrice(currentItem);
        CurrencyManager.Instance.AddGold(gold);

        inventoryUI.Inventory.RemoveItem(currentItem);
        inventoryUI.UpdateInventoryUI();

        GameEvents.OnShowToast.Raise($"Đã bán {currentItem.itemData.itemName}");
        Hide();
    }

    // ================= CONFIRM =================

    private void ShowUpgradeConfirm()
    {
        int next = currentItem.upgradeLevel + 1;
        int cost = currentItem.itemData.baseUpgradeCost * next;

        string statText = BuildUpgradeStatText(currentItem);

        UIManager.Instance.ShowPopupByType(PopupType.ItemConfirm);

        if (UIManager.Instance.TryGetPopup(PopupType.ItemConfirm, out var popup)
            && popup is ConfirmPopup confirm)
        {
            confirm.Show(
                "Nâng cấp",
                $"{currentItem.itemData.itemName} +{currentItem.upgradeLevel - 1} → +{next - 1}\n" +
                statText +
                $"\n\nGiá: {cost} vàng",
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

        text +=
            $"\n{label}: {Format(cur)}{suffix} → {Format(next)}{suffix} " +
            $"<color=#00FF00>(+{Format(add)}{suffix})</color>";
    }
    
    private string Format(float value)
    {
        return value % 1 == 0
            ? value.ToString("0")
            : value.ToString("0.0");
    }


    private void ShowSellConfirm()
    {
        int price = CalculateSellPrice(currentItem);

        UIManager.Instance.ShowPopupByType(PopupType.ItemConfirm);

        if (UIManager.Instance.TryGetPopup(PopupType.ItemConfirm, out var popup)
            && popup is ConfirmPopup confirm)
        {
            confirm.Show(
                "Xác nhận bán",
                $"{currentItem.itemData.itemName} +{currentItem.upgradeLevel}\nGiá: {price} vàng",
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
    private void ToggleLock()
    {
        currentItem.isLocked = !currentItem.isLocked;

        GameEvents.OnShowToast.Raise(currentItem.isLocked ? "Đã khóa vật phẩm" : "Đã mở khóa vật phẩm");

        RefreshLockVisual();
        inventoryUI.RefreshCurrentSelectedItemLock();
    }
  
}