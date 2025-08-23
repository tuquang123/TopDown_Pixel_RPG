using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public bool allAllItem;
    
    private IEnumerator Start()
    {
        // Chờ 1 frame để tất cả Singleton (QuestManager, CommonReferent...) Awake xong
        yield return null;  
        LoadGame();

        if (allAllItem) AddAllItemsToInventory();

        // Cheat test
       // TriggerKillGoblinQuest();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveGame(); // Auto save khi minimize hoặc thoát app
    }

    private void OnApplicationQuit()
    {
        SaveGame(); // Editor / PC build vẫn gọi
    }

    public void SaveGame()
    {
        SaveManager.Save(CommonReferent.Instance.playerStats, 
            CommonReferent.Instance.inventory,
            CommonReferent.Instance.equipment, 
            CommonReferent.Instance.skill, 
            CommonReferent.Instance.playerLevel);
    }

    public void LoadGame()
    {
        SaveManager.Load(CommonReferent.Instance.playerStats,
            CommonReferent.Instance.inventory,
            CommonReferent.Instance.equipment,
            CommonReferent.Instance.itemDatabase,
            CommonReferent.Instance.skill,
            CommonReferent.Instance.playerLevel);
        
        CommonReferent.Instance.playerLevelUI.RefreshUI(); 

    }

    [Button("Clear Save Data")]
    private void ClearSave()
    {
        SaveManager.Clear();
    }

    // Cheat
    public void TriggerKillGoblinQuest()
    {
        var quest = QuestManager.Instance.questDatabase.GetQuestByID("NV1");
        if (quest != null)
        {
            QuestManager.Instance.StartQuest(quest);
        }
    }

    // Cheat
    private void AddAllItemsToInventory()
    {
        foreach (var item in CommonReferent.Instance.itemDatabase.allItems)
        {
            if (item != null)
            {
                ItemInstance itemInstance = new ItemInstance(item);
                CommonReferent.Instance.inventory.AddItem(itemInstance);
            }
        }
    }
}
