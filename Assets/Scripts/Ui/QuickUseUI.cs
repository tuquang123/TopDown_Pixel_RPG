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
        int healthCount = healthPotion != null ? inventory.GetItemCount(healthPotion.itemData) : 0;
        int manaCount = manaPotion != null ? inventory.GetItemCount(manaPotion.itemData) : 0;

        // Health potion
        if (healthCount > 0)
        {
            healthPotionIcon.gameObject.SetActive(true);
            healthPotionButton.gameObject.SetActive(true);
            healthPotionCountText.text = $"x{healthCount}";
            healthPotionButton.interactable = !isHealthOnCooldown;
            if (healthPotion != null)
                healthPotionIcon.sprite = healthPotion.itemData.icon;
        }
        else
        {
            healthPotionIcon.gameObject.SetActive(false);
            healthPotionButton.gameObject.SetActive(false);
        }

        // Mana potion
        if (manaCount > 0)
        {
            manaPotionIcon.gameObject.SetActive(true);
            manaPotionButton.gameObject.SetActive(true);
            manaPotionCountText.text = $"x{manaCount}";
            manaPotionButton.interactable = !isManaOnCooldown;
            if (manaPotion != null)
                manaPotionIcon.sprite = manaPotion.itemData.icon;
        }
        else
        {
            manaPotionIcon.gameObject.SetActive(false);
            manaPotionButton.gameObject.SetActive(false);
        }
    }


}
