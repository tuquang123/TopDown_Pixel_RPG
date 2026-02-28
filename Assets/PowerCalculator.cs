using TMPro;
using UnityEngine;

public class PowerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text powerText;

    private void OnEnable()
    {
        CommonReferent.Instance.playerStats.OnStatsChanged += UpdatePower;
        UpdatePower(); // cập nhật lần đầu
    }

    private void OnDisable()
    {
        if ( CommonReferent.Instance.playerStats != null)
            CommonReferent.Instance.playerStats.OnStatsChanged -= UpdatePower;
    }

    private void UpdatePower()
    {
        powerText.text = "Power: " +  CommonReferent.Instance.playerStats.CurrentPower.ToString("N0");
    }
}