using TMPro;
using UnityEngine;

public class PlayerStatsPopupUI : BasePopup
{
    [SerializeField] private TextMeshProUGUI statsText;

    public override void Show()
    {
        base.Show();

        if (PlayerStats.Instance != null)
        {
            ShowStats(PlayerStats.Instance);
        }
    }

    private void ShowStats(PlayerStats stats)
    {
        statsText.text =
            $"<color=#FFD700>Attack:</color> {stats.attack.Value}\n" +
            $"<color=#FFD700>Defense:</color> {stats.defense.Value}\n" +
            $"<color=#FFD700>Speed:</color> {stats.speed.Value}\n" +
            $"<color=#FFD700>Crit:</color> {stats.critChance.Value}%\n" +
            $"<color=#FFD700>LifeSteal:</color> {stats.lifeSteal.Value}%\n" +
            $"<color=#FFD700>AttSpeed:</color> {stats.GetAttackSpeed().ToString("0.0")}";
    }
}