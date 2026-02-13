using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePopup : BasePopup
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;

    private Queue<DialogueLine> lines;
    private Coroutine typingCoroutine;
    private string currentSentence;
    private bool isTyping;
    private System.Action onComplete;

    protected override bool ShouldDestroyOnHide() => false;

    private void Start()
    {
        nextButton.onClick.AddListener(OnNextClicked);
        skipButton.onClick.AddListener(EndDialogue);
    }

    public void ShowDialogue(Dialogue dialogue, System.Action onComplete)
    {
        this.onComplete = onComplete;
        lines = new Queue<DialogueLine>(dialogue.lines);

        Show();
        DisplayNextLine();
    }

    private void OnNextClicked()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentSentence;
            isTyping = false;
        }
        else
        {
            DisplayNextLine();
        }
    }

    private void DisplayNextLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        var line = lines.Dequeue();
        nameText.text = line.speakerName;
        currentSentence = line.sentence;

        typingCoroutine = StartCoroutine(TypeSentence(currentSentence));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        isTyping = true;

        foreach (char c in sentence)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.02f);
        }

        isTyping = false;
    }

    private void EndDialogue()
    {
        Hide();
        onComplete?.Invoke();
        onComplete = null;
    }
}