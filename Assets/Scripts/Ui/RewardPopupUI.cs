using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardPopupUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;
    [SerializeField] private CanvasGroup canvasGroup;

    public RectTransform Rect { get; private set; }
    public CanvasGroup CanvasGroup => canvasGroup;

    void Awake()
    {
        Rect = GetComponent<RectTransform>();
    }

    public void Setup(Sprite sprite, string text)
    {
        icon.sprite = sprite;
        label.text = text;
    }
}
