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

    // Cập nhật tiến độ khi báo cáo
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
                    CheckQuestCompletion(quest);
                    questUI?.UpdateQuestProgress(quest);
                }
            }
        }
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

        // Trao thưởng cho người chơi
        AwardQuestReward(quest);
    }

    private void AwardQuestReward(Quest quest)
    {
        Debug.Log($"Awarded {quest.reward.experienceReward} XP and {quest.reward.goldReward} Gold.");
        // Ví dụ: Tăng XP và Gold
        //PlayerStats.Instance.AddXP(quest.reward.experienceReward);
        //PlayerStats.Instance.AddGold(quest.reward.goldReward);
        
        CurrencyManager.Instance.AddGold(99);
        
        // Thưởng vật phẩm (nếu có)
        foreach (var item in quest.reward.itemsReward)
        {
            Debug.Log($"Awarded item: {item.itemName}");
            // Lưu vật phẩm vào hệ thống Inventory
        }
    }
}
