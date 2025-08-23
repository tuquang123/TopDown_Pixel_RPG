using UnityEngine;

public class DialogueTrigger : MonoBehaviour {

    public void TriggerDialogue() 
    {
        //DialogueSystem.Instance.StartDialogueByID("tulq");
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerDialogue();
        }
    }
}