// ================= QuestManager.cs =================
using System.Collections.Generic;
using UnityEngine;


// ===== Thêm enum trạng thái quest =====
public enum QuestState
{
    NotAccepted,  // chưa nhận
    InProgress,   // đang làm
    Completed,    // xong nhiệm vụ
    Rewarded      // đã nhận thưởng
}

public class QuestManager : Singleton<QuestManager>
{
    public QuestDatabase questDatabase;
    public QuestUI questUI;
    [Header("Auto Quest Flow")]
    public bool autoAcceptNextQuest = true;
    public bool autoTurnInOnComplete = true;

    public List<QuestProgress> activeQuests = new List<QuestProgress>();
    public List<QuestProgress> completedQuests = new List<QuestProgress>();
    public List<QuestProgress> readyToTurnInQuests = new List<QuestProgress>();

    // Start một quest
    public void StartQuest(Quest quest)
    {
        if (quest == null) return;

        if (completedQuests.Exists(qp => qp.quest.questID == quest.questID))
            return;

        if (activeQuests.Exists(qp => qp.quest.questID == quest.questID))
            return;

        QuestProgress qpNew = new QuestProgress(quest);
        qpNew.state = QuestState.InProgress;
        activeQuests.Add(qpNew);
        OnQuestChanged?.Invoke();
        questUI?.UpdateQuestProgress(qpNew);
        Debug.Log($"Started Quest: {quest.questName}");
        UpdateArrow();
    }
    
    public QuestProgress CreateQuestProgress(Quest questSO)
    {
        var existing = GetQuestProgressByID(questSO.questID);
        if (existing != null) return existing;

        var progress = new QuestProgress(questSO);
        progress.state = QuestState.NotAccepted;

        return progress;
    }

    public bool NoQuest()
    {
        return activeQuests.Count == 0;
    }
    // Report tiến độ
    public void ReportProgress(string questID, string objectiveName, int amount = 1)
    {
        var qp = activeQuests.Find(q => q.quest.questID == questID);
        if (qp == null) return;
        
        if (!qp.progress.ContainsKey(objectiveName))
            qp.progress[objectiveName] = 0;

        qp.progress[objectiveName] += amount;

        if (qp.IsCompleted())
        {
            if (!readyToTurnInQuests.Contains(qp))
            {
                readyToTurnInQuests.Add(qp);
                qp.state = QuestState.Completed;
                Debug.Log($"Quest {qp.quest.questName} completed!");
            }

            if (autoTurnInOnComplete)
            {
                TurnInQuest(qp);
                return;
            }
        }
        OnQuestChanged?.Invoke();
        UpdateArrow();
        questUI?.UpdateQuestProgress(qp, qp.state == QuestState.Completed);
    }

    // Turn in quest
    public void TurnInQuest(QuestProgress qp)
    {
        if (!readyToTurnInQuests.Contains(qp)) return;

        readyToTurnInQuests.Remove(qp);
        activeQuests.Remove(qp);
        completedQuests.Add(qp);

        qp.state = QuestState.Rewarded;
        UpdateArrow();
        AwardQuestReward(qp);
        OnQuestChanged?.Invoke();
        bool hasStartedNextQuest = TryStartNextQuest(qp.quest.questID);
        if (!hasStartedNextQuest)
        {
            questUI?.Clear();
        }
        Debug.Log($"Quest Turned In: {qp.quest.questName}");
    }

    private void AwardQuestReward(QuestProgress qp)
    {
        if (qp == null || qp.quest == null) return;

        Quest quest = qp.quest;
        int exp = quest.reward.experienceReward;
        int gold = quest.reward.goldReward;

        // EXP
        if (PlayerStats.Instance != null)
        {
            var playerLevel = PlayerStats.Instance.GetComponent<PlayerLevel>();
            if (playerLevel != null)
            {
                playerLevel.levelSystem.AddExp(exp);
                FloatingTextSpawner.Instance.SpawnText("+ EXP :" + exp, transform.position, Color.magenta);
            }
        }

        // GOLD
        CurrencyManager.Instance.AddGold(gold);

        // REWARD POPUP
        if (exp > 0)
        {
            RewardPopupManager.Instance.ShowReward(CommonReferent.Instance.iconExp, "EXP", exp);
        }

        if (gold > 0)
        {
            RewardPopupManager.Instance.ShowReward(CommonReferent.Instance.iconGold, "Vàng", gold);
        }

        foreach (var itemID in quest.reward.itemIDs)
        {
            ItemData itemData = CommonReferent.Instance.itemDatabase.GetItemByID(itemID);
            if (itemData == null)
            {
                Debug.LogWarning($"Item ID không tồn tại: {itemID}");
                continue;
            }

            ItemInstance itemInstance = new ItemInstance(itemData);
            Inventory.Instance.AddItem(itemInstance);

            // show popup cho từng item
            RewardPopupManager.Instance.ShowReward(itemData.icon, itemData.itemName, 1);

            Debug.Log($"Đã nhận item từ nhiệm vụ: {itemData.itemName}");
        }

        // Cập nhật trạng thái runtime
        qp.state = QuestState.Rewarded;
    }



    public QuestProgress GetQuestProgressByID(string questID)
    {
        return activeQuests.Find(qp => qp.quest.questID == questID)
               ?? completedQuests.Find(qp => qp.quest.questID == questID);
    }
    
    public QuestSaveData ToData()
    {
        QuestSaveData data = new QuestSaveData();

        foreach (var qp in activeQuests)
        {
            ActiveQuestData aq = new ActiveQuestData
            {
                questID = qp.quest.questID,
                objectives = new List<ObjectiveProgressData>()
            };

            foreach (var obj in qp.quest.objectives)
            {
                int progress = qp.progress.ContainsKey(obj.objectiveName) ? qp.progress[obj.objectiveName] : 0;
                aq.objectives.Add(new ObjectiveProgressData
                {
                    objectiveName = obj.objectiveName,
                    currentAmount = progress
                });
            }

            data.activeQuests.Add(aq);
        }

        foreach (var qp in completedQuests)
        {
            data.completedQuestIDs.Add(qp.quest.questID);
        }

        return data;
    }

    public void FromData(QuestSaveData data, QuestDatabase questDatabase)
    {
        activeQuests.Clear();
        completedQuests.Clear();
        readyToTurnInQuests.Clear();

        if (data == null)
        {
            questUI?.Clear();
            return;
        }

        // Load active quests
        foreach (var aq in data.activeQuests)
        {
            Quest quest = questDatabase.GetQuestByID(aq.questID);
            if (quest == null) continue;

            QuestProgress qp = new QuestProgress(quest);
            qp.state = QuestState.InProgress;

            foreach (var obj in aq.objectives)
            {
                qp.progress[obj.objectiveName] = obj.currentAmount;
            }

            activeQuests.Add(qp);

            if (qp.IsCompleted())
            {
                qp.state = QuestState.Completed;
                readyToTurnInQuests.Add(qp);
            }
        }

        // Load completed quests
        foreach (var id in data.completedQuestIDs)
        {
            Quest quest = questDatabase.GetQuestByID(id);
            if (quest != null)
            {
                QuestProgress qp = new QuestProgress(quest);
                qp.state = QuestState.Rewarded;
                completedQuests.Add(qp);
            }
        }

        // Update UI
        if (activeQuests.Count > 0)
        {
            foreach (var qp in activeQuests)
                questUI?.UpdateQuestProgress(qp, qp.state == QuestState.Completed);
        }
        else
        {
            questUI?.Clear();
        }

        if (autoTurnInOnComplete && readyToTurnInQuests.Count > 0)
        {
            var readyQuests = new List<QuestProgress>(readyToTurnInQuests);
            foreach (var readyQuest in readyQuests)
            {
                TurnInQuest(readyQuest);
            }
        }
        else if (activeQuests.Count == 0)
        {
            TryStartNextQuest();
        }
        UpdateArrow();
    }
    public void ForceCompleteQuest(string questID, bool autoTurnIn = false)
    {
        var qp = activeQuests.Find(q => q.quest.questID == questID);
        if (qp == null)
        {
            Debug.LogWarning("Quest không tồn tại hoặc chưa được nhận.");
            return;
        }

        // Set full progress cho tất cả objective
        foreach (var obj in qp.quest.objectives)
        {
            qp.progress[obj.objectiveName] = obj.requiredAmount;
        }

        qp.state = QuestState.Completed;

        if (!readyToTurnInQuests.Contains(qp))
            readyToTurnInQuests.Add(qp);

        questUI?.UpdateQuestProgress(qp, true);
        UpdateArrow();
        Debug.Log($"Force completed quest: {qp.quest.questName}");

        if (autoTurnIn)
        {
            TurnInQuest(qp);
        }
    }
   
    public void ForceCompleteOneActiveQuest()
    {
        // Chỉ lấy quest đang InProgress
        var qp = activeQuests.Find(q => q.state == QuestState.InProgress);

        if (qp == null)
        {
            Debug.LogWarning("Không có quest nào đang làm.");
            return;
        }

        // Không đụng vào quest khác
        // Không tạo quest mới

        // Set full progress
        foreach (var obj in qp.quest.objectives)
        {
            qp.progress[obj.objectiveName] = obj.requiredAmount;
        }

        qp.state = QuestState.Completed;

        if (!readyToTurnInQuests.Contains(qp))
            readyToTurnInQuests.Add(qp);

        // Nhận thưởng luôn
        TurnInQuest(qp);
        UpdateArrow();
        Debug.Log($"Cheat complete quest: {qp.quest.questName}");
    }
    public void DevQuestStep()
    {
        // 1️⃣ Nếu có quest Completed → TurnIn trước
        if (readyToTurnInQuests.Count > 0)
        {
            TurnInQuest(readyToTurnInQuests[0]);
            Debug.Log("Dev Cheat: Turn In Quest");
            return;
        }

        // 2️⃣ Nếu đang có quest InProgress → Complete
        var inProgress = activeQuests.Find(q => q.state == QuestState.InProgress);

        if (inProgress != null)
        {
            foreach (var obj in inProgress.quest.objectives)
            {
                inProgress.progress[obj.objectiveName] = obj.requiredAmount;
            }

            inProgress.state = QuestState.Completed;

            if (!readyToTurnInQuests.Contains(inProgress))
                readyToTurnInQuests.Add(inProgress);

            questUI?.UpdateQuestProgress(inProgress, true);

            Debug.Log("Dev Cheat: Complete Quest");
            return;
        }

        // 3️⃣ Nếu không có quest → Start quest tiếp theo
        foreach (var quest in questDatabase.allQuests)
        {
            if (GetQuestProgressByID(quest.questID) == null)
            {
                StartQuest(quest);
                Debug.Log("Dev Cheat: Start Next Quest");
                return;
            }
        }
        UpdateArrow();
        Debug.Log("Dev Cheat: Không còn quest nào.");
    }
    public System.Action OnQuestChanged;
    public QuestArrow questArrow;
    private Transform FindNPCByName(string name)
    {
        var npcs = FindObjectsOfType<NPCDialogueTrigger>();

        foreach (var npc in npcs)
        {
            if (npc.nameObj == name)
            {
                return npc.transform;
            }
        }

        return null;
    }

  public void UpdateArrow()
{
    if (questArrow == null) return;

    Transform arrowTarget = null;

    // Ưu tiên 1: Quest đã hoàn thành → chỉ vào NPC nhận thưởng
    if (readyToTurnInQuests.Count > 0)
    {
        QuestProgress qp = readyToTurnInQuests[0];
        arrowTarget = FindTurnInTarget(qp);
    }
    // Ưu tiên 2: Quest đang tiến hành
    else if (activeQuests.Count > 0)
    {
        QuestProgress qp = activeQuests.Find(q => q.state == QuestState.InProgress);
        if (qp != null)
        {
            arrowTarget = FindQuestObjectiveTarget(qp);
        }
    }

    // Set target cho mũi tên
    questArrow.SetTarget(arrowTarget);
}

// Tìm NPC nhận thưởng
private Transform FindTurnInTarget(QuestProgress qp)
{
    if (qp == null || qp.quest == null) return null;

    // Ưu tiên turnInNPCName
    if (!string.IsNullOrEmpty(qp.quest.turnInNPCName))
    {
        Transform npc = FindNPCByName(qp.quest.turnInNPCName);
        if (npc != null && npc.gameObject.activeInHierarchy) 
            return npc;
    }

    // Fallback objective đầu tiên
    if (qp.quest.objectives != null && qp.quest.objectives.Length > 0)
    {
        var npc = FindNPCByName(qp.quest.objectives[0].objectiveName);
        if (npc != null && npc.gameObject.activeInHierarchy) 
            return npc;
    }

    return FindNPCByQuest(qp.quest);
}

// Tìm mục tiêu cho quest đang làm
private Transform FindQuestObjectiveTarget(QuestProgress qp)
{
    if (qp == null || qp.quest == null || qp.quest.objectives == null) 
        return null;

    foreach (var obj in qp.quest.objectives)
    {
        if (obj.type == ObjectiveType.KillEnemies)
        {
            // Tìm enemy gần nhất cần giết
            Transform closestEnemy = FindClosestEnemyByName(obj.objectiveName);
            if (closestEnemy != null)
                return closestEnemy;
        }
        else
        {
            // Các objective khác (TalkToNPC, CollectItems...)
            Transform target = FindNPCByName(obj.objectiveName);
            if (target != null && target.gameObject.activeInHierarchy)
                return target;
        }
    }
    return null;
}



    private Transform FindNPCByQuest(Quest quest)
    {
       
        string possibleName1 = quest.questName;                  // ví dụ: "Tìm kho báu"
        string possibleName2 = "NPC_" + quest.questID;           // ví dụ: "NPC_Q001"
        string possibleName3 = quest.questName.Replace(" ", ""); // "TimKhoBau"

        var candidates = new[] { possibleName1, possibleName2, possibleName3 };

        foreach (var name in candidates)
        {
            var npc = FindNPCByName(name);
            if (npc != null) return npc;
        }

        // Cách C: Tìm NPC gần nhất (fallback) - không khuyến khích
        // hoặc trả về null → tắt mũi tên

        Debug.LogWarning($"Không tìm thấy NPC nhận thưởng cho quest: {quest.questName}");
        return null;
    }
    // ====================== HÀM TÌM ENEMY GẦN NHẤT ======================
    private Transform FindClosestEnemyByName(string enemyName)
    {
        if (string.IsNullOrEmpty(enemyName)) return null;

        // Lấy vị trí player một cách an toàn
        Vector3 playerPosition = GetPlayerPosition();
        if (playerPosition == Vector3.zero) return null;

        EnemyAI[] allEnemies = FindObjectsOfType<EnemyAI>(true);

        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead || !enemy.gameObject.activeInHierarchy)
                continue;

            // So sánh tên (không phân biệt hoa thường)
            if (string.Equals(enemy.EnemyName, enemyName, System.StringComparison.OrdinalIgnoreCase))
            {
                float dist = Vector3.Distance(enemy.transform.position, playerPosition);
            
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = enemy.transform;
                }
            }
        }

        return closest;
    }

// Hàm hỗ trợ lấy vị trí Player an toàn
    private Vector3 GetPlayerPosition()
    {
        // Cách 1: Từ PlayerController (khuyến nghị)
        if (PlayerController.Instance != null && PlayerController.Instance.transform != null)
        {
            return PlayerController.Instance.transform.position;
        }

        // Cách 2: Từ CommonReferent (nếu bạn có)
        if (CommonReferent.Instance != null && CommonReferent.Instance.playerPrefab != null)
        {
            return CommonReferent.Instance.playerPrefab.transform.position;
        }

        // Cách 3: Tìm bằng Tag (fallback)
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            return playerObj.transform.position;
        }

        Debug.LogWarning("[QuestManager] Cannot find Player position for Quest Arrow!");
        return Vector3.zero;
    }
    public void StartFirstQuestIfNone()
    {
        // Nếu đã có quest hoặc đã hoàn thành rồi → bỏ qua
        if (activeQuests.Count > 0 || completedQuests.Count > 0)
            return;

        if (questDatabase == null || questDatabase.allQuests == null || questDatabase.allQuests.Count == 0)
        {
            Debug.LogWarning("Không có quest trong database!");
            return;
        }

        if (!TryStartNextQuest())
        {
            Debug.LogWarning("Không còn quest nào để tự nhận.");
        }
    }
    private void Start()
    {
        StartFirstQuestIfNone();
    }

    private bool TryStartNextQuest(string completedQuestID = null)
    {
        if (!autoAcceptNextQuest) return false;
        if (questDatabase == null || questDatabase.allQuests == null || questDatabase.allQuests.Count == 0) return false;
        if (activeQuests.Count > 0) return false;

        int completedQuestIndex = -1;
        if (!string.IsNullOrEmpty(completedQuestID))
        {
            completedQuestIndex = questDatabase.allQuests.FindIndex(q => q.questID == completedQuestID);
        }

        int startIndex = completedQuestIndex >= 0 ? completedQuestIndex + 1 : 0;

        for (int i = startIndex; i < questDatabase.allQuests.Count; i++)
        {
            Quest nextQuest = questDatabase.allQuests[i];
            if (GetQuestProgressByID(nextQuest.questID) != null) continue;

            StartQuest(nextQuest);
            Debug.Log($"Auto accepted next quest: {nextQuest.questName}");
            return true;
        }

        return false;
    }
}
