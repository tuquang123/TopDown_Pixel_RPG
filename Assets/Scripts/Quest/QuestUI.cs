using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUI : MonoBehaviour
{
    public TextMeshProUGUI questProgressText;
    [SerializeField] private Button claimButton;

    private QuestProgress currentQuest;

    private void Awake()
    {
        EnsureClaimButton();

        if (claimButton != null)
        {
            claimButton.onClick.RemoveListener(OnClickClaim);
            claimButton.onClick.AddListener(OnClickClaim);
            claimButton.gameObject.SetActive(false);
        }
    }

    private void EnsureClaimButton()
    {
        if (claimButton != null)
            return;

        var claimGO = new GameObject("ClaimButton", typeof(RectTransform), typeof(Image), typeof(Button));
        claimGO.transform.SetParent(transform, false);

        var rect = claimGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 8f);
        rect.sizeDelta = new Vector2(140f, 36f);

        var image = claimGO.GetComponent<Image>();
        image.color = new Color(0.2f, 0.65f, 0.2f, 0.95f);

        claimButton = claimGO.GetComponent<Button>();
        claimButton.targetGraphic = image;

        var textGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textGO.transform.SetParent(claimGO.transform, false);

        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var label = textGO.GetComponent<Text>();
        label.text = "Claim";
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    public void UpdateQuestProgress(QuestProgress qp, bool readyToTurnIn = false)
    {
        currentQuest = qp;

        if (qp == null || qp.quest == null || qp.quest.objectives == null)
        {
            Clear();
            return;
        }

        string text = $"<b>Quest:</b> {qp.quest.questName}\n";

        foreach (var obj in qp.quest.objectives)
        {
            int current = 0;

            if (qp.progress != null && qp.progress.ContainsKey(obj.objectiveName))
                current = qp.progress[obj.objectiveName];

            text += $"- {obj.objectiveName}: {current}/{obj.requiredAmount}\n";
        }

        if (qp.quest.reward != null)
        {
            text += "\n<b><color=#4CAF50>Reward</color></b>\n";

            if (qp.quest.reward.experienceReward > 0)
                text += $"<color=#C084FC>+{qp.quest.reward.experienceReward} EXP</color>\n";

            if (qp.quest.reward.goldReward > 0)
                text += $"<color=#FFD700>+{qp.quest.reward.goldReward} Gold</color>\n";

            if (qp.quest.reward.gemReward > 0)
                text += $"<color=#3BA4FF>+{qp.quest.reward.gemReward} Gem</color>\n";

            if (qp.quest.reward.itemIDs != null)
            {
                foreach (var item in qp.quest.reward.itemIDs)
                {
                    text += $"• {item}\n";
                }
            }
        }

        if (readyToTurnIn)
        {
            text += "\n<color=yellow>Hoàn thành! Nhấn Claim để nhận thưởng.</color>";
        }

        questProgressText.text = text;

        if (claimButton != null)
            claimButton.gameObject.SetActive(readyToTurnIn);
    }

    public void Clear()
    {
        currentQuest = null;
        questProgressText.text = "Không có nhiệm vụ\nHãy đi tìm NPC";

        if (claimButton != null)
            claimButton.gameObject.SetActive(false);
    }

    private void OnClickClaim()
    {
        if (currentQuest == null || currentQuest.state != QuestState.Completed)
            return;

        QuestManager.Instance?.TurnInQuest(currentQuest);
    }
}
