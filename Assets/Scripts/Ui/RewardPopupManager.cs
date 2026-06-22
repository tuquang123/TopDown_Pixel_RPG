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
    [SerializeField] private float spacing = 4f;        // khoảng trống giữa các dòng
    [SerializeField] private float slotHeightOverride = 0f; // nếu > 0: dùng giá trị này thay vì đọc từ prefab
    [SerializeField] private float stackBottomY = 0f;  // Y của item đầu tiên (mới nhất)

    [Header("Slide In (phải → trái)")]
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
    private float _slotHeight = -1f; // đọc 1 lần từ prefab thật

    // ── Public API ───────────────────────────────────────────────────────────

    public void ShowReward(Sprite icon, string label, int quantity)
        => SpawnPopup(icon, $"{label}(+{quantity})");

    public void ShowGold(int amount)
    {
        if (amount <= 0) return;
        SpawnPopup(goldIcon, $"Vàng(+{amount})");
    }

    public void ShowEXP(int amount)
    {
        if (amount <= 0) return;
        SpawnPopup(expIcon, $"EXP(+{amount})");
    }

    // ── Core ─────────────────────────────────────────────────────────────────

    private void SpawnPopup(Sprite icon, string text)
    {
        StartCoroutine(SpawnRoutine(icon, text));
    }

    private IEnumerator SpawnRoutine(Sprite icon, string text)
    {
        if (_active.Count >= maxVisible)
            RemoveOldest();

        // Instantiate trước để đọc chiều cao thực
        RewardPopupUI popup = Instantiate(rewardPrefab, popupParent);
        popup.Setup(icon, text);
        popup.Rect.anchoredPosition = new Vector2(slideStartX, stackBottomY);
        popup.Rect.localScale = Vector3.one;
        popup.CanvasGroup.alpha = 1f;

        // Chờ 1 frame để Canvas tính layout xong rồi đọc chiều cao
        yield return null;

        if (_slotHeight < 0f)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(popup.Rect);
            _slotHeight = slotHeightOverride > 0f
                ? slotHeightOverride
                : popup.Rect.rect.height + spacing;
        }

        // Đẩy các popup cũ lên 1 slot (dùng _slotHeight thực)
        for (int i = 0; i < _active.Count; i++)
        {
            float targetY = stackBottomY + (i + 1) * _slotHeight;
            _active[i].Rect.DOKill(false);
            _active[i].Rect.DOAnchorPosY(targetY, slideInDuration).SetEase(Ease.OutCubic);
        }

        // Popup mới ở slot 0, Y đã đặt sẵn — chỉ slide X
        _active.Insert(0, popup);
        popup.Rect.DOAnchorPosX(0f, slideInDuration).SetEase(Ease.OutCubic);
        popup.Rect.DOPunchScale(Vector3.one * 0.12f, scaleDuration, 1, 0.5f);

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