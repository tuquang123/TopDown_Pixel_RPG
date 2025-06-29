using System.Collections.Generic;
using UnityEngine;

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
    [HideInInspector] public List<ItemData> itemsReward;
}

[System.Serializable]
public class Item
{
    public string itemName;
    public Sprite itemIcon;
}

public enum ObjectiveType
{
    KillEnemies,
    CollectItems,
    TalkToNPC,
    ExploreArea,
    Custom
}
