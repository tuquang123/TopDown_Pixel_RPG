using TMPro;
using UnityEngine;

public class StatsDetailPopupUI : BasePopup
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
    public void Close()
    {
        UIManager.Instance.HidePopupByType(PopupType.Stats);
    }
}