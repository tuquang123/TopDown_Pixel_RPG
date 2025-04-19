using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    public void TriggerQuest()
    {
        var quest = QuestManager.Instance.questDatabase.GetQuestByID("nv1");
        if (quest != null)
        {
            QuestManager.Instance.StartQuest(quest);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TriggerQuest();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            QuestManager.Instance.ReportProgress(ObjectiveType.KillEnemies, "jack", 1);
        }
    }
   
}