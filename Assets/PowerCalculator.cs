using TMPro;
using UnityEngine;

public class PowerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text powerText;

    private void OnEnable()
    {
        if (CommonReferent.Instance == null) return;
        if (CommonReferent.Instance.playerStats == null) return;

        CommonReferent.Instance.playerStats.OnStatsChanged += UpdatePower;
        UpdatePower();
    }

    private void OnDisable()
    {
        if (CommonReferent.Instance == null) return;
        if (CommonReferent.Instance.playerStats == null) return;

        CommonReferent.Instance.playerStats.OnStatsChanged -= UpdatePower;
    }

    private void UpdatePower()
    {
        if (powerText == null) return;

        powerText.text = "Power: " + CommonReferent.Instance.playerStats.CurrentPower.ToString("N0");
    }
}