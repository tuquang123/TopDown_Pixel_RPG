// ================= QuestManager.cs =================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum QuestState
{
    NotAccepted,
    InProgress,
    Completed,
    Rewarded
}

public class QuestManager : Singleton<QuestManager>
{
    public QuestDatabase questDatabase;
    public QuestUI questUI;

    public List<QuestProgress> activeQuests = new List<QuestProgress>();
    public List<QuestProgress> completedQuests = new List<QuestProgress>();
    public List<QuestProgress> readyToTurnInQuests = new List<QuestProgress>();

    [Header("Reward Popup")]
    public QuestRewardPopup questRewardPopupPrefab;

    public System.Action OnQuestChanged;
    public QuestArrow questArrow;
    public int turnInLevelIndex;

    private void Start()
    {
        EnsureCurrentQuestAssigned();
        RefreshMainQuestUI();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[QuestManager] Scene Loaded → Delay Update Arrow");
        StartCoroutine(DelayUpdateArrow());
    }

    IEnumerator DelayUpdateArrow()
    {
        yield return new WaitForSeconds(0.2f);
        UpdateArrow();
    }

    // ====================== QUEST FLOW ======================

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
        }

        OnQuestChanged?.Invoke();
        UpdateArrow();
        questUI?.UpdateQuestProgress(qp, qp.state == QuestState.Completed);
    }

    // Gọi từ NPC → mở popup trước
    public void TurnInQuest(QuestProgress qp)
    {
        if (!readyToTurnInQuests.Contains(qp)) return;

        if (questRewardPopupPrefab != null)
        {
            var popup = Instantiate(questRewardPopupPrefab);
            popup.Setup(qp);
            popup.Show();
            return;
        }

        FinalizeTurnIn(qp);
    }

    // Gọi từ popup khi bấm "Nhận thưởng", hoặc từ cheat/dev (bypass popup)
    public void FinalizeTurnIn(QuestProgress qp)
    {
        if (!readyToTurnInQuests.Contains(qp)) return;

        readyToTurnInQuests.Remove(qp);
        activeQuests.Remove(qp);
        completedQuests.Add(qp);

        qp.state = QuestState.Rewarded;
        AwardQuestReward(qp);

        EnsureCurrentQuestAssigned();
        RefreshMainQuestUI();
        OnQuestChanged?.Invoke();
        UpdateArrow();

        Debug.Log($"Quest Turned In: {qp.quest.questName}");
    }

    private void AwardQuestReward(QuestProgress qp)
    {
        if (qp == null || qp.quest == null) return;

        Quest quest = qp.quest;
        int exp = quest.reward.experienceReward;
        int gold = quest.reward.goldReward;

        if (PlayerStats.Instance != null)
        {
            var playerLevel = PlayerStats.Instance.GetComponent<PlayerLevel>();
            if (playerLevel != null)
            {
                playerLevel.levelSystem.AddExp(exp);
                FloatingTextSpawner.Instance.SpawnText("+ EXP :" + exp, transform.position, Color.magenta);
            }
        }

        CurrencyManager.Instance.AddGold(gold);

        if (exp > 0)
            RewardPopupManager.Instance.ShowReward(CommonReferent.Instance.iconExp, "EXP", exp);

        if (gold > 0)
            RewardPopupManager.Instance.ShowReward(CommonReferent.Instance.iconGold, "Vàng", gold);

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
            RewardPopupManager.Instance.ShowReward(itemData.icon, itemData.itemName, 1);
            Debug.Log($"Đã nhận item từ nhiệm vụ: {itemData.itemName}");
        }

        qp.state = QuestState.Rewarded;
    }

    // ====================== QUERY ======================

    public QuestProgress GetQuestProgressByID(string questID)
    {
        return activeQuests.Find(qp => qp.quest.questID == questID)
               ?? completedQuests.Find(qp => qp.quest.questID == questID);
    }

    private QuestProgress GetMainQuestProgress()
    {
        if (readyToTurnInQuests.Count > 0)
            return readyToTurnInQuests[0];

        if (activeQuests.Count > 0)
            return activeQuests[0];

        return null;
    }

    public void RefreshMainQuestUI()
    {
        var mainQuest = GetMainQuestProgress();

        if (mainQuest != null)
            questUI?.UpdateQuestProgress(mainQuest, mainQuest.state == QuestState.Completed);
        else
            questUI?.Clear();
    }

    public void EnsureCurrentQuestAssigned()
    {
        if (questDatabase == null || questDatabase.allQuests == null)
            return;

        if (activeQuests.Count > 0 || readyToTurnInQuests.Count > 0)
            return;

        foreach (var quest in questDatabase.allQuests)
        {
            if (quest == null) continue;

            bool isCompleted = completedQuests.Exists(qp => qp.quest.questID == quest.questID);
            if (isCompleted) continue;

            StartQuest(quest);
            return;
        }
    }

    // ====================== CHEAT / DEV (bypass popup) ======================

    public void ForceClaimCurrentQuest()
    {
        if (readyToTurnInQuests.Count == 0) return;
        FinalizeTurnIn(readyToTurnInQuests[0]);
    }

    public void ForceCompleteQuest(string questID, bool autoTurnIn = false)
    {
        var qp = activeQuests.Find(q => q.quest.questID == questID);
        if (qp == null)
        {
            Debug.LogWarning("Quest không tồn tại hoặc chưa được nhận.");
            return;
        }

        foreach (var obj in qp.quest.objectives)
            qp.progress[obj.objectiveName] = obj.requiredAmount;

        qp.state = QuestState.Completed;

        if (!readyToTurnInQuests.Contains(qp))
            readyToTurnInQuests.Add(qp);

        questUI?.UpdateQuestProgress(qp, true);
        UpdateArrow();
        Debug.Log($"Force completed quest: {qp.quest.questName}");

        if (autoTurnIn)
            FinalizeTurnIn(qp);
    }

    public void ForceCompleteOneActiveQuest()
    {
        var qp = activeQuests.Find(q => q.state == QuestState.InProgress);
        if (qp == null)
        {
            Debug.LogWarning("Không có quest nào đang làm.");
            return;
        }

        foreach (var obj in qp.quest.objectives)
            qp.progress[obj.objectiveName] = obj.requiredAmount;

        qp.state = QuestState.Completed;

        if (!readyToTurnInQuests.Contains(qp))
            readyToTurnInQuests.Add(qp);

        FinalizeTurnIn(qp);
        UpdateArrow();
        Debug.Log($"Cheat complete quest: {qp.quest.questName}");
    }

    public void DevQuestStep()
    {
        // 1. Có quest Completed → TurnIn (bypass popup)
        if (readyToTurnInQuests.Count > 0)
        {
            FinalizeTurnIn(readyToTurnInQuests[0]);
            Debug.Log("Dev Cheat: Turn In Quest");
            return;
        }

        // 2. Có quest InProgress → Complete
        var inProgress = activeQuests.Find(q => q.state == QuestState.InProgress);
        if (inProgress != null)
        {
            foreach (var obj in inProgress.quest.objectives)
                inProgress.progress[obj.objectiveName] = obj.requiredAmount;

            inProgress.state = QuestState.Completed;

            if (!readyToTurnInQuests.Contains(inProgress))
                readyToTurnInQuests.Add(inProgress);

            questUI?.UpdateQuestProgress(inProgress, true);
            UpdateArrow();
            Debug.Log("Dev Cheat: Complete Quest");
            return;
        }

        // 3. Không có quest → Start quest tiếp theo
        foreach (var quest in questDatabase.allQuests)
        {
            if (GetQuestProgressByID(quest.questID) == null)
            {
                StartQuest(quest);
                Debug.Log("Dev Cheat: Start Next Quest");
                return;
            }
        }

        Debug.Log("Dev Cheat: Không còn quest nào.");
    }

    // ====================== REPORT SPECIAL ======================

    public void ReportProgressByItemUse(string itemID)
    {
        foreach (var qp in activeQuests)
        {
            if (qp.state != QuestState.InProgress) continue;

            foreach (var obj in qp.quest.objectives)
            {
                if (obj.type != ObjectiveType.UseItem) continue;

                if (!string.Equals(obj.targetID, itemID, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!qp.progress.ContainsKey(obj.objectiveName))
                    qp.progress[obj.objectiveName] = 0;

                qp.progress[obj.objectiveName]++;
                Debug.Log($"[Quest] UseItem matched: {itemID}");

                if (qp.IsCompleted())
                {
                    if (!readyToTurnInQuests.Contains(qp))
                    {
                        readyToTurnInQuests.Add(qp);
                        qp.state = QuestState.Completed;
                        Debug.Log($"Quest {qp.quest.questName} completed!");
                    }
                }

                questUI?.UpdateQuestProgress(qp, qp.state == QuestState.Completed);
            }
        }

        OnQuestChanged?.Invoke();
        UpdateArrow();
    }

    public void ReportLevelUp(int level)
    {
        foreach (var qp in activeQuests)
        {
            if (qp.state != QuestState.InProgress) continue;

            foreach (var obj in qp.quest.objectives)
            {
                if (obj.type != ObjectiveType.LevelUp) continue;

                qp.progress[obj.objectiveName] = level;

                if (level >= obj.requiredAmount)
                {
                    if (!readyToTurnInQuests.Contains(qp))
                    {
                        readyToTurnInQuests.Add(qp);
                        qp.state = QuestState.Completed;
                        Debug.Log($"Quest {qp.quest.questName} completed!");
                    }
                }

                questUI?.UpdateQuestProgress(qp, qp.state == QuestState.Completed);
            }
        }

        OnQuestChanged?.Invoke();
        UpdateArrow();
    }

    // ====================== SAVE / LOAD ======================

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
                int progress = qp.progress.ContainsKey(obj.objectiveName)
                    ? qp.progress[obj.objectiveName] : 0;

                aq.objectives.Add(new ObjectiveProgressData
                {
                    objectiveName = obj.objectiveName,
                    currentAmount = progress
                });
            }

            data.activeQuests.Add(aq);
        }

        foreach (var qp in completedQuests)
            data.completedQuestIDs.Add(qp.quest.questID);

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

        foreach (var aq in data.activeQuests)
        {
            Quest quest = questDatabase.GetQuestByID(aq.questID);
            if (quest == null) continue;

            QuestProgress qp = new QuestProgress(quest);
            qp.state = QuestState.InProgress;

            foreach (var obj in aq.objectives)
                qp.progress[obj.objectiveName] = obj.currentAmount;

            if (qp.IsCompleted())
            {
                qp.state = QuestState.Completed;
                readyToTurnInQuests.Add(qp);
            }

            activeQuests.Add(qp);
        }

        foreach (var id in data.completedQuestIDs)
        {
            Quest quest = questDatabase.GetQuestByID(id);
            if (quest == null) continue;

            QuestProgress qp = new QuestProgress(quest);
            qp.state = QuestState.Rewarded;
            completedQuests.Add(qp);
        }

        EnsureCurrentQuestAssigned();
        RefreshMainQuestUI();
        UpdateArrow();
    }

    // ====================== ARROW ======================

    public void UpdateArrow()
    {
        StopAllCoroutines();
        StartCoroutine(UpdateArrowRoutine());
    }

    private IEnumerator UpdateArrowRoutine()
    {
        yield break; 
        float timeout = 2f;
        float timer = 0f;

        while (timer < timeout)
        {
            Transform target = GetCurrentQuestTarget();
            if (target != null)
            {
                questArrow.SetTarget(target);
                yield break;
            }

            timer += 0.2f;
            yield return new WaitForSeconds(0.2f);
        }

        //questArrow.SetTarget(null);
        Debug.LogWarning("Không tìm thấy target cho quest!");
    }

    public void ForceClearArrow()
    {
        if (questArrow != null)
            questArrow.SetTarget(null);
    }

    private Transform GetCurrentQuestTarget()
    {
        if (readyToTurnInQuests.Count > 0)
            return FindTurnInTarget(readyToTurnInQuests[0]);

        if (activeQuests.Count > 0)
            return FindQuestObjectiveTarget(activeQuests[0]);

        return null;
    }

    private Transform FindTurnInTarget(QuestProgress qp)
    {
        if (qp == null || qp.quest == null) return null;

        if (!string.IsNullOrEmpty(qp.quest.turnInNPCName))
        {
            Transform npc = FindNPCByName(qp.quest.turnInNPCName);
            if (npc != null && npc.gameObject.activeInHierarchy)
                return npc;
        }

        return FindLevelExit(goNext: false);
    }

    private Transform FindQuestObjectiveTarget(QuestProgress qp)
    {
        if (qp == null || qp.quest == null) return null;

        Vector3 playerPos = GetPlayerPosition();
        if (playerPos == Vector3.zero) return null;

        Transform bestTarget = null;
        float minDist = Mathf.Infinity;

        foreach (var obj in qp.quest.objectives)
        {
            int currentAmount = qp.progress.ContainsKey(obj.objectiveName)
                ? qp.progress[obj.objectiveName] : 0;

            if (currentAmount >= obj.requiredAmount) continue;

            string targetID = obj.targetID;
            if (string.IsNullOrEmpty(targetID)) continue;

            Transform target = null;

            switch (obj.type)
            {
                case ObjectiveType.TalkToNPC:
                    target = FindNPCByName(targetID);
                    break;
                case ObjectiveType.KillEnemies:
                    target = FindClosestEnemyByKillID(targetID);
                    break;
            }

            if (target != null)
            {
                float dist = Vector3.Distance(playerPos, target.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    bestTarget = target;
                }
            }
        }

        if (bestTarget == null)
            bestTarget = FindLevelExit(goNext: true);

        return bestTarget;
    }

    // ====================== FIND HELPERS ======================

    private Transform FindNPCByName(string name)
    {
        var npcs = FindObjectsOfType<NPCDialogueTrigger>();
        foreach (var npc in npcs)
        {
            if (string.Equals(npc.nameObj, name, System.StringComparison.OrdinalIgnoreCase))
                return npc.transform;
        }

        GameObject go = GameObject.Find(name);
        if (go != null) return go.transform;

        Debug.LogWarning($"❌ Không tìm thấy NPC với ID: {name}");
        return null;
    }

    private Transform FindNPCByQuest(Quest quest)
    {
        string possibleName1 = quest.questName;
        string possibleName2 = "NPC_0" + quest.questID;
        string possibleName3 = quest.questName.Replace(" ", "");

        var candidates = new[] { possibleName1, possibleName2, possibleName3 };

        foreach (var name in candidates)
        {
            var npc = FindNPCByName(name);
            if (npc != null) return npc;
        }

        Debug.LogWarning($"Không tìm thấy NPC nhận thưởng cho quest: {quest.questName}");
        return null;
    }

    private Transform FindClosestEnemyByName(string enemyName)
    {
        if (string.IsNullOrEmpty(enemyName)) return null;

        Vector3 playerPosition = GetPlayerPosition();
        if (playerPosition == Vector3.zero) return null;

        EnemyAI[] allEnemies = FindObjectsOfType<EnemyAI>(true);
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead || !enemy.gameObject.activeInHierarchy) continue;

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

    private Transform FindClosestEnemyByKillID(string killID)
    {
        if (EnemyTracker.Instance == null) return null;

        Vector3 playerPos = GetPlayerPosition();
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var e in EnemyTracker.Instance.GetAllEnemies())
        {
            if (e == null || e.IsDead || !e.gameObject.activeInHierarchy) continue;

            if (!string.Equals(e.KillID, killID, System.StringComparison.OrdinalIgnoreCase)) continue;

            float dist = Vector3.Distance(playerPos, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = e.transform;
            }
        }

        return closest;
    }

    private Transform FindClosestDestructible(string id)
    {
        if (DestructibleTracker.Instance == null) return null;

        Vector3 playerPos = GetPlayerPosition();
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var d in DestructibleTracker.Instance.GetAll())
        {
            if (d == null || !d.gameObject.activeInHierarchy) continue;

            if (!string.Equals(d.ObjectiveID, id, System.StringComparison.OrdinalIgnoreCase)) continue;

            float dist = Vector3.Distance(playerPos, d.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = d.transform;
            }
        }

        return closest;
    }

    Transform FindLevelExit(bool goNext)
    {
        LevelTrigger[] triggers = FindObjectsOfType<LevelTrigger>(true);

        if (triggers == null || triggers.Length == 0)
        {
            Debug.LogWarning("[Quest] Không tìm thấy LevelTrigger trong scene!");
            return null;
        }

        Vector3 playerPos = GetPlayerPosition();
        if (playerPos == Vector3.zero) return null;

        Transform best = null;
        float minDist = Mathf.Infinity;

        foreach (var t in triggers)
        {
            if (t == null || !t.gameObject.activeInHierarchy) continue;

            if (goNext && t.Type != LevelTrigger.TriggerType.Next) continue;
            if (!goNext && t.Type != LevelTrigger.TriggerType.Back) continue;

            float dist = Vector2.Distance(playerPos, t.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                best = t.transform;
            }
        }

        if (best == null)
            Debug.LogWarning("[Quest] Không tìm được trigger phù hợp!");

        return best;
    }

    private Vector3 GetPlayerPosition()
    {
        if (PlayerController.Instance != null && PlayerController.Instance.transform != null)
            return PlayerController.Instance.transform.position;

        if (CommonReferent.Instance != null && CommonReferent.Instance.playerPrefab != null)
            return CommonReferent.Instance.playerPrefab.transform.position;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) return playerObj.transform.position;

        Debug.LogWarning("[QuestManager] Cannot find Player position for Quest Arrow!");
        return Vector3.zero;
    }
}