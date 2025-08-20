using UnityEngine;
using TMPro;

public class QuestUI : MonoBehaviour
{
    public TextMeshProUGUI questProgressText;

    public void UpdateQuestProgress(Quest quest)
    {
        if (quest == null || quest.objectives == null || quest.objectives.Length == 0)
        {
            questProgressText.text = "khong co nhiem vu";
            return;
        }

        string progress = $"<b>Quest :</b> {quest.questName}\n";
        
        foreach (var obj in quest.objectives)
        {
            int currentAmount = QuestManager.Instance.GetObjectiveProgress(quest.questID, obj.objectiveName);
            progress += $"- {obj.objectiveName}: {currentAmount}/{obj.requiredAmount}\n";
        }

        questProgressText.text = progress;
    }
    
    public void Clear()
    {
        questProgressText.text = "khong co nhiem vu";
    }

}