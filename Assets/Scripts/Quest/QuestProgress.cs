using System.Collections.Generic;

[System.Serializable]
public class QuestProgress
{
    public Quest quest;
    public QuestState state;
    public Dictionary<string, int> progress;

    public QuestProgress(Quest quest)
    {
        this.quest = quest;
        this.state = QuestState.NotAccepted;
        this.progress = new Dictionary<string, int>();
    }

    public bool IsCompleted()
    {
        if (quest == null || quest.objectives == null) return false;

        foreach (var obj in quest.objectives)
        {
            int current = progress.ContainsKey(obj.objectiveName) ? progress[obj.objectiveName] : 0;
            if (current < obj.requiredAmount)
                return false;
        }

        return true;
    }
}