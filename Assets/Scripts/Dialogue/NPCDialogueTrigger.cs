using UnityEngine;
using UnityEngine.UI;

public class NPCDialogueTrigger : MonoBehaviour
{
    [SerializeField] string dialogueID;
    [SerializeField] string questID;   // gắn quest phát ra từ NPC này
    
    private Vector3 offset = new Vector3(0, .85f, 0);
    private Button interactButton;
    private Camera mainCam;
    private GameObject targetNPC;

    private void Awake()
    {
        interactButton = CommonReferent.Instance.dialogBtn.GetComponent<Button>();
        if (interactButton != null)
            interactButton.gameObject.SetActive(false);

        mainCam = Camera.main;
    }

    private void Start()
    {
        SetTarget(gameObject, dialogueID);
    }

    public void SetTarget(GameObject npc, string dialogueID)
    {
        targetNPC = npc;
        this.dialogueID = dialogueID;

        if (interactButton != null)
        {
            interactButton.onClick.RemoveAllListeners();
            interactButton.onClick.AddListener(StartDialogue);
        }
    }

    public void ShowUI() => interactButton?.gameObject.SetActive(true);
    public void HideUI() => interactButton?.gameObject.SetActive(false);

    private void StartDialogue()
    {
        DialogueSystem.Instance.StartDialogueByID(dialogueID, OnDialogueComplete);
        HideUI();
    }

    /// <summary>
    /// Callback khi thoại kết thúc
    /// </summary>
    private void OnDialogueComplete()
    {
        if (!string.IsNullOrEmpty(questID))
        {
            var quest = QuestManager.Instance.questDatabase.GetQuestByID(questID);
            if (quest != null)
            {
                QuestManager.Instance.StartQuest(quest);
                Debug.Log($"Player nhận nhiệm vụ: {questID}");
            }
        }
    }

    private void LateUpdate()
    {
        if (targetNPC == null || interactButton == null || !interactButton.gameObject.activeSelf)
            return;

        Vector3 worldPos = targetNPC.transform.position + offset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
        interactButton.transform.position = screenPos;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            ShowUI();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            HideUI();
    }
}
