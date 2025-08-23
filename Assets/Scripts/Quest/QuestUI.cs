using UnityEngine;
using TMPro;

public class QuestUI : MonoBehaviour
{
    public TextMeshProUGUI questProgressText;

    public void UpdateQuestProgress(QuestProgress qp, bool readyToTurnIn = false)
    {
        if (qp == null || qp.quest.objectives == null || qp.quest.objectives.Length == 0)
        {
            questProgressText.text = "khong co nhiem vu";
            return;
        }

        string progress = $"<b>Quest :</b> {qp.quest.questName}\n";
        foreach (var obj in qp.quest.objectives)
        {
            int currentAmount = qp.progress.ContainsKey(obj.objectiveName) ? qp.progress[obj.objectiveName] : 0;
            progress += $"- {obj.objectiveName}: {currentAmount}/{obj.requiredAmount}\n";
        }

        if (readyToTurnIn)
            progress += "\n<color=yellow>Đã hoàn thành! Hãy quay lại NPC để nhận thưởng.</color>";

        questProgressText.text = progress;
    }

    public void Clear() => questProgressText.text = "khong co nhiem vu";
}