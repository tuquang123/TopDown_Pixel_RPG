using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "Dialogue/Database")]
public class DialogueDatabase : ScriptableObject
{
    public DialogueEntry[] entries;

    private Dictionary<string, DialogueEntry> lookup;

    private string GetKey(string npcName, string questId) => $"{npcName}_{questId}";

    public Dialogue GetDialogue(string npcName, string questId, QuestState state)
    {
        lookup = new Dictionary<string, DialogueEntry>();
        foreach (var entry in entries)
        {
            string key = GetKey(entry.npcName, entry.questId);
            if (!lookup.ContainsKey(key))
                lookup.Add(key, entry);
        }

        string keyLookup = GetKey(npcName, questId);
        if (lookup.TryGetValue(keyLookup, out var entryValue))
            return entryValue.GetDialogue(state);

        Debug.LogWarning($"Dialogue not found for NPC:{npcName}, Quest:{questId}, State:{state}");
        return null;
    }

}


[System.Serializable]
public class NPCDialogueEntry
{
    public List<Dialogue> dialogues;

    public Dialogue GetDialogue(string key)
    {
        return dialogues.FirstOrDefault(d => d.id == key);
    }
}

[System.Serializable]
public class DialogueEntry
{
    public string npcName;   // VD: "FarmerNPC"
    public string questId;   // VD: "Quest001"

    [Header("Dialogue Variants")]
    public Dialogue notAcceptedDialogue;
    public Dialogue inProgressDialogue;
    public Dialogue completedDialogue;
    public Dialogue rewardedDialogue;

    public Dialogue GetDialogue(QuestState state)
    {
        return state switch
        {
            QuestState.NotAccepted => notAcceptedDialogue,
            QuestState.InProgress  => inProgressDialogue,
            QuestState.Completed   => completedDialogue,
            QuestState.Rewarded    => rewardedDialogue,
            _ => null
        };
    }
}