using UnityEngine;
using UnityEngine.UI;

public class MapPopup : BasePopup
{
    [Header("Map Elements")]
    public GameObject mapContent;   // map chính
    public Button closeButton;

    protected override void Awake()
    {
        base.Awake();

        if (closeButton != null)
            closeButton.onClick.AddListener(() =>
            {
                UIManager.Instance.HidePopupByType(PopupType.Map);
            });
    }

    private void Start()
    {
        Show(); // mở popup ngay khi spawn
    }
}