using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueSystem : Singleton<DialogueSystem>
{
    [Header("UI Components")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;
    public Button nextButton;
    public Button skipButton;

    [Header("Database")]
    public DialogueDatabase dialogueDatabase;

    private Queue<DialogueLine> lines;
    private bool isTyping = false;
    
    private void Start()
    {
        dialoguePanel.SetActive(false); 
        nextButton.onClick.AddListener(DisplayNextLine);
        skipButton.onClick.AddListener(EndDialogue);
    }

    public void StartDialogueByID(string id)
    {
        var dialogue = dialogueDatabase.GetDialogueByID(id);
        if (dialogue != null)
        {
            StartDialogue(dialogue);
        }
    }

    private void StartDialogue(Dialogue dialogue)
    {
        dialoguePanel.SetActive(true);
        lines = new Queue<DialogueLine>(dialogue.lines);
        DisplayNextLine();
    }

    private void OnNextClicked()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentFullSentence;
            isTyping = false;
        }
        else
        {
            DisplayNextLine();
        }
    }

    private string currentFullSentence = "";

    private void DisplayNextLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        var line = lines.Dequeue();
        nameText.text = line.speakerName;
        currentFullSentence = line.sentence;

        StopAllCoroutines();
        StartCoroutine(TypeSentence(line.sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        isTyping = true;

        foreach (var letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.02f);
        }

        isTyping = false;
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
    }
}
