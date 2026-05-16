using UnityEngine;

public class UIButtonPopupLink : MonoBehaviour
{
    [Header("Popup")]
    [SerializeField] private PopupType popupType;

    [Header("Options")]
    [SerializeField] private bool hideOthers = false;

    /// <summary>
    /// Mở popup
    /// </summary>
    public void Open()
    {
        if (hideOthers)
        {
            UIManager.Instance.HideAllPopups();
        }

        UIManager.Instance.ShowPopupByType(popupType);
    }

    /// <summary>
    /// Đóng popup
    /// </summary>
    public void Close()
    {
        UIManager.Instance.HidePopupByType(popupType);
    }

    /// <summary>
    /// Bật / tắt popup
    /// </summary>
    public void Toggle()
    {
        if (UIManager.Instance.IsPopupOpen(popupType))
        {
            UIManager.Instance.HidePopupByType(popupType);
        }
        else
        {
            if (hideOthers)
            {
                UIManager.Instance.HideAllPopups();
            }

            UIManager.Instance.ShowPopupByType(popupType);
        }
    }
}