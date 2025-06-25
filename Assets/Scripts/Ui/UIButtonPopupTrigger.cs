using UnityEngine;

public class UIButtonPopupTrigger : MonoBehaviour
{
    public void OpenPlayerStatsPopup() => UIManager.Instance.ShowPopup<PlayerStatsPopupUI>();
    public void ClosePlayerStatsPopup() => UIManager.Instance.HidePopup<PlayerStatsPopupUI>();
    
    public void TogglePlayerStatsPopup()
    {
        var popup = UIManager.Instance.GetPopup<PlayerStatsPopupUI>();
        if (popup == null) return;

        if (popup.gameObject.activeSelf)
            popup.Hide();
        else
            popup.Show();
    }
}