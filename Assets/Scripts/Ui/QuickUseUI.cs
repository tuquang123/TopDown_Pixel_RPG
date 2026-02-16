using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class QuickUseUI : MonoBehaviour
{
    [Header("Button & Text")]
    public Button potionButton;
    public TMP_Text potionCountText;
    public Image potionIcon;

    [Header("Cooldown Settings")]
    public float potionCooldown = 3f;

    private Inventory inventory;
    private bool isOnCooldown = false;

    private void Start()
    {
        inventory = Inventory.Instance;

        potionButton.onClick.AddListener(UsePotion);
        inventory.OnInventoryChanged += UpdateQuickUseUI;

        UpdateQuickUseUI();
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= UpdateQuickUseUI;
    }

    private void UsePotion()
    {
        if (isOnCooldown) return;

        // Tìm potion có hồi HP hoặc Mana
        var potion = inventory.FindFirstConsumable(true, true);

        if (potion != null)
        {
            PlayerStats.Instance.Consume(potion.itemData);
            inventory.RemoveItem(potion);

            StartCoroutine(PotionCooldown());
        }

        UpdateQuickUseUI();
    }

    private IEnumerator PotionCooldown()
    {
        isOnCooldown = true;
        potionButton.interactable = false;

        float t = potionCooldown;

        while (t > 0f)
        {
            potionCountText.text = $"{t:0.0}s";
            t -= Time.deltaTime;
            yield return null;
        }

        isOnCooldown = false;
        UpdateQuickUseUI();
    }

    public void UpdateQuickUseUI()
    {
        var potion = inventory.FindFirstConsumable(true, true);

        int count = potion != null 
            ? inventory.GetItemCount(potion.itemData) 
            : 0;

        if (count > 0)
        {
            potionButton.gameObject.SetActive(true);
            potionIcon.gameObject.SetActive(true);

            if (!isOnCooldown)
                potionCountText.text = $"x{count}";

            potionButton.interactable = !isOnCooldown;

            if (potion != null)
                potionIcon.sprite = potion.itemData.icon;
        }
        else
        {
            potionButton.gameObject.SetActive(false);
            potionIcon.gameObject.SetActive(false);
        }
    }
}
