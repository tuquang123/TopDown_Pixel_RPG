using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class ShopDetailPopup : MonoBehaviour
{
    [Header("UI")]
    public ItemIconHandler icon;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text tierText;
    public TMP_Text weaponCategoryText;
    public TMP_Text priceText;

    public Button buyButton;
    public Button cancelButton;

    public Image tierBackground;
    public StatDisplayComponent statDisplayComponent;

    [Header("Animation")]
    public float fadeDuration = 0.2f;
    public float scaleDuration = 0.25f;

    private CanvasGroup canvasGroup;
    private Tween fadeTween;
    private Tween scaleTween;

    private ItemInstance currentItem;
    private ShopUI shopUI;

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
        UnregisterGoldEvent();
    }

    // ================= SHOW / HIDE =================

    public void Setup(ShopUI shop)
    {
        shopUI = shop;
        gameObject.SetActive(false);
    }

    public void Show(ItemInstance item)
    {
        if (item == null || item.itemData == null) return;

        currentItem = item;
        RefreshUI();

        gameObject.SetActive(true);
        RegisterGoldEvent();
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
                currentItem = null; // 🔥 FIX CỐT LÕI
                UnregisterGoldEvent(); // 🔥 chủ động tháo event
                gameObject.SetActive(false);
            });
    }


    // ================= UI =================

    private void RefreshUI()
    {
        ItemData data = currentItem.itemData;

        // Icon + Name
        icon.SetupIcons(currentItem);
        nameText.text = data.itemName;

        // Tier
        tierBackground.sprite =
            CommonReferent.Instance.itemTierColorConfig.GetBackground(data.tier);
        tierBackground.color = Color.white;

        tierText.text = data.tier.ToString();
        tierText.color = ItemUtility.GetColorByTier(data.tier);

        // Description
        descriptionText.text = data.description;

        // Weapon Category
        SetupWeaponCategory(data);

        // Stats
        statDisplayComponent.SetStats(currentItem);

        // Price
        priceText.text = $"{data.price} <sprite name=\"gold_icon\">";

        SetupButtons();
        UpdateBuyButtonState();
    }

    private void SetupWeaponCategory(ItemData data)
    {
        if (data.itemType != ItemType.Weapon)
        {
            weaponCategoryText.gameObject.SetActive(false);
            return;
        }

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

    // ================= BUTTON =================

    private void SetupButtons()
    {
        buyButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        buyButton.onClick.AddListener(OnClickBuy);
        cancelButton.onClick.AddListener(Hide);
    }

    private void OnClickBuy()
    {
        int price = currentItem.itemData.price;

        if (CurrencyManager.Instance.Gold < price)
        {
            GameEvents.OnShowToast.Raise("Không đủ vàng");
            return;
        }

        ForceHideImmediate();
        shopUI.BuyItem(currentItem);
        Hide();
    }
    
    private void ForceHideImmediate()
    {
        fadeTween?.Kill(true);
        scaleTween?.Kill(true);

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        gameObject.SetActive(false);
    }


    // ================= GOLD EVENT =================

    private void RegisterGoldEvent()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged += OnGoldChanged;
    }

    private void UnregisterGoldEvent()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged -= OnGoldChanged;
    }

    private void OnGoldChanged(int gold)
    {
        if (!canvasGroup.interactable) return;
        if (currentItem == null) return;
        UpdateBuyButtonState();
    }
    
    private void UpdateBuyButtonState()
    {
        bool enough = CurrencyManager.Instance.Gold >= currentItem.itemData.price;
        buyButton.interactable = enough;

        var colors = buyButton.colors;
        colors.normalColor = enough ? Color.white : new Color(1f, 1f, 1f, 0.5f);
        buyButton.colors = colors;
    }
}
