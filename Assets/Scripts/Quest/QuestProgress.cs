using System.Collections.Generic;

[System.Serializable]
public class QuestProgress
{
    public Quest quest;                    
    public QuestState state = QuestState.NotAccepted;
    public Dictionary<string,int> progress = new Dictionary<string,int>();

    public QuestProgress(Quest quest)
    {
        this.quest = quest;
        foreach (var obj in quest.objectives)
            progress[obj.objectiveName] = 0;
    }

    public bool IsCompleted()
    {
        foreach (var obj in quest.objectives)
        {
            if (!progress.ContainsKey(obj.objectiveName) || progress[obj.objectiveName] < obj.requiredAmount)
                return false;
        }
        return true;
    }
}


