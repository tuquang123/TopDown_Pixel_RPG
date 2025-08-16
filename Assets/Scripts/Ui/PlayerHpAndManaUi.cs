using UnityEngine;
using TMPro;

public class PlayerHpAndManaUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI manaText;

    [SerializeField] private PlayerStats playerStats;
    
    private void Start()
    {
        playerStats = PlayerStats.Instance;

        if (playerStats != null)
        {
            playerStats.OnHealthChanged += UpdateAllText;
            playerStats.OnManaChanged += UpdateAllText;
            playerStats.OnStatsChanged += UpdateAllText;

            UpdateAllText();
        }
        else
        {
            Debug.LogError("PlayerStats.Instance chưa được khởi tạo!");
        }
    }


    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= UpdateAllText;
            playerStats.OnManaChanged -= UpdateAllText;
            playerStats.OnStatsChanged-= UpdateAllText;
        }
    }

    private void UpdateAllText()
    {
        hpText.text = $"HP: {playerStats.GetCurrentHealth()} / {(int)playerStats.maxHealth.Value}";
        manaText.text = $"Mana: {playerStats.currentMana} / {(int)playerStats.maxMana.Value}";
    }

}