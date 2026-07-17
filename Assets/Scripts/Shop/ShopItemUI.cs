using System.Collections.Generic;
using System.Linq;
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

    private void OnEnable()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += RefreshLocalizedText;
        }

        RefreshLocalizedText();
    }

    private void OnDisable()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= RefreshLocalizedText;
        }
    }

    public void Setup(ItemInstance instance, ShopUI ui)
    {
        itemInstance = instance;
        shopUI = ui;

        var data = instance.itemData;
        icon.SetupIcons(instance);

        nameText.text = data.itemName;
        tierText.text = ItemUtility.GetLocalizedTier(data.tier);
        priceText.text = $"{data.price} <sprite name=\"gold_icon\">";

        backgroundImage.sprite =
            CommonReferent.Instance.itemTierColorConfig.GetBackground(data.tier);

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() =>
        {
            if (shopUI.DetailPopupUI != null)
                shopUI.DetailPopupUI.Show(itemInstance);
        });

        CurrencyManager.Instance.OnGoldChanged -= UpdateButtonState;
        CurrencyManager.Instance.OnGoldChanged += UpdateButtonState;

        UpdateButtonState(CurrencyManager.Instance.Gold);
    }

    public void RefreshState()
    {
        UpdateButtonState(CurrencyManager.Instance.Gold);
    }

    private void RefreshLocalizedText()
    {
        if (itemInstance == null || itemInstance.itemData == null) return;

        nameText.text = itemInstance.itemData.itemName;
        tierText.text = ItemUtility.GetLocalizedTier(itemInstance.itemData.tier);

        if (CurrencyManager.Instance != null)
        {
            UpdateButtonState(CurrencyManager.Instance.Gold);
        }
    }

    private void UpdateButtonState(int gold)
    {
        if (itemInstance == null || itemInstance.itemData == null
            || shopUI == null || shopUI.PlayerInventory == null)
            return;

        var data = itemInstance.itemData;

        // ===== CONSUMABLE =====
        if (data.itemType == ItemType.Consumable)
        {
            buyButton.interactable = true;
            int amount = shopUI.PlayerInventory.GetItemCount(data);
            priceText.text = $"{data.price} <sprite name=\"gold_icon\"> (x{amount})";
            return;
        }

        // ===== EQUIPMENT =====
        bool isPurchased = shopUI.PlayerInventory.items
            .Any(i => i?.itemData?.itemID == data.itemID);

        if (!isPurchased)
        {
            var equipment = shopUI.PlayerInventory.GetComponent<Equipment>();
            if (equipment != null)
                isPurchased = equipment.equippedItems.Values
                    .Any(v => v?.itemData?.itemID == data.itemID);
        }

        if (isPurchased)
        {
            buyButton.interactable = false;
            priceText.text = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("purchased") : "Đã mua";
        }
        else
        {
            buyButton.interactable = true;
            priceText.text = $"{data.price} <sprite name=\"gold_icon\">";
        }
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged -= UpdateButtonState;

        if (LanguageManager.Instance != null)
            LanguageManager.Instance.OnLanguageChanged -= RefreshLocalizedText;
    }
}
