// ================= QuestManager.cs =================
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : Singleton<QuestManager>
{
    public QuestDatabase questDatabase;
    public QuestUI questUI;

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

        questUI?.UpdateQuestProgress(qpNew);
        Debug.Log($"Started Quest: {quest.questName}");
    }
    
    public QuestProgress CreateQuestProgress(Quest questSO)
    {
        var existing = GetQuestProgressByID(questSO.questID);
        if (existing != null) return existing;

        var progress = new QuestProgress(questSO);
        activeQuests.Add(progress); 
        return progress;
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
        }

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

        AwardQuestReward(qp);

        questUI?.Clear();
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
    }

}
