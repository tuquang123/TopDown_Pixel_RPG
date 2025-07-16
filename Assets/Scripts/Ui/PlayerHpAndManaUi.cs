using UnityEngine;
using TMPro;

public class PlayerHpAndManaUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI manaText;

    private PlayerStats playerStats;

    private void Start()
    {
        playerStats = PlayerStats.Instance;

        // Đăng ký sự kiện để cập nhật UI khi máu hoặc mana thay đổi
        playerStats.OnHealthChanged += UpdateAllText;
        playerStats.OnManaChanged += UpdateAllText;
        playerStats.OnStatsChanged += UpdateAllText;
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