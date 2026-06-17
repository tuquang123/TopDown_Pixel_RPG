using System.Collections;
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

    protected override void Awake()
    {
        base.Awake();

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(OnConfirmClick);
            confirmButton.onClick.AddListener(OnConfirmClick);
        }
    }

    public void ShowReward(QuestReward reward, string questName = "")
    {
        if (reward == null) return;

        if (titleText != null)
            titleText.text = "Congratulations!";

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
    }
    private string GetRewardSubtitle(QuestReward reward, string questName)
    {
        // Táº¡o danh sĂ¡ch pháº§n thÆ°á»Ÿng
        var rewards = new System.Collections.Generic.List<string>();
        
        if (reward.goldReward > 0)
            rewards.Add($"Gold x{reward.goldReward}");

        if (reward.gemReward > 0)
            rewards.Add($"Gems x{reward.gemReward}");

        if (reward.experienceReward > 0)
            rewards.Add($"EXP x{reward.experienceReward}");

        if (reward.itemIDs != null)
        {
            foreach (var itemID in reward.itemIDs)
            {
                string itemName = itemID;
                ItemData item = CommonReferent.Instance?.itemDatabase?.GetItemByID(itemID);
                if (item != null)
                    itemName = item.itemName;

                rewards.Add($"{itemName} x1");
            }
        }

        if (reward.rewardItem != null && reward.rewardItem.itemData != null)
            rewards.Add($"{reward.rewardItem.itemData.itemName} x1");

        string rewardText = string.Join(" & ", rewards);

        if (string.IsNullOrEmpty(questName))
        {
            if (rewards.Count == 0)
                return "You completed the quest!";
            else if (rewards.Count == 1)
                return $"Received {rewardText}!";
            else
                return $"Received {rewardText}!";
        }
        else
        {
            if (rewards.Count == 0)
                return $"Completed: {questName}";
            else if (rewards.Count == 1)
                return $"Received {rewardText}!";
            else
                return $"Received {rewardText}!";
        }
    }

    private void OnConfirmClick()
    {
        UIManager.Instance.HidePopupByType(PopupType.QuestReward);
    }

    public override void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateOut());
    }

    private IEnumerator AnimateIn()
    {
        transform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);
            transform.localScale = Vector3.one * EaseOutBack(t);
            yield return null;
        }

        transform.localScale = Vector3.one;
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
            yield return null;
        }

        transform.localScale = Vector3.one;
        base.Hide();
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
