using UnityEngine;

public class UIButtonPopupLink : MonoBehaviour
{
    [Header("Popup")]
    [SerializeField] private PopupType popupType;

    [Header("Options")]
    [SerializeField] private bool hideOthers = false;

    // FeatureButtonUI set cái này
    [HideInInspector] public bool isLocked = false;

    public void Open()
    {
        if (isLocked) return; // ← chặn tại đây

        if (hideOthers)
            UIManager.Instance.HideAllPopups();

        UIManager.Instance.ShowPopupByType(popupType);
    }

    public void Close()
    {
        UIManager.Instance.HidePopupByType(popupType);
    }
    
    public void Toggle()
    {
        if (isLocked) return;

        Debug.Log($"[UIButtonPopupLink] Toggle | popupType={popupType} | isOpen={UIManager.Instance.IsPopupOpen(popupType)}");

        if (UIManager.Instance.IsPopupOpen(popupType))
        {
            UIManager.Instance.HidePopupByType(popupType);
        }
        else
        {
            if (hideOthers)
                UIManager.Instance.HideAllPopups();

            UIManager.Instance.ShowPopupByType(popupType);
        }
    }
}