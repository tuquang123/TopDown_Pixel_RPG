using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;



public class DialogueSystem : Singleton<DialogueSystem>
{
    [Header("UI Components")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;
    public Button nextButton;
    public Button skipButton;
    public CanvasGroup canvasGroup; 
    private Tween openTween;
    private Tween closeTween;
    [Header("Database")] 
    public DialogueDatabase dialogueDatabase;

    private Queue<DialogueLine> lines;
    private bool isTyping = false;
    private string currentFullSentence = "";
    private System.Action onComplete;
    private Coroutine typingCoroutine;

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

        // reset
        openTween?.Kill();
        closeTween?.Kill();

        canvasGroup.alpha = 0f;
        dialoguePanel.transform.localScale = Vector3.one * 0.9f;

        openTween = DOTween.Sequence()
            .Append(canvasGroup.DOFade(1f, 0.2f))
            .Join(dialoguePanel.transform
                .DOScale(1f, 0.25f)
                .SetEase(Ease.OutBack));

        lines = new Queue<DialogueLine>(dialogue.lines);
        DisplayNextLine();
    }


    private void OnNextClicked()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
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

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeSentence(line.sentence));
    }


    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        isTyping = true;

        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.02f);
        }

        isTyping = false;
    }


    private void EndDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        openTween?.Kill();
        closeTween?.Kill();

        closeTween = DOTween.Sequence()
            .Append(canvasGroup.DOFade(0f, 0.15f))
            .Join(dialoguePanel.transform
                .DOScale(0.9f, 0.15f))
            .OnComplete(() =>
            {
                dialoguePanel.SetActive(false);
                onComplete?.Invoke();
                onComplete = null;
            });
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
