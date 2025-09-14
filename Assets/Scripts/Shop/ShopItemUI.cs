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
        priceText.text = $"{data.price}";
        backgroundImage.color = ItemUtility.GetColorByTier(data.tier);

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => shopUI.BuyItem(itemInstance));
        
        UpdateButtonState(CurrencyManager.Instance.Gold);
        CurrencyManager.Instance.OnGoldChanged += UpdateButtonState;
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

        // ✅ Check trong inventory
        foreach (var invItem in shopUI.playerInventory.items)
        {
            if (invItem == null || invItem.itemData == null) continue; // bỏ slot trống
            if (invItem.itemData.itemID == data.itemID)
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
                    if (kvp.Value == null || kvp.Value.itemData == null) continue;
                    if (kvp.Value.itemData.itemID == data.itemID)
                    {
                        isPurchased = true;
                        break;
                    }
                }
            }
        }

        // ✅ Cập nhật UI nút
        if (isPurchased)
        {
            buyButton.interactable = false;
            priceText.text = "Đã mua";
            Debug.Log($"[ShopItemUI] {data.itemName} đã tồn tại trong inventory/equipment.");
        }
        else
        {
            buyButton.interactable = gold >= data.price;
            priceText.text = $"{data.price}";
            Debug.Log($"[ShopItemUI] {data.itemName} chưa mua. Gold: {gold}/{data.price} → interactable={buyButton.interactable}");
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