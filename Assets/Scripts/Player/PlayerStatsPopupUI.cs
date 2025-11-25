using TMPro;
using UnityEngine;

public class PlayerStatsPopupUI : BasePopup
{
    public StatDisplayComponent statDisplayComponent;
    public override void Show()
    {
        base.Show();

        if (PlayerStats.Instance != null)
        {
            statDisplayComponent.SetStats(PlayerStats.Instance);
        }
    }
}