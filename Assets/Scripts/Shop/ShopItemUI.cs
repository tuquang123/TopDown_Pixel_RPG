using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
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

        backgroundImage.sprite =
            CommonReferent.Instance.itemTierColorConfig
                .GetBackground(data.tier);
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => 
        {
            if (shopUI.DetailPopupUI != null)
                shopUI.DetailPopupUI.Show(itemInstance);
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
        if (itemInstance == null || itemInstance.itemData == null || shopUI == null || shopUI.PlayerInventory == null) 
            return;

        var data = itemInstance.itemData;

        // ===== CONSUMABLE =====
        if (data.itemType == ItemType.Consumable)
        {
            buyButton.interactable = gold >= data.price;

            int amount = shopUI.PlayerInventory.GetItemCount(data);

            priceText.text = $"{data.price} <sprite name=\"gold_icon\"> (x{amount})";
            return;
        }

        // ===== EQUIPMENT =====
        bool isPurchased = false;

        foreach (var invItem in shopUI.PlayerInventory.items)
        {
            if (invItem == null || invItem.itemData == null) continue;

            if (invItem.itemData.itemID == data.itemID)
            {
                isPurchased = true;
                break;
            }
        }

        if (!isPurchased)
        {
            var equipment = shopUI.PlayerInventory.GetComponent<Equipment>();
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
