using TMPro;
using UnityEngine;

public class PlayerStatsPopupUI : BasePopup
{
    [Header("Stat Texts")]
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI critText;
    [SerializeField] private TextMeshProUGUI lifestealText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;

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
        attackText.text =
            $"<sprite name=\"attack_icon\" color=#FF4D4D> " +
            $"<color=#FFD700>Attack:</color> {stats.attack.Value}";

        defenseText.text =
            $"<sprite name=\"defense_icon\" color=#4DA6FF> " +
            $"<color=#FFD700>Defense:</color> {stats.defense.Value}";

        speedText.text =
            $"<sprite name=\"speed_icon\" color=#7CFF4D> " +
            $"<color=#FFD700>Speed:</color> {stats.speed.Value}";

        critText.text =
            $"<sprite name=\"crit_icon\" color=#FFB84D> " +
            $"<color=#FFD700>Crit:</color> {stats.critChance.Value}%";

        lifestealText.text =
            $"<sprite name=\"lifesteal_icon\" color=#C44DFF> " +
            $"<color=#FFD700>LifeSteal:</color> {stats.lifeSteal.Value}%";

        attackSpeedText.text =
            $"<sprite name=\"attackspeed_icon\" color=#4DFFE3> " +
            $"<color=#FFD700>AttSpeed:</color> {stats.GetAttackSpeed():0.0}";
    }

}