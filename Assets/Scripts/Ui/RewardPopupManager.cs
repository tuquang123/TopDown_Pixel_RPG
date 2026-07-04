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

    private void SpawnPopup(Sprite icon, string text)
    {
        if (_slotHeight < 0f) MeasureSlotHeight();
        if (_slotHeight < 0f) _slotHeight = 50f;

        if (_active.Count >= maxVisible)
            RemoveOldest();

        RewardPopupUI popup = Instantiate(rewardPrefab, popupParent);
        popup.Setup(icon, text);
        popup.Rect.anchoredPosition = new Vector2(slideStartX, stackBottomY);
        popup.Rect.localScale = Vector3.one;
        popup.CanvasGroup.alpha = 1f;

        for (int i = 0; i < _active.Count; i++)
        {
            float targetY = stackBottomY + (i + 1) * _slotHeight;
            _active[i].Rect.DOKill(true);
            _active[i].Rect.DOAnchorPosY(targetY, slideInDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true); // FIX: chạy bằng unscaled time, không bị đứng khi timeScale = 0
        }

        _active.Insert(0, popup);
        popup.Rect.DOAnchorPosX(0f, slideInDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true); // FIX
        popup.Rect.DOPunchScale(Vector3.one * 0.12f, scaleDuration, 1, 0.5f)
            .SetUpdate(true); // FIX

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
            .SetUpdate(true) // FIX
            .OnComplete(() => { if (oldest != null) Destroy(oldest.gameObject); });
    }

    private IEnumerator DismissAll()
    {
        // FIX: WaitForSeconds bị đứng khi timeScale = 0, đổi sang Realtime
        yield return new WaitForSecondsRealtime(visibleDuration);

        var snapshot = new List<RewardPopupUI>(_active);
        _active.Clear();

        foreach (var popup in snapshot)
        {
            if (popup == null) continue;
            popup.Rect.DOKill(false);
            popup.CanvasGroup.DOKill(false);
            popup.Rect.DOAnchorPosY(popup.Rect.anchoredPosition.y + flyUpY, flyDuration)
                .SetEase(Ease.InCubic)
                .SetUpdate(true); // FIX
            popup.CanvasGroup.DOFade(0f, fadeDuration)
                .SetUpdate(true) // FIX
                .OnComplete(() => { if (popup != null) Destroy(popup.gameObject); });
        }

        _dismissCoroutine = null;
    }
}