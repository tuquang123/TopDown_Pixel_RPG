using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RewardPopupManager : Singleton<RewardPopupManager>
{
    [SerializeField] private Transform popupParent;
    [SerializeField] private RewardPopupUI rewardPrefab;

    [Header("Icons")]
    [SerializeField] private Sprite expIcon;
    [SerializeField] private Sprite goldIcon;

    [Header("Stack Layout")]
    [SerializeField] private int maxVisible = 4;
    [SerializeField] private float spacing = 4f;
    [SerializeField] private float slotHeightOverride = 0f;
    [SerializeField] private float stackBottomY = 0f;

    [Header("Slide In (right → left)")]
    [SerializeField] private float slideStartX = 400f;
    [SerializeField] private float slideInDuration = 0.25f;

    [Header("Scale Punch")]
    [SerializeField] private float scaleDuration = 0.3f;

    [Header("Fly Up & Fade Out")]
    [SerializeField] private float visibleDuration = 1.8f;
    [SerializeField] private float flyUpY = 250f;
    [SerializeField] private float flyDuration = 0.5f;
    [SerializeField] private float fadeDuration = 0.4f;

    // runtime
    private readonly List<RewardPopupUI> _active = new();
    private Coroutine _dismissCoroutine;

    // Cached slot height — measured once from prefab at startup
    private float _slotHeight = -1f;

    // ── Lifecycle ────────────────────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();

        // FIX: popupParent không được dùng Layout Group (HorizontalLayoutGroup /
        // VerticalLayoutGroup / GridLayoutGroup). Nếu có, nó sẽ tự động sắp xếp lại
        // vị trí các con theo layout NGAY SAU khi ta set anchoredPosition bằng tay,
        // gây ra hiện tượng lệch hàng / chồng item khi 2-3 popup được tạo gần nhau
        // trong cùng một frame (đúng như lỗi trong ảnh).
        var layoutGroup = popupParent != null ? popupParent.GetComponent<LayoutGroup>() : null;
        if (layoutGroup != null)
        {
            Debug.LogWarning(
                $"[RewardPopupManager] popupParent '{popupParent.name}' có {layoutGroup.GetType().Name}. " +
                "Component này sẽ ghi đè anchoredPosition do code set, gây lệch hàng khi nhận nhiều item cùng lúc. " +
                "Đang tự động disable để layout chạy đúng.",
                popupParent);
            layoutGroup.enabled = false;
        }

        // Measure slot height immediately from prefab (no need to wait for a frame)
        MeasureSlotHeight();
    }

    private void MeasureSlotHeight()
    {
        if (slotHeightOverride > 0f)
        {
            _slotHeight = slotHeightOverride;
            return;
        }

        if (rewardPrefab == null) return;

        // Instantiate off-screen, measure, destroy immediately
        var probe = Instantiate(rewardPrefab, popupParent);
        probe.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(probe.Rect);
        _slotHeight = probe.Rect.rect.height + spacing;
        Destroy(probe.gameObject);
    }

    // ── Public API ───────────────────────────────────────────────────────────

    public void ShowReward(Sprite icon, string label, int quantity)
        => SpawnPopup(icon, $"{label}(+{quantity})");

    public void ShowGold(int amount)
    {
        if (amount <= 0) return;
        SpawnPopup(goldIcon, $"Gold(+{amount})");
    }

    public void ShowEXP(int amount)
    {
        if (amount <= 0) return;
        SpawnPopup(expIcon, $"EXP(+{amount})");
    }

    // ── Core ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Fully synchronous spawn — no coroutine, no frame delay.
    /// All layout calculations happen immediately so batch calls
    /// (EXP + Gold + Gem in one frame) stay perfectly aligned.
    /// </summary>
    private void SpawnPopup(Sprite icon, string text)
    {
        // Ensure slot height is known
        if (_slotHeight < 0f) MeasureSlotHeight();
        if (_slotHeight < 0f) _slotHeight = 50f; // last-resort fallback

        if (_active.Count >= maxVisible)
            RemoveOldest();

        // Create popup at slot-0 position (off-screen right, correct Y)
        RewardPopupUI popup = Instantiate(rewardPrefab, popupParent);
        popup.Setup(icon, text);
        popup.Rect.anchoredPosition = new Vector2(slideStartX, stackBottomY);
        popup.Rect.localScale = Vector3.one;
        popup.CanvasGroup.alpha = 1f;

        // FIX: Push existing popups up one slot.
        // Trước đây dùng DOKill(false) — chỉ DỪNG tween tại vị trí hiện tại
        // (vị trí giữa đường, không hoàn tất animation), khiến popup cũ kẹt ở
        // toạ độ Y không phải là slot hợp lệ. Khi một popup thứ 3 spawn tiếp
        // theo lại tính targetY dựa trên index, nhưng popup đang kẹt giữa
        // đường vẫn chưa kịp đến đúng slot => chồng/lệch hàng như trong ảnh.
        // Giải pháp: DOKill(true) để hoàn tất (complete) tween cũ trước khi gán
        // tween mới, đảm bảo luôn xuất phát từ một vị trí slot hợp lệ.
        for (int i = 0; i < _active.Count; i++)
        {
            float targetY = stackBottomY + (i + 1) * _slotHeight;
            _active[i].Rect.DOKill(true); // complete = true: nhảy thẳng tới vị trí slot cũ trước khi tween tiếp
            _active[i].Rect.DOAnchorPosY(targetY, slideInDuration).SetEase(Ease.OutCubic);
        }

        // Insert new popup at front and slide it in
        _active.Insert(0, popup);
        popup.Rect.DOAnchorPosX(0f, slideInDuration).SetEase(Ease.OutCubic);
        popup.Rect.DOPunchScale(Vector3.one * 0.12f, scaleDuration, 1, 0.5f);

        // Reset dismiss timer
        if (_dismissCoroutine != null)
            StopCoroutine(_dismissCoroutine);
        _dismissCoroutine = StartCoroutine(DismissAll());
    }

    private void RemoveOldest()
    {
        if (_active.Count == 0) return;
        int last = _active.Count - 1;
        var oldest = _active[last];
        _active.RemoveAt(last);
        oldest.Rect.DOKill(false);
        oldest.CanvasGroup.DOKill(false);
        oldest.CanvasGroup.DOFade(0f, 0.15f)
            .OnComplete(() => { if (oldest != null) Destroy(oldest.gameObject); });
    }

    private IEnumerator DismissAll()
    {
        yield return new WaitForSeconds(visibleDuration);

        var snapshot = new List<RewardPopupUI>(_active);
        _active.Clear();

        foreach (var popup in snapshot)
        {
            if (popup == null) continue;
            popup.Rect.DOKill(false);
            popup.CanvasGroup.DOKill(false);
            popup.Rect.DOAnchorPosY(popup.Rect.anchoredPosition.y + flyUpY, flyDuration)
                .SetEase(Ease.InCubic);
            popup.CanvasGroup.DOFade(0f, fadeDuration)
                .OnComplete(() => { if (popup != null) Destroy(popup.gameObject); });
        }

        _dismissCoroutine = null;
    }
}