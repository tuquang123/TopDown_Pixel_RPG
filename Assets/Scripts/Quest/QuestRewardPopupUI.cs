using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestRewardPopupUI : BasePopup
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;

    [Header("Gold Slot")]
    [SerializeField] private GameObject goldSlot;
    [SerializeField] private TextMeshProUGUI goldAmountText;

    [Header("Gem Slot")]
    [SerializeField] private GameObject gemSlot;
    [SerializeField] private TextMeshProUGUI gemAmountText;

    [Header("EXP Slot")]
    [SerializeField] private GameObject expSlot;
    [SerializeField] private TextMeshProUGUI expAmountText;

    [Header("Button")]
    [SerializeField] private Button confirmButton;

    [Header("Animation")]
    [SerializeField] private float animDuration = 0.35f;

    [Header("Light Burst Effect")]
    [SerializeField] private RectTransform lightBurstOuter;
    [SerializeField] private RectTransform lightBurstInner;
    [SerializeField] private Color burstColorOuter = new Color(1f, 0.85f, 0.2f, 0.55f);
    [SerializeField] private Color burstColorInner = new Color(1f, 1f, 0.9f, 0.45f);

    private Sequence _burstSequence;
    private CanvasGroup _canvasGroup;

    protected override void Awake()
    {
        base.Awake();

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(OnConfirmClick);
            confirmButton.onClick.AddListener(OnConfirmClick);
        }

        TryBuildBurstSprites();
    }

    private void TryBuildBurstSprites()
    {
        if (lightBurstOuter != null)
        {
            var imgOuter = lightBurstOuter.GetComponent<Image>();
            if (imgOuter != null && imgOuter.sprite == null)
                imgOuter.sprite = CreateStarburstSprite(256, 16, 0.55f);
            if (imgOuter != null)
                imgOuter.color = burstColorOuter;
        }

        if (lightBurstInner != null)
        {
            var imgInner = lightBurstInner.GetComponent<Image>();
            if (imgInner != null && imgInner.sprite == null)
                imgInner.sprite = CreateStarburstSprite(256, 10, 0.65f);
            if (imgInner != null)
                imgInner.color = burstColorInner;
        }
    }

    private Sprite CreateStarburstSprite(int size, int rayCount, float rayWidthFactor)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float center = size * 0.5f;
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float r  = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx);

                float radialFade = Mathf.Clamp01(1f - r / center);
                radialFade *= radialFade;

                float rayPattern = Mathf.Cos(angle * rayCount * 0.5f);
                rayPattern = Mathf.Pow(Mathf.Max(0f, rayPattern), 1f / rayWidthFactor);

                pixels[y * size + x] = new Color(1f, 1f, 1f, radialFade * rayPattern);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    // ─── Light Burst ───────────────────────────────────────────────────────────
    private void PlayBurstEffect()
    {
        _burstSequence?.Kill();
        _burstSequence = DOTween.Sequence().SetUpdate(true);

        if (lightBurstOuter != null)
        {
            lightBurstOuter.localRotation = Quaternion.identity;
            lightBurstOuter.localScale    = Vector3.one;
            _burstSequence.Join(
                lightBurstOuter.DORotate(new Vector3(0, 0, -360f), 6f, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart));
            _burstSequence.Join(
                lightBurstOuter.DOScale(Vector3.one * 1.08f, 1.2f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo));
        }

        if (lightBurstInner != null)
        {
            lightBurstInner.localRotation = Quaternion.identity;
            lightBurstInner.localScale    = Vector3.one;
            _burstSequence.Join(
                lightBurstInner.DORotate(new Vector3(0, 0, 360f), 4f, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart));

            var imgInner = lightBurstInner.GetComponent<Image>();
            if (imgInner != null)
                _burstSequence.Join(
                    imgInner.DOFade(0.2f, 0.8f)
                        .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                        .From(burstColorInner.a));
        }
    }

    private void StopBurstEffect()
    {
        _burstSequence?.Kill();
        _burstSequence = null;
    }

    // ─── Public API ────────────────────────────────────────────────────────────
    public void ShowReward(QuestReward reward, string questName = "")
    {
        if (reward == null) return;

        if (titleText != null)
            titleText.text = "Achievement Rewards!";

        if (subtitleText != null)
            subtitleText.text = GetRewardSubtitle(reward, questName);

        bool hasGold = reward.goldReward > 0;
        if (goldSlot != null) goldSlot.SetActive(hasGold);
        if (hasGold && goldAmountText != null)
            goldAmountText.text = reward.goldReward.ToString();

        bool hasGem = reward.gemReward > 0;
        if (gemSlot != null) gemSlot.SetActive(hasGem);
        if (hasGem && gemAmountText != null)
            gemAmountText.text = reward.gemReward.ToString();

        bool hasExp = reward.experienceReward > 0;
        if (expSlot != null) expSlot.SetActive(hasExp);
        if (hasExp && expAmountText != null)
            expAmountText.text = reward.experienceReward.ToString();

        base.Show();
        StopAllCoroutines();
        StartCoroutine(AnimateIn());
        PlayBurstEffect();
    }

    // ─── Subtitle ──────────────────────────────────────────────────────────────
    private string GetRewardSubtitle(QuestReward reward, string questName)
    {
        var rewards = new System.Collections.Generic.List<string>();

        if (reward.goldReward > 0)        rewards.Add($"Gold x{reward.goldReward}");
        if (reward.gemReward > 0)         rewards.Add($"Gems x{reward.gemReward}");
        if (reward.experienceReward > 0)  rewards.Add($"EXP x{reward.experienceReward}");

        if (reward.itemIDs != null)
        {
            foreach (var itemID in reward.itemIDs)
            {
                string itemName = itemID;
                ItemData item = CommonReferent.Instance?.itemDatabase?.GetItemByID(itemID);
                if (item != null) itemName = item.itemName;
                rewards.Add($"{itemName} x1");
            }
        }

        if (reward.rewardItem != null && reward.rewardItem.itemData != null)
            rewards.Add($"{reward.rewardItem.itemData.itemName} x1");

        string rewardText = string.Join(" & ", rewards);

        if (string.IsNullOrEmpty(questName))
            return rewards.Count == 0 ? "You completed the quest!" : $"Received {rewardText}!";
        else
            return rewards.Count == 0 ? $"Completed: {questName}" : $"Received {rewardText}!";
    }

    // ─── Button ────────────────────────────────────────────────────────────────
    private void OnConfirmClick()
    {
        UIManager.Instance.HidePopupByType(PopupType.QuestReward);
    }

    // ─── Hide / Anim ───────────────────────────────────────────────────────────
    public override void Hide()
    {
        StopAllCoroutines();
        StopBurstEffect();
        StartCoroutine(AnimateOut());
    }

    private IEnumerator AnimateIn()
    {
        transform.localScale = Vector3.zero;
        _canvasGroup.alpha   = 0f;

        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);
            transform.localScale = Vector3.one * EaseOutBack(t);
            _canvasGroup.alpha   = t;
            yield return null;
        }

        transform.localScale = Vector3.one;
        _canvasGroup.alpha   = 1f;
    }

    private IEnumerator AnimateOut()
    {
        float duration = animDuration * 0.6f;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.one * (1f - t);
            _canvasGroup.alpha   = 1f - t;
            yield return null;
        }

        transform.localScale = Vector3.one;
        _canvasGroup.alpha   = 1f;
        base.Hide();
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}