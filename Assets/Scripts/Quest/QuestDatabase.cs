using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Quest/Database", order = 1)]
public class QuestDatabase : ScriptableObject
{
    public List<Quest> allQuests;

    public Quest GetQuestByID(string questID)
    {
        return allQuests.Find(quest => quest.questID == questID);
    }
}