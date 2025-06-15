using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text priceText;
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
        
        // Disable nếu đã mua
        if (shopUI.playerInventory.HasItem(data))
        {
            buyButton.interactable = false;
            priceText.text = "Đã mua";
        }
    }
}