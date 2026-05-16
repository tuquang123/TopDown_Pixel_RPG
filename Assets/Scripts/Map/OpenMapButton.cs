using UnityEngine;
using UnityEngine.UI;

public class OpenMapButton : MonoBehaviour
{
    public Button button;

    private void Awake()
    {
        button.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPopupByType(PopupType.Map);
        });
    }
}