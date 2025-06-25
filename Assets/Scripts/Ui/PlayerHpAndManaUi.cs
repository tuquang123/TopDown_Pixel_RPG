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
        playerStats.OnHealthChanged += UpdateHPText;
        playerStats.OnManaChanged += UpdateManaText;

        // Gọi lần đầu để cập nhật UI khi bắt đầu
        UpdateHPText();
        UpdateManaText();
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= UpdateHPText;
            playerStats.OnManaChanged -= UpdateManaText;
        }
    }

    private void UpdateHPText()
    {
        hpText.text = $"HP: {playerStats.GetCurrentHealth()} / {(int)playerStats.maxHealth.Value}";
    }

    private void UpdateManaText()
    {
        manaText.text = $"Mana: {playerStats.currentMana} / {(int)playerStats.maxMana.Value}";
    }
}