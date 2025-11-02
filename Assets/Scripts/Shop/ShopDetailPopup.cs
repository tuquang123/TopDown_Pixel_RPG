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
        priceText.text = $"{data.price} vàng";

        buyButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        // ⚙️ Gọi cập nhật trạng thái nút khi mở popup
        UpdateBuyButtonState();

        buyButton.onClick.AddListener(() =>
        {
            // Nếu đủ tiền mới cho mua
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

        // 🔄 Lắng nghe thay đổi vàng realtime
        CurrencyManager.Instance.OnGoldChanged += OnGoldChanged;
    }

    private void UpdateBuyButtonState()
    {
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
        CurrencyManager.Instance.OnGoldChanged -= OnGoldChanged;
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged -= OnGoldChanged;
    }
}
