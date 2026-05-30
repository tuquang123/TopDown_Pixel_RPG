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
    [SerializeField] public RectTransform goldIconRect;
    [SerializeField] public RectTransform gemIconRect;

    [Header("Collect Effect")]
    [SerializeField] private Image goldIconImage;
    [SerializeField] private Image gemIconImage;

    private Canvas canvas;
    private Tween goldTextTween;
    private Tween goldIconTween;
    private Tween gemTextTween;
    private Tween gemIconTween;

    void Awake()
    {
        Instance = this;
        canvas = GetComponentInParent<Canvas>();
    }

    void Start()
    {
        CurrencyManager.Instance.OnGoldChanged += UpdateGold;
        CurrencyManager.Instance.OnGemsChanged += UpdateGems;
        UpdateGold(CurrencyManager.Instance.Gold);
        UpdateGems(CurrencyManager.Instance.Gems);
    }

    void UpdateGold(int gold) => goldText.text = CurrencyManager.FormatGold(gold);
    void UpdateGems(int gems) => gemsText.text = $"{gems}";

    // ── Hiệu ứng khi Gold đến UI ──────────────────────────────
    public void PlayGoldCollectEffect()
    {
        // Icon: punch scale
        goldIconTween?.Kill();
        goldIconRect.localScale = Vector3.one;
        goldIconTween = goldIconRect
            .DOPunchScale(Vector3.one * 0.35f, 0.4f, vibrato: 2, elasticity: 0.5f);

        // Text: flash màu vàng sáng → về trắng
        goldTextTween?.Kill();
        goldText.color = Color.white;
        goldTextTween = goldText
            .DOColor(new Color(1f, 0.9f, 0.2f), 0.15f)
            .SetEase(Ease.OutFlash)
            .OnComplete(() =>
                goldText.DOColor(Color.white, 0.25f).SetEase(Ease.InSine));
    }

    // ── Hiệu ứng khi Gem đến UI ───────────────────────────────
    public void PlayGemCollectEffect()
    {
        gemIconTween?.Kill();
        gemIconRect.localScale = Vector3.one;
        gemIconTween = gemIconRect
            .DOPunchScale(Vector3.one * 0.35f, 0.4f, vibrato: 2, elasticity: 0.5f);

        gemTextTween?.Kill();
        gemsText.color = Color.white;
        gemTextTween = gemsText
            .DOColor(new Color(0.8f, 0.4f, 1f), 0.15f)
            .SetEase(Ease.OutFlash)
            .OnComplete(() =>
                gemsText.DOColor(Color.white, 0.25f).SetEase(Ease.InSine));
    }

    // ── Lấy world position của UI icon ────────────────────────
    public Vector3 GetWorldPosition(RectTransform uiRect)
    {
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, uiRect.position);
            return Camera.main.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane + 1f));
        }
        Vector3[] corners = new Vector3[4];
        uiRect.GetWorldCorners(corners);
        return (corners[0] + corners[2]) * 0.5f;
    }

    public Vector3 GoldTargetWorldPos => GetWorldPosition(goldIconRect);
    public Vector3 GemTargetWorldPos  => GetWorldPosition(gemIconRect);
}