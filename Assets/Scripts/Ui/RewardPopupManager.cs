using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RewardPopupManager : Singleton<RewardPopupManager>
{
    [SerializeField] private Transform popupParent;
    [SerializeField] private RewardPopupUI rewardPrefab;

    [Header("Popup Timing")]
    [SerializeField] private float scaleDuration = 0.3f;
    [SerializeField] private float moveInDuration = 0.3f;
    [SerializeField] private float visibleDuration = 1.2f;
    [SerializeField] private float moveOutDuration = 0.5f;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Popup Position")]
    [SerializeField] private float startYOffset = -200f;
    [SerializeField] private float endYOffset = 200f;

    private Queue<RewardData> rewardQueue = new();
    private bool isShowing = false;

    public void ShowReward(Sprite icon, string name, int quantity)
    {
        rewardQueue.Enqueue(new RewardData(icon, name, quantity));
        if (!isShowing)
            StartCoroutine(ProcessQueue());
    }

    private System.Collections.IEnumerator ProcessQueue()
    {
        isShowing = true;

        while (rewardQueue.Count > 0)
        {
            var data = rewardQueue.Dequeue();
            RewardPopupUI popup = Instantiate(rewardPrefab, popupParent);

            popup.Setup(data.icon, $"{data.name} x{data.quantity}");

            // reset state
            popup.Rect.anchoredPosition = new Vector2(0, startYOffset);
            popup.Rect.localScale = Vector3.zero;
            popup.CanvasGroup.alpha = 0f;

            // animation sequence
            Sequence seq = DOTween.Sequence();
            seq.Append(popup.Rect.DOScale(1f, scaleDuration).SetEase(Ease.OutBack));
            seq.Join(popup.Rect.DOAnchorPosY(0, moveInDuration).SetEase(Ease.OutCubic));
            seq.Join(popup.CanvasGroup.DOFade(1f, fadeDuration));
            seq.AppendInterval(visibleDuration);
            seq.Append(popup.Rect.DOAnchorPosY(endYOffset, moveOutDuration).SetEase(Ease.InCubic));
            seq.Join(popup.CanvasGroup.DOFade(0f, fadeDuration));
            seq.OnComplete(() => Destroy(popup.gameObject));

            yield return seq.WaitForCompletion();
        }

        isShowing = false;
    }

    private struct RewardData
    {
        public Sprite icon;
        public string name;
        public int quantity;

        public RewardData(Sprite i, string n, int q)
        {
            icon = i;
            name = n;
            quantity = q;
        }
    }
}
