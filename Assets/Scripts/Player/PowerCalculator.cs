using TMPro;
using UnityEngine;

public class PowerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text powerText;
    [SerializeField] private TMP_Text timeText;

    private PlayerStats stats;
    private float elapsedTime;

    private void Start()
    {
        if (CommonReferent.Instance == null) return;

        stats = CommonReferent.Instance.playerStats;
        if (stats == null) return;

        stats.OnStatsChanged += UpdatePower;
        stats.OnDeath        += ResetTime;
        UpdatePower();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        int hours   = (int)(elapsedTime / 3600);
        int minutes = (int)(elapsedTime % 3600 / 60);
        int seconds = (int)(elapsedTime % 60);

        if (timeText != null)
            timeText.text = hours > 0
                ? $"{hours:00}:{minutes:00}:{seconds:00}"
                : $"{minutes:00}:{seconds:00}";
    }

    private void OnDestroy()
    {
        if (stats != null)
        {
            stats.OnStatsChanged -= UpdatePower;
            stats.OnDeath        -= ResetTime;
        }
    }

    private void UpdatePower()
    {
        if (powerText == null || stats == null) return;
        powerText.text = (LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("power") : "Power") + ": " + stats.CurrentPower.ToString("N0");
    }

    private void ResetTime()
    {
        elapsedTime = 0f;
    }
}
