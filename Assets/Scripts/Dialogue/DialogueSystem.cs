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
    private string currentFullSentence = "";
    private System.Action onComplete;

    private void Start()
    {
        dialoguePanel.SetActive(false);
        nextButton.onClick.AddListener(OnNextClicked);
        skipButton.onClick.AddListener(EndDialogue);
    }

    // ===== API chính gọi từ NPC =====
    public void StartDialogueForQuest(string npcId, string questId, QuestState state, System.Action onComplete = null)
    {
        // Lấy dialogue trực tiếp từ database theo NPC + quest + state
        Dialogue dialogue = dialogueDatabase.GetDialogue(npcId, questId, state);

        if (dialogue == null)
        {
            Debug.LogWarning($"Dialogue not found for NPC:{npcId}, Quest:{questId}, State:{state}");
            return;
        }

        // Gán callback và bắt đầu dialogue
        this.onComplete = onComplete;
        StartDialogue(dialogue);
    }


    
    // ===== Nội bộ xử lý thoại =====
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

    private void DisplayNextLine()
    {
        if (lines == null || lines.Count == 0)
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
        onComplete?.Invoke();
        onComplete = null;
    }
}

// ===== Thêm enum trạng thái quest =====
public enum QuestState
{
    NotAccepted,  // chưa nhận
    InProgress,   // đang làm
    Completed,    // xong nhiệm vụ
    Rewarded      // đã nhận thưởng
}
