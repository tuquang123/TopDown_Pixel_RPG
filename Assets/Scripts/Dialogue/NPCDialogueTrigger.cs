using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue & Quest")]
    [SerializeField] string dialogueID;
    [SerializeField] List<string> questIDs;
    [SerializeField] string nameObj = "NPC_0";

    [Header("Interaction Settings")]
    [SerializeField] float interactRange = 2.5f; // phạm vi nói chuyện
    [SerializeField] Vector3 offset = new Vector3(0, 0.9f, 0);

    private Button interactButton;
    private Camera mainCam;
    private GameObject targetNPC;
    private Transform player;

    private QuestProgress currentQuest;
    private bool isPlayerInRange;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void Start()
    {
        player = CommonReferent.Instance.playerPrefab.transform;
        interactButton = CommonReferent.Instance.dialogBtn.GetComponent<Button>();
        if (interactButton != null)
            interactButton.gameObject.SetActive(false);

        SetTarget(gameObject, dialogueID);
        UpdateCurrentQuest();

        // start first quest (nếu cần)
        if (currentQuest != null)
        {
            QuestManager.Instance.StartQuest(currentQuest.quest);
            currentQuest.state = QuestState.InProgress;
            QuestManager.Instance.questUI.UpdateQuestProgress(currentQuest);
            Debug.Log("startQuest " + currentQuest.quest.questName);
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool inRange = distance <= interactRange;

        if (inRange && !isPlayerInRange)
        {
            // Player vừa vào phạm vi
            ShowUI();
            isPlayerInRange = true;
        }
        else if (!inRange && isPlayerInRange)
        {
            // Player vừa rời phạm vi
            HideUI();
            isPlayerInRange = false;
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

    private void UpdateCurrentQuest()
    {
        if (questIDs == null || questIDs.Count == 0) return;

        foreach (var id in questIDs)
        {
            var qp = QuestManager.Instance.GetQuestProgressByID(id);
            if (qp == null)
            {
                var questSO = QuestManager.Instance.questDatabase.GetQuestByID(id);
                if (questSO != null)
                    qp = QuestManager.Instance.CreateQuestProgress(questSO);
            }

            if (qp.state != QuestState.Rewarded)
            {
                currentQuest = qp;
                return;
            }
        }

        currentQuest = null;
    }

    private void StartDialogue()
    {
        if (currentQuest != null && currentQuest.state == QuestState.InProgress)
        {
            foreach (var obj in currentQuest.quest.objectives)
            {
                if (obj.objectiveName == nameObj)
                {
                    QuestManager.Instance.ReportProgress(currentQuest.quest.questID, obj.objectiveName);
                    break;
                }
            }
        }

        if (currentQuest == null)
        {
            DialogueSystem.Instance.StartDialogueForQuest(dialogueID, "NV0", QuestState.Rewarded);
            return;
        }

        DialogueSystem.Instance.StartDialogueForQuest(dialogueID, currentQuest.quest.questID, currentQuest.state, () =>
        {
            if (currentQuest.state == QuestState.NotAccepted)
            {
                QuestManager.Instance.StartQuest(currentQuest.quest);
                currentQuest.state = QuestState.InProgress;
                QuestManager.Instance.questUI?.UpdateQuestProgress(currentQuest);
            }
            else if (currentQuest.state == QuestState.Completed)
            {
                QuestManager.Instance.TurnInQuest(currentQuest);
                UpdateCurrentQuest();
            }
        });
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

    private void ShowUI() => interactButton?.gameObject.SetActive(true);
    private void HideUI() => interactButton?.gameObject.SetActive(false);
}
