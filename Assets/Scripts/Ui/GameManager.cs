using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        LoadGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    public void SaveGame()
    {
        SaveManager.Save(CommonReferent.Instance.playerStats, CommonReferent.Instance.inventory, 
            CommonReferent.Instance.equipment , CommonReferent.Instance.skill);
    }
    public void LoadGame()
    {
        SaveManager.Load(CommonReferent.Instance.playerStats, CommonReferent.Instance.inventory,
            CommonReferent.Instance.equipment, CommonReferent.Instance.itemDatabase , CommonReferent.Instance.skill);
    }

    public bool allAllItem;
    private void Start()
    {
        if(allAllItem) AddAllItemsToInventory();
        TriggerKillGoblinQuest();
    }
    
    //Cheat
    public void TriggerKillGoblinQuest()
    {
        var quest = QuestManager.Instance.questDatabase.GetQuestByID("NV1");
        if (quest != null)
        {
            QuestManager.Instance.StartQuest(quest);
        }
    }

    //Cheat
    private void AddAllItemsToInventory()
    {
        foreach (var item in CommonReferent.Instance.itemDatabase.allItems)
        {
            if (item != null)
            {
                // Tạo ItemInstance từ ItemData
                ItemInstance itemInstance = new ItemInstance(item);
                CommonReferent.Instance.inventory.AddItem(itemInstance);
            }
        }
    }
}