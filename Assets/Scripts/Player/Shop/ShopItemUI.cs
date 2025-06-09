using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public Image icon;
    public Text nameText;
    public Text priceText;
    public Button buyButton;

    private ItemData itemData;
    private ShopUI shopUI;

    public void Setup(ItemData data, ShopUI ui)
    {
        itemData = data;
        shopUI = ui;

        icon.sprite = data.icon;
        nameText.text = data.itemName;
        priceText.text = data.price.ToString();

        buyButton.onClick.AddListener(() => shopUI.BuyItem(itemData));
    }
}