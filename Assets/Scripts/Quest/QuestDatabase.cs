using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Quest/Database", order = 1)]
public class QuestDatabase : ScriptableObject
{
    public List<Quest> allQuests;

    public Quest GetQuestByID(string questID)
    {
        return allQuests.Find(q => q.questID == questID);
    }
}

[System.Serializable]
public class Quest
{
    public string questID;
    public string questName;
    public string description;
    public QuestObjective[] objectives;
    public QuestReward reward;
}

[System.Serializable]
public class QuestObjective
{
    public string objectiveName;
    public ObjectiveType type;
    public int requiredAmount;
}

[System.Serializable]
public class QuestReward
{
    public int experienceReward;
    public int goldReward;
    public List<string> itemIDs;
}

public enum ObjectiveType
{
    KillEnemies,
    CollectItems,
    TalkToNPC,
    ExploreArea,
    Custom
}

