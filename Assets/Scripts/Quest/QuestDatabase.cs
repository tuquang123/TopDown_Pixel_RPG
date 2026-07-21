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

    [Tooltip("Localization key for questName. Example: quest.kill_slimes.name")]
    public string questNameKey;
    public string questName;

    [Tooltip("Localization key for description. Example: quest.kill_slimes.description")]
    public string descriptionKey;
    public string description;

    public QuestObjective[] objectives;
    public QuestReward reward;
    public string turnInNPCName;

    public string GetDisplayName()
    {
        string key = !string.IsNullOrWhiteSpace(questNameKey)
            ? questNameKey
            : BuildQuestKey("name");

        return Localize(key, questName);
    }

    public string GetDisplayDescription()
    {
        string key = !string.IsNullOrWhiteSpace(descriptionKey)
            ? descriptionKey
            : BuildQuestKey("description");

        return Localize(key, description);
    }

    public string GetObjectiveDisplayName(QuestObjective objective)
    {
        if (objective == null)
            return string.Empty;

        return objective.GetDisplayName(questID);
    }

    private string BuildQuestKey(string suffix)
    {
        return string.IsNullOrWhiteSpace(questID) ? string.Empty : $"quest.{questID}.{suffix}";
    }

    private static string Localize(string key, string fallback)
    {
        return LanguageManager.Instance != null
            ? LanguageManager.Instance.GetTranslationOrFallback(key, fallback)
            : fallback;
    }
}

[System.Serializable]
public class QuestObjective
{
    [Tooltip("Logic/progress ID. Keep this stable; do not use translated text here.")]
    public string objectiveName;

    [Tooltip("Localization key for display text. Example: quest.kill_slimes.objective.kill")]
    public string objectiveTextKey;

    public ObjectiveType type;
    public int requiredAmount;
    [SerializeField] public string targetID;
    public string TargetID => targetID;

    public string GetDisplayName(string questID)
    {
        string key = !string.IsNullOrWhiteSpace(objectiveTextKey)
            ? objectiveTextKey
            : BuildObjectiveKey(questID);

        return LanguageManager.Instance != null
            ? LanguageManager.Instance.GetTranslationOrFallback(key, objectiveName)
            : objectiveName;
    }

    private string BuildObjectiveKey(string questID)
    {
        if (string.IsNullOrWhiteSpace(questID) || string.IsNullOrWhiteSpace(objectiveName))
            return string.Empty;

        return $"quest.{questID}.objective.{objectiveName}";
    }
}

[System.Serializable]
public class QuestReward
{
    public int experienceReward;
    public int goldReward;
    public int gemReward;
    public List<string> itemIDs;
    public ItemInstance rewardItem; // item nhận được, để null nếu không có
}

public enum ObjectiveType
{
    KillEnemies,
    CollectItems,
    TalkToNPC,
    ExploreArea,
    Custom,
    DestroyObject,
    EnterZone,
    UseItem,
    LevelUp
}
