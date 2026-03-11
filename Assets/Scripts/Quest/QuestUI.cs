using UnityEngine;
using TMPro;

public class QuestUI : MonoBehaviour
{
    public TextMeshProUGUI questProgressText;
    
    public void UpdateQuestProgress(QuestProgress qp, bool readyToTurnIn = false)
    {
        if (qp == null || qp.quest.objectives == null || qp.quest.objectives.Length == 0)
        {
            questProgressText.text = "không có nhiệm vụ";
            return;
        }

        string progress = $"<b>Quest :</b> {qp.quest.questName}\n";

        foreach (var obj in qp.quest.objectives)
        {
            int currentAmount = qp.progress.ContainsKey(obj.objectiveName) ? qp.progress[obj.objectiveName] : 0;
            progress += $"- {obj.objectiveName}: {currentAmount}/{obj.requiredAmount}\n";
        }

        // ===== REWARD =====
        if (qp.quest.reward != null)
        {
            progress += "\n<b><color=#4CAF50>Reward</color></b>\n";

            if (qp.quest.reward.experienceReward > 0)
                progress += $"<color=#C084FC>+{qp.quest.reward.experienceReward} EXP</color>";

            if (qp.quest.reward.gemReward > 0)
                progress += $"<color=#3BA4FF>+{qp.quest.reward.gemReward} Gem</color>";
            
            if (qp.quest.reward.goldReward > 0)
                progress += $"<color=#FFD700>+{qp.quest.reward.goldReward} Gold</color>\n";

            if (qp.quest.reward.itemIDs != null && qp.quest.reward.itemIDs.Count > 0)
            {
                foreach (var item in qp.quest.reward.itemIDs)
                {
                    progress += $"• {item}\n";
                }
            }
        }

        if (readyToTurnIn)
            progress += "\n<color=yellow>Đã hoàn thành! Hãy quay lại NPC để nhận thưởng.</color>";

        questProgressText.text = progress;
    }
    public void Clear() => questProgressText.text = "khong co nhiem vu";
}