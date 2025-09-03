using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class QuickUseUI : MonoBehaviour
{
    [Header("Buttons & Text")]
    public Button healthPotionButton;
    public Button manaPotionButton;
    public TMP_Text healthPotionCountText;
    public TMP_Text manaPotionCountText;

    [Header("Cooldown Settings")]
    public float potionCooldown = 3f; 

    private Inventory inventory;
    [SerializeField] private InventoryUI inventoryUI;
    private bool isHealthOnCooldown = false;
    private bool isManaOnCooldown = false;
    [SerializeField] private Image healthPotionIcon;
    [SerializeField] private Image manaPotionIcon;

    private void Start()
    {
        inventory = Inventory.Instance;

        healthPotionButton.onClick.AddListener(UseHealthPotion);
        manaPotionButton.onClick.AddListener(UseManaPotion);

        // Lắng nghe event
        inventory.OnInventoryChanged += UpdateQuickUseUI;

        // Refresh ban đầu
        UpdateQuickUseUI();
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= UpdateQuickUseUI;
    }


    private void UseHealthPotion()
    {
        if (isHealthOnCooldown) return;

        var potion = inventory.FindFirstConsumable(true, false); // chỉ health
        if (potion != null)
        {
            PlayerStats.Instance.Consume(potion.itemData);
            inventory.RemoveItem(potion);
            inventoryUI.UpdateInventoryUI();

            StartCoroutine(PotionCooldown(healthPotionButton, true));
        }
        UpdateQuickUseUI();
    }

    private void UseManaPotion()
    {
        if (isManaOnCooldown) return;

        var potion = inventory.FindFirstConsumable(false, true); // chỉ mana
        if (potion != null)
        {
            PlayerStats.Instance.Consume(potion.itemData);
            inventory.RemoveItem(potion);
            inventoryUI.UpdateInventoryUI();

            StartCoroutine(PotionCooldown(manaPotionButton, false));
        }
        UpdateQuickUseUI();
    }

    private IEnumerator PotionCooldown(Button button, bool isHealth)
    {
        if (isHealth) isHealthOnCooldown = true;
        else isManaOnCooldown = true;

        button.interactable = false;

        float t = potionCooldown;
        while (t > 0f)
        {
            // Có thể hiển thị số giây còn lại lên text thay cho "xN"
            if (isHealth)
                healthPotionCountText.text = $"{t:0.0}s";
            else
                manaPotionCountText.text = $"{t:0.0}s";

            t -= Time.deltaTime;
            yield return null;
        }

        if (isHealth) isHealthOnCooldown = false;
        else isManaOnCooldown = false;

        UpdateQuickUseUI();
    }

    public void UpdateQuickUseUI()
    {
        var healthPotion = inventory.FindFirstConsumable(true, false);
        var manaPotion = inventory.FindFirstConsumable(false, true);

        // Update số lượng
        healthPotionCountText.text = healthPotion != null 
            ? $"x{inventory.GetItemCount(healthPotion.itemData)}" 
            : "0";
        manaPotionCountText.text = manaPotion != null 
            ? $"x{inventory.GetItemCount(manaPotion.itemData)}" 
            : "0";

        // Update icon
        if (healthPotion != null)
            healthPotionIcon.sprite = healthPotion.itemData.icon;
        if (manaPotion != null)
            manaPotionIcon.sprite = manaPotion.itemData.icon;

        // Nếu có bình + không cooldown thì enable
        healthPotionButton.interactable = healthPotion != null && !isHealthOnCooldown;
        manaPotionButton.interactable = manaPotion != null && !isManaOnCooldown;
    }

}
