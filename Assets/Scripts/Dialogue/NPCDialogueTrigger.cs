// ================= NPCDialogueTrigger.cs =================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCDialogueTrigger : MonoBehaviour
{
    [SerializeField] string dialogueID;
    [SerializeField] List<string> questIDs;   // quests chain from this NPC
    
    private Vector3 offset = new Vector3(0, .85f, 0); // button position offset
    private Button interactButton;
    private Camera mainCam;
    private GameObject targetNPC;
    
    private QuestProgress currentQuest; // current quest in chain
    private string nameOBj = "NPC_0";   // objective name for this NPC

    private void Awake()
    {
        // reference to interact button in UI
        interactButton = CommonReferent.Instance.dialogBtn.GetComponent<Button>();
        if (interactButton != null)
            interactButton.gameObject.SetActive(false);

        mainCam = Camera.main;
    }

    private void Start()
    {
        // set this NPC as target and update current quest
        SetTarget(gameObject, dialogueID);
        UpdateCurrentQuest();
    }

    /// <summary>
    /// Find first quest in chain that is not rewarded yet
    /// If not created, create new QuestProgress
    /// </summary>
    private void UpdateCurrentQuest()
    {
        if (questIDs == null || questIDs.Count == 0) return;

        for (int i = 0; i < questIDs.Count; i++)
        {
            var qp = QuestManager.Instance.GetQuestProgressByID(questIDs[i]);

            if (qp == null) // create progress if not exist
            {
                var questSO = QuestManager.Instance.questDatabase.GetQuestByID(questIDs[i]);
                if (questSO != null)
                {
                    qp = QuestManager.Instance.CreateQuestProgress(questSO);
                }
            }

            // stop at first quest not rewarded yet
            if (qp.state != QuestState.Rewarded)
            {
                currentQuest = qp;
                return;
            }
        }

        currentQuest = null; // no quest left
    }

    /// <summary>
    /// Start dialogue + quest flow with NPC
    /// </summary>
    private void StartDialogue()
    {
        // check progress objective (talk to NPC)
        if (currentQuest != null && currentQuest.state == QuestState.InProgress)
        {
            foreach (var obj in currentQuest.quest.objectives)
            {
                if (obj.objectiveName == nameOBj)
                {
                    QuestManager.Instance.ReportProgress(currentQuest.quest.questID, obj.objectiveName);
                    break;
                }
            }
        }

        if (currentQuest == null)
        {
            // Không còn quest nào → thoại mặc định “thanks”
            DialogueSystem.Instance.StartDialogueForQuest(dialogueID, "NV0", QuestState.Rewarded);
            return;
        }

        // start dialogue với quest hiện tại
        DialogueSystem.Instance.StartDialogueForQuest(dialogueID, currentQuest.quest.questID, currentQuest.state, () =>
        {
            if (currentQuest.state == QuestState.NotAccepted)
            {
                // accept quest
                QuestManager.Instance.StartQuest(currentQuest.quest);
                currentQuest.state = QuestState.InProgress;
                QuestManager.Instance.questUI?.UpdateQuestProgress(currentQuest);
            }
            else if (currentQuest.state == QuestState.Completed)
            {
                // turn in quest và mở quest kế tiếp
                QuestManager.Instance.TurnInQuest(currentQuest);
                UpdateCurrentQuest();
            }
        });

        HideUI();
    }


    /// <summary>
    /// Link this NPC with dialogue button
    /// </summary>
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

    private void LateUpdate()
    {
        // move button above NPC in screen space
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

    public void ShowUI() => interactButton?.gameObject.SetActive(true);
    public void HideUI() => interactButton?.gameObject.SetActive(false);
}
