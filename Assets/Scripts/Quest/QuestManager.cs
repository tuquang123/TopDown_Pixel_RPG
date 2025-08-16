using System.Collections.Generic;
using UnityEngine;

public class QuestManager : Singleton<QuestManager>
{
    public List<Quest> activeQuests = new List<Quest>(); // Các nhiệm vụ đang hoạt động
    public List<Quest> completedQuests = new List<Quest>(); // Các nhiệm vụ đã hoàn thành
    
    public QuestDatabase questDatabase;
    public QuestUI questUI; // Drag từ Inspector

    private Dictionary<string, Dictionary<string, int>> progressTracker = new Dictionary<string, Dictionary<string, int>>(); // Theo dõi tiến độ theo QuestID và ObjectiveName

    void Start()
    {
        // Bắt đầu 1 nhiệm vụ ví dụ khi game start
        var quest = questDatabase.GetQuestByID("nv1");
        if (quest != null)
        {
            StartQuest(quest);
        }
    }
    
    public void StartQuest(Quest quest)
    {
        if (!activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);
            Debug.Log($"Started Quest: {quest.questName}");
            questUI?.UpdateQuestProgress(quest);
            InitializeProgress(quest);
        }
    }

    // Khởi tạo tiến độ cho mỗi mục tiêu trong nhiệm vụ
    private void InitializeProgress(Quest quest)
    {
        if (!progressTracker.ContainsKey(quest.questID))
        {
            progressTracker[quest.questID] = new Dictionary<string, int>();
        }

        foreach (var objective in quest.objectives)
        {
            progressTracker[quest.questID][objective.objectiveName] = 0; // Mặc định tiến độ là 0
        }
    }

    // Trong ReportProgress()
    public void ReportProgress(ObjectiveType type, string objectiveName, int amount = 1)
    {
        for (int i = activeQuests.Count - 1; i >= 0; i--)
        {
            var quest = activeQuests[i];
            foreach (var obj in quest.objectives)
            {
                if (obj.type == type && obj.objectiveName == objectiveName)
                {
                    progressTracker[quest.questID][objectiveName] += amount;

                    bool wasCompleted = IsQuestCompleted(quest); // <- Check trước

                    CheckQuestCompletion(quest); // <- Gọi hàm hoàn thành

                    // Chỉ cập nhật UI nếu chưa hoàn thành (tránh bị ghi đè sau khi Clear)
                    if (!wasCompleted)
                        questUI?.UpdateQuestProgress(quest);
                }
            }
        }
    }

    
    private bool IsQuestCompleted(Quest quest)
    {
        foreach (var obj in quest.objectives)
        {
            if (!progressTracker.ContainsKey(quest.questID) ||
                !progressTracker[quest.questID].ContainsKey(obj.objectiveName) ||
                progressTracker[quest.questID][obj.objectiveName] < obj.requiredAmount)
            {
                return false;
            }
        }
        return true;
    }
    
    public int GetObjectiveProgress(string questID, string objectiveName)
    {
        if (progressTracker.ContainsKey(questID) && progressTracker[questID].ContainsKey(objectiveName))
        {
            return progressTracker[questID][objectiveName];
        }
        return 0; // Trả về 0 nếu không tìm thấy tiến độ
    }


    // Kiểm tra tiến độ của nhiệm vụ và đánh dấu hoàn thành nếu tất cả các mục tiêu đã hoàn thành
    private void CheckQuestCompletion(Quest quest)
    {
        bool allObjectivesCompleted = true;

        foreach (var obj in quest.objectives)
        {
            int currentAmount = progressTracker[quest.questID].ContainsKey(obj.objectiveName) 
                ? progressTracker[quest.questID][obj.objectiveName] 
                : 0;

            if (currentAmount < obj.requiredAmount)
            {
                allObjectivesCompleted = false;
                break;
            }
        }

        if (allObjectivesCompleted)
        {
            CompleteQuest(quest);
        }
    }

    private void CompleteQuest(Quest quest)
    {
        activeQuests.Remove(quest);
        completedQuests.Add(quest);
        Debug.Log($"Quest Completed: {quest.questName}");

        AwardQuestReward(quest); // ← Thưởng xong rồi toast

        // Clear UI
        questUI?.Clear();
    }

    private void AwardQuestReward(Quest quest)
    {
        int exp = quest.reward.experienceReward;
        int gold = quest.reward.goldReward;

        // EXP
        if (PlayerStats.Instance != null)
        {
            var playerLevel = PlayerStats.Instance.GetComponent<PlayerLevel>();
            if (playerLevel != null)
            {
                playerLevel.levelSystem.AddExp(exp);
            }
        }

        // GOLD
        CurrencyManager.Instance.AddGold(gold);

        // Bắt đầu ghi nội dung Toast
        string toastMsg = $"Hoàn thành nhiệm vụ: <b>{quest.questName}</b>\n";

        if (exp > 0)
            toastMsg += $"<color=yellow>+{exp} EXP</color>\n";

        if (gold > 0)
            toastMsg += $"<color=#FFD700>+{gold} Vàng</color>\n";

        // ITEMS từ ID
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

            toastMsg += $"<color=#00FFFF>+ {itemData.itemName}</color>\n";
            Debug.Log($"Đã nhận item từ nhiệm vụ: {itemData.itemName}");
        }

        // ✅ Show toast sau khi cộng hết
        GameEvents.OnShowToast.Raise(toastMsg);
    }



}
