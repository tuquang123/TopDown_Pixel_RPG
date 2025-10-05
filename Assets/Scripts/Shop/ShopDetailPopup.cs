using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopDetailPopup : MonoBehaviour
{
    [Header("References")]
    public Image icon;
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

        icon.sprite = data.icon;
        nameText.text = data.itemName;
        tierText.text = $"Cấp độ: {data.tier}";
        descriptionText.text = data.description;
        priceText.text = $"{data.price} vàng";

        buyButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        buyButton.onClick.AddListener(() =>
        {
            shopUI.BuyItem(currentItem);
            gameObject.SetActive(false);
        });

        cancelButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}