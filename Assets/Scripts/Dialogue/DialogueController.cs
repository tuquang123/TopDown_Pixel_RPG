using UnityEngine;

public class DialogueController : Singleton<DialogueController>
{
    [SerializeField] private DialoguePopup popup;
    [SerializeField] private DialogueDatabase database;

    public void StartDialogue(string npcId, string questId, QuestState state, System.Action onComplete = null)
    {
        var dialogue = database.GetDialogue(npcId, questId, state);

        if (dialogue == null)
        {
            Debug.LogWarning("Dialogue not found");
            return;
        }

        popup.ShowDialogue(dialogue, onComplete);
    }
}