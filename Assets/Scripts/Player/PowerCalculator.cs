using TMPro;
using UnityEngine;

public class PowerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text powerText;

    private PlayerStats stats;

    private void Start()
    {
        if (CommonReferent.Instance == null) return;

        stats = CommonReferent.Instance.playerStats;
        if (stats == null) return;

        stats.OnStatsChanged += UpdatePower;
        UpdatePower();
    }

    private void OnDestroy()
    {
        if (stats != null)
            stats.OnStatsChanged -= UpdatePower;
    }

    private void UpdatePower()
    {
        if (powerText == null || stats == null) return;

        powerText.text = "Power: " + stats.CurrentPower.ToString("N0");
    }
}