using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CurrencyUI : MonoBehaviour
{
    public static CurrencyUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI gemsText;

    [Header("Icon References")]
    [SerializeField] private RectTransform goldIconRect;
    [SerializeField] private RectTransform gemIconRect;

    [Header("Collect Effect")]
    [SerializeField] private Image goldIconImage;
    [SerializeField] private Image gemIconImage;

    private Canvas canvas;

    private Tween goldTextTween;
    private Tween goldIconTween;
    private Tween gemTextTween;
    private Tween gemIconTween;

    private void Awake()
    {
        Instance = this;
        canvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        CurrencyManager.Instance.OnGoldChanged += UpdateGold;
        CurrencyManager.Instance.OnGemsChanged += UpdateGems;

        UpdateGold(CurrencyManager.Instance.Gold);
        UpdateGems(CurrencyManager.Instance.Gems);
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance == null) return;

        CurrencyManager.Instance.OnGoldChanged -= UpdateGold;
        CurrencyManager.Instance.OnGemsChanged -= UpdateGems;
    }

    private void UpdateGold(int gold)
    {
        goldText.text = CurrencyManager.FormatGold(gold);
    }

    private void UpdateGems(int gems)
    {
        gemsText.text = gems.ToString();
    }

    public void PlayGoldCollectEffect()
    {
        goldIconTween?.Kill();

        goldIconRect.localScale = Vector3.one;

        goldIconTween = goldIconRect
            .DOPunchScale(
                Vector3.one * 0.2f,
                0.25f,
                2,
                0.5f);

        goldTextTween?.Kill();

        goldText.color = Color.white;

        goldTextTween = goldText
            .DOColor(new Color(1f, 0.9f, 0.3f), 0.1f)
            .OnComplete(() =>
            {
                goldText.DOColor(Color.white, 0.2f);
            });
    }

    public void PlayGemCollectEffect()
    {
        gemIconTween?.Kill();

        gemIconRect.localScale = Vector3.one;

        gemIconTween = gemIconRect
            .DOPunchScale(
                Vector3.one * 0.2f,
                0.25f,
                2,
                0.5f);

        gemTextTween?.Kill();

        gemsText.color = Color.white;

        gemTextTween = gemsText
            .DOColor(new Color(0.8f, 0.5f, 1f), 0.1f)
            .OnComplete(() =>
            {
                gemsText.DOColor(Color.white, 0.2f);
            });
    }

    public Vector3 GetWorldPosition(RectTransform uiRect)
    {
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Vector3 screenPos =
                RectTransformUtility.WorldToScreenPoint(
                    null,
                    uiRect.position);

            return Camera.main.ScreenToWorldPoint(
                new Vector3(
                    screenPos.x,
                    screenPos.y,
                    Camera.main.nearClipPlane + 2f));
        }

        Vector3[] corners = new Vector3[4];
        uiRect.GetWorldCorners(corners);

        return (corners[0] + corners[2]) * 0.5f;
    }

    public Vector3 GoldTargetWorldPos =>
        GetWorldPosition(goldIconRect);

    public Vector3 GemTargetWorldPos =>
        GetWorldPosition(gemIconRect);
}