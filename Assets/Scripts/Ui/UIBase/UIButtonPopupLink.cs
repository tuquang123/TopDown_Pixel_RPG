using UnityEngine;

public class UIButtonPopupLink : MonoBehaviour
{
    [SerializeField] private BasePopup targetPopup;
    [SerializeField] private bool hideOthers = false;

    public void Open()
    {
        if (hideOthers)
            UIManager.Instance.HideAllPopups();

        targetPopup?.Show();
    }

    public void Close()
    {
        targetPopup?.Hide();
    }

    public void Toggle()
    {
        if (targetPopup == null) return;

        if (targetPopup.gameObject.activeSelf)
            targetPopup.Hide();
        else
        {
            if (hideOthers)
                UIManager.Instance.HideAllPopups();

            targetPopup.Show();
        }
    }
}