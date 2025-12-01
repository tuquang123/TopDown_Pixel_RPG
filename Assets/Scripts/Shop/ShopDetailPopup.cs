using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private ShopUI shopUI;
    private ItemInstance currentItem;

    public void Setup(ShopUI shop)
    {
        shopUI = shop;
        gameObject.SetActive(false);
    }

    public void ShowDetail(ItemInstance instance)
    {
        if (instance == null || instance.itemData == null) return;

        currentItem = instance;
        var data = instance.itemData;

        gameObject.SetActive(true);

        icon.SetupIcons(instance);
        nameText.text = data.itemName;
        tierText.text = $"{data.tier}";
        descriptionText.text = data.description;
        
        statDisplayComponent.SetStats(instance);

        priceText.text = $"{data.price} <sprite name=\"gold_icon\" > ";

        buyButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        UpdateBuyButtonState();

        buyButton.onClick.AddListener(() =>
        {
            if (CurrencyManager.Instance.Gold >= data.price)
            {
                shopUI.BuyItem(currentItem);
                gameObject.SetActive(false);
            }
            else
            {
                GameEvents.OnShowToast.Raise("Gold not enough!");
            }
        });

        cancelButton.onClick.AddListener(() => gameObject.SetActive(false));

        // đăng ký listener cập nhật vàng
        CurrencyManager.Instance.OnGoldChanged += OnGoldChanged;
    }

    private void UpdateBuyButtonState()
    {
        if (currentItem == null || currentItem.itemData == null) return;

        var enoughGold = CurrencyManager.Instance.Gold >= currentItem.itemData.price;
        buyButton.interactable = enoughGold;

        var colors = buyButton.colors;
        colors.normalColor = enoughGold ? Color.white : new Color(1, 1, 1, 0.5f);
        buyButton.colors = colors;
    }

    private void OnGoldChanged(int gold)
    {
        if (gameObject.activeSelf)
            UpdateBuyButtonState();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged -= OnGoldChanged;
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged -= OnGoldChanged;
    }

    // ----- HÀM BUILD STAT (bổ sung) -----
    private string BuildStatText(ItemInstance item)
    {
        if (item == null || item.itemData == null) return "";

        var data = item.itemData;
        string stats = "";

        void AddStatLine(string label, ItemStatBonus bonus, float upgradePercent = 0.1f, string suffix = "")
        {
            if (bonus == null || !bonus.HasValue) return;

            // Nếu item trong shop thường là chưa nâng cấp, dùng giá trị flat trực tiếp
            float flat = bonus.flat;
            float percent = bonus.percent;

            bool showDecimal = (label == "Tốc đánh" || label == "Tốc phép" || label == "Speed");

            if (Mathf.Abs(flat) > 0.0001f)
                stats += showDecimal
                    ? $"{label}: {flat:F1}{suffix}\n"
                    : $"{label}: {Mathf.RoundToInt(flat)}{suffix}\n";

            if (Mathf.Abs(percent) > 0.0001f)
                stats += showDecimal
                    ? $"{label}: +{percent:F1}%{suffix}\n"
                    : $"{label}: +{percent}%{suffix}\n";
        }

        AddStatLine("Dame", data.attack);
        AddStatLine("Giáp", data.defense);
        AddStatLine("Máu", data.health);
        AddStatLine("Mana", data.mana);
        AddStatLine("Crit", data.critChance, 0.05f);
        AddStatLine("Speed", data.speed, 0.05f);
        AddStatLine("Tốc đánh", data.attackSpeed, 0.05f);
        AddStatLine("Hút máu", data.lifeSteal, 0.05f);

        return stats.TrimEnd();
    }
}
