using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    public TextMeshProUGUI questProgressText;

    public void UpdateQuestProgress(Quest quest)
    {
        if (quest == null || quest.objectives == null || quest.objectives.Length == 0)
        {
            questProgressText.text = "";
            return;
        }

        string progress = $"<b>Nhiệm vụ:</b> {quest.questName}\n";

        // Sử dụng progressTracker để lấy tiến độ
        foreach (var obj in quest.objectives)
        {
            // Kiểm tra xem tiến độ của mục tiêu này đã được lưu trong progressTracker chưa
            int currentAmount = QuestManager.Instance.GetObjectiveProgress(quest.questID, obj.objectiveName);
            progress += $"- {obj.objectiveName}: {currentAmount}/{obj.requiredAmount}\n";
        }

        questProgressText.text = progress;
    }
}