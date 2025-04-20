using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dialogue/Database")]
public class DialogueDatabase : ScriptableObject
{
    public DialogueEntry[] entries;

    private Dictionary<string, Dialogue> lookup;

    public Dialogue GetDialogueByID(string id)
    {
        if (lookup == null)
        {
            lookup = new Dictionary<string, Dialogue>();
            foreach (var entry in entries)
            {
                if (!lookup.ContainsKey(entry.id))
                    lookup.Add(entry.id, entry.dialogue);
            }
        }

        if (lookup.TryGetValue(id, out var dialogue))
            return dialogue;

        Debug.LogWarning($"Dialogue ID not found: {id}");
        return null;
    }
}

[System.Serializable]
public class DialogueEntry
{
    public string id;
    public Dialogue dialogue;
}