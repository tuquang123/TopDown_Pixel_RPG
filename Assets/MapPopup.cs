using UnityEngine;
using UnityEngine.UI;

public class MapPopup : BasePopup
{
    [Header("Data")]
    public LevelDatabase levelDatabase;

    [Header("UI")]
    public Transform mapContent;
    public Button closeButton;

    private MapNodeUI[] nodes;

    protected override void Awake()
    {
        base.Awake();

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() =>
            {
                UIManager.Instance.HidePopupByType(PopupType.Map);
            });
        }
    }

    private void Start()
    {
        Show();

        nodes = mapContent.GetComponentsInChildren<MapNodeUI>(true);

        InitNodes();
        RefreshMap();
    }

    void InitNodes()
    {
        foreach (var node in nodes)
        {
            node.Init(levelDatabase);
        }
    }

    public void RefreshMap()
    {
        int currentIndex = LevelManager.Instance.CurrentLevel;

        foreach (var node in nodes)
        {
            node.Refresh(currentIndex);
        }
    }
}