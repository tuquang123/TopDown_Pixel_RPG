using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public ItemIconHandler icon;
    public Image backgroundImage; 
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text tierText;
    public Button buyButton;
    
    private ShopUI shopUI;
    private ItemInstance itemInstance;
    
    public void Setup(ItemInstance instance, ShopUI ui)
    {
        itemInstance = instance;
        shopUI = ui;

        var data = instance.itemData; 
        icon.SetupIcons(instance);

        nameText.text = data.itemName;
        tierText.text = data.tier.ToString();

        // ✔ Hiển thị giá + icon vàng
        priceText.text = $"{data.price} <sprite name=\"gold_icon\">";

        backgroundImage.color = ItemUtility.GetColorByTier(data.tier);

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => 
        {
            if (shopUI.detailPopup != null)
                shopUI.detailPopup.ShowDetail(itemInstance);
        });

        // ✔ tránh double-subscribe
        CurrencyManager.Instance.OnGoldChanged -= UpdateButtonState;
        CurrencyManager.Instance.OnGoldChanged += UpdateButtonState;

        UpdateButtonState(CurrencyManager.Instance.Gold);
    }
    
    public void RefreshState()
    {
        UpdateButtonState(CurrencyManager.Instance.Gold);
    }

    private void UpdateButtonState(int gold)
    {
        if (itemInstance == null || itemInstance.itemData == null || shopUI == null || shopUI.playerInventory == null) 
            return;

        var data = itemInstance.itemData;
        bool isPurchased = false;

        // ✔ check trong inventory
        foreach (var invItem in shopUI.playerInventory.items)
        {
            if (invItem == null || invItem.itemData == null) continue;
            if (invItem.itemData.itemID == data.itemID)
            {
                isPurchased = true;
                break;
            }
        }

        // ✔ check trong equipment
        if (!isPurchased)
        {
            var equipment = shopUI.playerInventory.GetComponent<Equipment>();
            if (equipment != null)
            {
                foreach (var kvp in equipment.equippedItems)
                {
                    if (kvp.Value == null || kvp.Value.itemData == null) continue;
                    if (kvp.Value.itemData.itemID == data.itemID)
                    {
                        isPurchased = true;
                        break;
                    }
                }
            }
        }

        // ✔ Update UI
        if (isPurchased)
        {
            buyButton.interactable = false;
            priceText.text = "Đã mua";
        }
        else 
        {
            buyButton.interactable = gold >= data.price;
            priceText.text = $"{data.price} <sprite name=\"gold_icon\">";
        }
    }
    
    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged -= UpdateButtonState;
    }
}
