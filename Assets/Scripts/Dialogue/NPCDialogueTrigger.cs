using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue & Quest")]
    [SerializeField] string dialogueID;
    [SerializeField] List<string> questIDs;
    [SerializeField] public string nameObj = "NPC_0";

    [Header("Interaction Settings")]
    [SerializeField] float interactRange = 2.5f; // phạm vi nói chuyện
    [SerializeField] Vector3 offset = new Vector3(0, 0.9f, 0);

    private Button interactButton;
    private Camera mainCam;
    private GameObject targetNPC;
    private Transform player;

    private QuestProgress currentQuest;
    private bool isPlayerInRange;

    public GameObject questAvailableIcon;
    public GameObject questTurnInIcon;
    private void Awake()
    {
        mainCam = Camera.main;
    }

    public void Start()
    {
        // Tìm player instance thực tế (KHÔNG dùng prefab)
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"[NPCDialogueTrigger] Player found: {player.name} at {player.position}");
        }
        else
        {
            Debug.LogError("[NPCDialogueTrigger] Không tìm thấy Player trong scene! Kiểm tra tag 'Player'.");
            return; // thoát sớm nếu không có player
        }

        // Lấy interact button
        interactButton = CommonReferent.Instance.dialogBtn.GetComponent<Button>();
        if (interactButton != null)
        {
            interactButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[NPCDialogueTrigger] Không tìm thấy dialogBtn trong CommonReferent!");
        }

        SetTarget(gameObject, dialogueID);
        UpdateCurrentQuest();

        // Nếu NPC này có quest cần start tự động khi gặp lần đầu (thường không cần ở đây nữa)
        // Ví dụ: chỉ start nếu quest NotAccepted và player lần đầu gặp
        // Nhưng tốt nhất để player tự tương tác → không auto start ở đây
    }

    private void Update()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            if (player == null) return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        bool inRange = distance <= interactRange;

        if (inRange && !isPlayerInRange)
        {
            ShowUI();
            isPlayerInRange = true;
        }
        else if (!inRange && isPlayerInRange)
        {
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

    public void UpdateCurrentQuest()
    {
        if (questIDs == null || questIDs.Count == 0)
        {
            questAvailableIcon.SetActive(false);
            questTurnInIcon.SetActive(false);
            return;
        }

        foreach (var id in questIDs)
        {
            var qp = QuestManager.Instance.GetQuestProgressByID(id);

            if (qp != null && qp.state != QuestState.Rewarded)
            {
                currentQuest = qp;
                questAvailableIcon.SetActive(qp.state == QuestState.NotAccepted);
                questTurnInIcon.SetActive(qp.state == QuestState.Completed);
                

                return;
            }
        }

        currentQuest = null;
        questAvailableIcon.SetActive(false);
        questTurnInIcon.SetActive(false);
    }
    private void StartDialogue()
    {
        // 🔥 LẤY QUEST THẬT
        QuestProgress realQuest = null;

        if (currentQuest != null)
        {
            realQuest = QuestManager.Instance.GetQuestProgressByID(currentQuest.quest.questID);
        }

        // ===== UPDATE TALK TO NPC =====
        if (realQuest != null && realQuest.state == QuestState.InProgress)
        {
            foreach (var obj in realQuest.quest.objectives)
            {
                if (obj.type == ObjectiveType.TalkToNPC && obj.targetID == nameObj)
                {
                    QuestManager.Instance.ReportProgress(realQuest.quest.questID, obj.objectiveName);
                    break;
                }
            }
        }

        // ===== KHÔNG CÓ QUEST =====
        if (currentQuest == null)
        {
            DialogueController.Instance.StartDialogue(dialogueID, "NV0", QuestState.Rewarded);
            return;
        }

        // ===== DIALOGUE =====
        DialogueController.Instance.StartDialogue(
            dialogueID,
            currentQuest.quest.questID,
            realQuest != null ? realQuest.state : QuestState.NotAccepted,
            () =>
            {
                // Quest đã được auto nhận từ QuestManager.
                // Nhận thưởng sẽ thực hiện bằng nút Claim trong Quest UI.
                UpdateCurrentQuest();
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
    private void OnEnable()
    {
        QuestManager.Instance.OnQuestChanged += UpdateCurrentQuest;
    }

    private void OnDisable()
    {
        QuestManager.Instance.OnQuestChanged -= UpdateCurrentQuest;
    }
    private void ShowUI() => interactButton?.gameObject.SetActive(true);
    private void HideUI() => interactButton?.gameObject.SetActive(false);
}
