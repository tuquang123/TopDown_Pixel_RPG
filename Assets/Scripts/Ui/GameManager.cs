using UnityEngine;

public class GameManager : MonoBehaviour
{
    public ItemDatabase itemDatabase;
    public Inventory inventory;

    private void Start()
    {
        AddAllItemsToInventory();
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
        foreach (var item in itemDatabase.allItems)
        {
            if (item != null)
            {
                // Tạo ItemInstance từ ItemData
                ItemInstance itemInstance = new ItemInstance(item);
                inventory.AddItem(itemInstance);
            }
        }
    }
}