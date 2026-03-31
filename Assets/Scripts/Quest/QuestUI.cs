using UnityEngine;
using TMPro;

public class QuestUI : MonoBehaviour
{
    public TextMeshProUGUI questProgressText;

    public void UpdateQuestProgress(QuestProgress qp, bool readyToTurnIn = false)
    {
        // ===== NULL CHECK =====
        if (qp == null || qp.quest == null || qp.quest.objectives == null || qp.quest.objectives.Length == 0)
        {
            questProgressText.text =
                "<b><color=#FFD54F>Không có nhiệm vụ</color></b>\n" +
                "<color=#B0BEC5>Hãy tìm NPC để bắt đầu</color>";
            return;
        }

        string progress = "";

        // ===== TITLE =====
        progress += $"<b><color=#FFD54F>Nhiệm vụ:</color></b> {qp.quest.questName}\n";

        // ===== STATE =====
        switch (qp.state)
        {
            case QuestState.NotAccepted:
                progress += "<color=#B0BEC5>Chưa nhận nhiệm vụ</color>\n";
                break;

            case QuestState.InProgress:
                progress += "<color=#64B5F6>Đang thực hiện...</color>\n";
                break;

            case QuestState.Completed:
                progress += "<color=#81C784>Đã hoàn thành!</color>\n";
                break;

            case QuestState.Rewarded:
                progress += "<color=#B0BEC5>Đã nhận thưởng</color>\n";
                break;
        }

        // ===== OBJECTIVES =====
        progress += "\n";

        foreach (var obj in qp.quest.objectives)
        {
            int currentAmount = qp.progress.ContainsKey(obj.objectiveName)
                ? qp.progress[obj.objectiveName]
                : 0;

            bool done = currentAmount >= obj.requiredAmount;

            if (done)
            {
                progress += $"<color=#81C784>✔ {obj.objectiveName}: {currentAmount}/{obj.requiredAmount}</color>\n";
            }
            else
            {
                progress += $"- {obj.objectiveName}: {currentAmount}/{obj.requiredAmount}\n";
            }
        }

        // ===== REWARD =====
        if (qp.quest.reward != null)
        {
            progress += "\n<b><color=#4CAF50>Phần thưởng</color></b>\n";

            if (qp.quest.reward.experienceReward > 0)
                progress += $"<color=#C084FC>+{qp.quest.reward.experienceReward} EXP</color>\n";

            if (qp.quest.reward.gemReward > 0)
                progress += $"<color=#3BA4FF>+{qp.quest.reward.gemReward} Gem</color>\n";

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

        // ===== TURN IN =====
        if (readyToTurnIn || qp.state == QuestState.Completed)
        {
            progress += "\n<color=yellow>➡ Hãy quay lại NPC để nhận thưởng</color>";
        }

        questProgressText.text = progress;
    }

    public void Clear()
    {
        questProgressText.text =
            "<b><color=#FFD54F>Không có nhiệm vụ</color></b>\n" +
            "<color=#B0BEC5>Hãy tìm NPC để bắt đầu</color>";
    }
}