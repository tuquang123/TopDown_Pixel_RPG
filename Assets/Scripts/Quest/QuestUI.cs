using UnityEngine;
using TMPro;

public class QuestUI : MonoBehaviour
{
    public TextMeshProUGUI questProgressText;

    public void UpdateQuestProgress(QuestProgress qp, bool readyToTurnIn = false)
    {
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

        // ===== REWARD =====
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
            text += "\n<color=yellow>Hoàn thành! Quay lại NPC.</color>";
        }

        questProgressText.text = text;
    }

    public void Clear()
    {
        questProgressText.text = "Không có nhiệm vụ\nHãy đi tìm NPC";
    }
}