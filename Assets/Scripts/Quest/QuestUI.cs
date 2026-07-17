using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUI : MonoBehaviour
{
    public TextMeshProUGUI questProgressText;
    [SerializeField] private Button claimButton;

    private QuestProgress currentQuest;
    private bool currentReadyToTurnIn;

    private void Awake()
    {
        EnsureClaimButton();

        if (claimButton != null)
        {
            claimButton.onClick.RemoveListener(OnClickClaim);
            claimButton.onClick.AddListener(OnClickClaim);
            claimButton.gameObject.SetActive(false);
        }

        Clear();
    }

    private void OnEnable()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.OnLanguageChanged += RefreshLanguage;
    }

    private void OnDisable()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.OnLanguageChanged -= RefreshLanguage;
    }

    private void RefreshLanguage()
    {
        RefreshClaimButtonLabel();

        if (currentQuest != null)
            UpdateQuestProgress(currentQuest, currentReadyToTurnIn);
        else
            Clear();
    }

    private void EnsureClaimButton()
    {
        if (claimButton != null)
            return;

        var claimGO = new GameObject("ClaimButton", typeof(RectTransform), typeof(Image), typeof(Button));
        claimGO.transform.SetParent(transform, false);

        var rect              = claimGO.GetComponent<RectTransform>();
        rect.anchorMin        = new Vector2(0.5f, 0f);
        rect.anchorMax        = new Vector2(0.5f, 0f);
        rect.pivot            = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 8f);
        rect.sizeDelta        = new Vector2(140f, 36f);

        var image   = claimGO.GetComponent<Image>();
        image.color = new Color(0.2f, 0.65f, 0.2f, 0.95f);

        claimButton               = claimGO.GetComponent<Button>();
        claimButton.targetGraphic = image;

        var textGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textGO.transform.SetParent(claimGO.transform, false);

        var textRect       = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var label       = textGO.GetComponent<Text>();
        label.text      = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("claim") : "Claim";
        label.alignment = TextAnchor.MiddleCenter;
        label.color     = Color.white;
        label.font      = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    public void UpdateQuestProgress(QuestProgress qp, bool readyToTurnIn = false)
    {
        currentQuest = qp;
        currentReadyToTurnIn = readyToTurnIn;

        if (qp == null || qp.quest == null || qp.quest.objectives == null)
        {
            Clear();
            return;
        }

        string questLabel = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("quest") : "Quest";
        string rewardLabel = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("reward") : "Reward";
        string completedLabel = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("completed") : "Completed";
        string claimLabel = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("claim") : "Claim";
        string receivedLabel = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("received") : "Received";
        string goldLabel = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("gold") : "Gold";
        string gemLabel = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("gem") : "Gem";

        string text = $"<b>{questLabel}:</b> {TranslateRuntimeText(qp.quest.questName)}\n";

        foreach (var obj in qp.quest.objectives)
        {
            if (obj == null || string.IsNullOrEmpty(obj.objectiveName))
                continue;

            int current = 0;

            if (qp.progress != null && qp.progress.ContainsKey(obj.objectiveName))
                current = qp.progress[obj.objectiveName];

            text += $"- {TranslateRuntimeText(obj.objectiveName)}: {current}/{obj.requiredAmount}\n";
        }

        if (qp.quest.reward != null)
        {
            text += $"\n<b><color=#4CAF50>{rewardLabel}</color></b>\n";

            if (qp.quest.reward.experienceReward > 0)
                text += $"<color=#C084FC>+{qp.quest.reward.experienceReward} EXP</color>\n";

            if (qp.quest.reward.goldReward > 0)
                text += $"<color=#FFD700>+{qp.quest.reward.goldReward} {goldLabel}</color>\n";

            if (qp.quest.reward.gemReward > 0)
                text += $"<color=#3BA4FF>+{qp.quest.reward.gemReward} {gemLabel}</color>\n";

            if (qp.quest.reward.itemIDs != null)
            {
                foreach (var item in qp.quest.reward.itemIDs)
                    text += $"- {TranslateRuntimeText(item)}\n";
            }


            if (qp.quest.reward.rewardItem != null && qp.quest.reward.rewardItem.itemData != null)
                text += $"- {TranslateRuntimeText(qp.quest.reward.rewardItem.itemData.itemName)}\n";
        }

        if (readyToTurnIn)
            text += $"\n<color=yellow>{completedLabel}! {claimLabel}: {receivedLabel} {rewardLabel}</color>";

        if (questProgressText != null)
            questProgressText.text = text;

        RefreshClaimButtonLabel();

        if (claimButton != null)
            claimButton.gameObject.SetActive(readyToTurnIn);
    }

    public void Clear()
    {
        currentQuest = null;
        currentReadyToTurnIn = false;
        if (questProgressText != null)
        {
            questProgressText.text = LanguageManager.Instance != null && LanguageManager.Instance.CurrentLanguage == "vi"
                ? "Ch\u01b0a c\u00f3 nhi\u1ec7m v\u1ee5\nH\u00e3y t\u00ecm NPC \u0111\u1ec3 nh\u1eadn nhi\u1ec7m v\u1ee5"
                : "No active quest\nFind an NPC for a quest";
        }

        if (claimButton != null)
            claimButton.gameObject.SetActive(false);
    }

    private void OnClickClaim()
    {
        if (currentQuest == null || currentQuest.state != QuestState.Completed)
            return;

        if (currentQuest.quest?.reward != null)
        {
            UIManager.Instance.ShowQuestRewardPopup(
                currentQuest.quest.reward,
                currentQuest.quest.questName
            );
        }

        QuestManager.Instance?.TurnInQuest(currentQuest);

        if (claimButton != null)
            claimButton.gameObject.SetActive(false);
    }

    private void RefreshClaimButtonLabel()
    {
        if (claimButton == null)
            return;

        string claimLabel = LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation("claim") : "Claim";

        var tmpLabel = claimButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmpLabel != null)
            tmpLabel.text = claimLabel;

        var legacyLabel = claimButton.GetComponentInChildren<Text>(true);
        if (legacyLabel != null)
            legacyLabel.text = claimLabel;
    }

    private string TranslateRuntimeText(string value)
    {
        return LanguageManager.Instance != null ? LanguageManager.Instance.TranslateText(value) : value;
    }
}
