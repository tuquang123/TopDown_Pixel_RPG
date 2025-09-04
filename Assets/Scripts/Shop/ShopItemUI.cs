using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public Image icon;
    public Image backgroundImage; 
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text tierText;
    public Button buyButton;

    private ItemData itemData;
    private ShopUI shopUI;

    public void Setup(ItemData data, ShopUI ui)
    {
        itemData = data;
        shopUI = ui;

        if (itemData == null)
        {
            Debug.LogError("ItemData is null in ShopItemUI.Setup.");
            return;
        }

        icon.sprite = data.icon;
        nameText.text = data.itemName; 
        tierText.text = data.tier.ToString();
        priceText.text = $"{data.price}";
        backgroundImage.color = ItemUtility.GetColorByTier(data.tier); 

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => shopUI.BuyItem(itemData));

        UpdateButtonState(CurrencyManager.Instance.Gold);
        CurrencyManager.Instance.OnGoldChanged += UpdateButtonState;
    }

    public void RefreshState()
    {
        UpdateButtonState(CurrencyManager.Instance.Gold);
    }

    private void UpdateButtonState(int gold)
    {
        if (itemData == null || shopUI == null || shopUI.playerInventory == null) return;

        bool isPurchased = false;

        // ✅ Check trong inventory
        foreach (var item in shopUI.playerInventory.items)
        {
            if (item.itemData != null && item.itemData.itemID == itemData.itemID)
            {
                isPurchased = true;
                break;
            }
        }

        // ✅ Check trong equipment
        if (!isPurchased)
        {
            var equipment = shopUI.playerInventory.GetComponent<Equipment>();
            if (equipment != null)
            {
                foreach (var kvp in equipment.equippedItems)
                {
                    if (kvp.Value != null && kvp.Value.itemData.itemID == itemData.itemID)
                    {
                        isPurchased = true;
                        break;
                    }
                }
            }
        }

        if (isPurchased)
        {
            buyButton.interactable = false;
            priceText.text = "Đã mua";
        }
        else
        {
            buyButton.interactable = gold >= itemData.price;
            priceText.text = $"{itemData.price}";
        }
    }



    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnGoldChanged -= UpdateButtonState;
        }
    }
}