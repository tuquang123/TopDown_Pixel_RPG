using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ShopDetailPopup : MonoBehaviour
{
    [Header("References")]
    public ItemIconHandler icon;
    public TMP_Text nameText;
    public TMP_Text tierText;
    public TMP_Text descriptionText;
    public TMP_Text priceText;
    public Button buyButton;
    public Button cancelButton;
    public StatDisplayComponent statDisplayComponent;

    [Header("Weapon Info")]
    public TMP_Text weaponRangeText;

    [Header("Tier Background")]
    public Image backgroundImage;

    [Header("Animation")]
    public float animDuration = 0.25f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private ShopUI shopUI;
    private ItemInstance currentItem;

    private CanvasGroup canvasGroup;
    private Coroutine animCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }

    public void Setup(ShopUI shop)
    {
        shopUI = shop;
        gameObject.SetActive(false);
    }

    // ================= SHOW =================
    public void ShowDetail(ItemInstance instance)
    {
        if (instance == null || instance.itemData == null) return;

        currentItem = instance;
        var data = instance.itemData;

        // ===== Icon + Name =====
        icon.SetupIcons(instance);
        nameText.text = data.itemName;

        // ===== Tier =====
        tierText.text = data.tier.ToString();
        tierText.color = ItemUtility.GetColorByTier(data.tier);
        ApplyTierBackgroundColor(data.tier);

        // ===== Description =====
        descriptionText.text = data.description;

        // ===== Weapon Category =====
        if (data.itemType == ItemType.Weapon)
        {
            weaponRangeText.gameObject.SetActive(true);
            switch (data.weaponCategory)
            {
                case WeaponCategory.Melee:
                    weaponRangeText.text = "Cận chiến";
                    weaponRangeText.color = new Color(0.85f, 0.85f, 0.85f);
                    break;
                case WeaponCategory.Ranged:
                    weaponRangeText.text = "Đánh xa";
                    weaponRangeText.color = new Color(0.6f, 0.8f, 1f);
                    break;
                case WeaponCategory.HeavyMelee:
                    weaponRangeText.text = "Cận nặng";
                    weaponRangeText.color = new Color(1f, 0.7f, 0.4f);
                    break;
                default:
                    weaponRangeText.text = "Không xác định";
                    weaponRangeText.color = Color.white;
                    break;
            }
        }
        else
        {
            weaponRangeText.gameObject.SetActive(false);
        }

        // ===== Stats =====
        statDisplayComponent.SetStats(instance);

        // ===== Price =====
        priceText.text = $"{data.price} <sprite name=\"gold_icon\">";

        buyButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        UpdateBuyButtonState();

        buyButton.onClick.AddListener(() =>
        {
            if (CurrencyManager.Instance.Gold >= data.price)
            {
                shopUI.BuyItem(currentItem);
                Hide();
            }
            else
            {
                GameEvents.OnShowToast.Raise("Gold not enough!");
            }
        });

        cancelButton.onClick.AddListener(Hide);

        CurrencyManager.Instance.OnGoldChanged += OnGoldChanged;

        PlayOpenAnimation();
    }

    // ================= ANIMATION =================
    private void PlayOpenAnimation()
    {
        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        gameObject.SetActive(true);
        animCoroutine = StartCoroutine(OpenAnim());
    }

    private IEnumerator OpenAnim()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        transform.localScale = Vector3.zero;

        float t = 0f;
        while (t < animDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / animDuration;

            float scale = scaleCurve.Evaluate(p);
            transform.localScale = Vector3.one * scale;
            canvasGroup.alpha = p;

            yield return null;
        }

        transform.localScale = Vector3.one;
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        if (!gameObject.activeSelf) return;

        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        animCoroutine = StartCoroutine(CloseAnim());
    }

    private IEnumerator CloseAnim()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float t = 0f;
        while (t < animDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / animDuration;

            transform.localScale = Vector3.one * (1 - p);
            canvasGroup.alpha = 1 - p;

            yield return null;
        }

        gameObject.SetActive(false);

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged -= OnGoldChanged;
    }

    // ================= HELPERS =================
    private void ApplyTierBackgroundColor(ItemTier tier)
    {
        if (backgroundImage == null) return;

        backgroundImage.sprite =
            CommonReferent.Instance.itemTierColorConfig.GetBackground(tier);

        backgroundImage.color = Color.white;
    }

    private void UpdateBuyButtonState()
    {
        if (currentItem == null || currentItem.itemData == null) return;

        bool enoughGold = CurrencyManager.Instance.Gold >= currentItem.itemData.price;
        buyButton.interactable = enoughGold;

        var colors = buyButton.colors;
        colors.normalColor = enoughGold ? Color.white : new Color(1f, 1f, 1f, 0.5f);
        buyButton.colors = colors;
    }

    private void OnGoldChanged(int gold)
    {
        if (gameObject.activeSelf)
            UpdateBuyButtonState();
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged -= OnGoldChanged;
    }
}
