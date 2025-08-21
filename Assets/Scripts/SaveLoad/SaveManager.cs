using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public PlayerStatsData playerStats;
    public List<ItemInstanceData> inventory;
    public List<EquipmentData> equipment;
    public SkillSaveData skill;
    public LevelData levelData; 
    public QuestSaveData questData;
}

[System.Serializable] 
public class LevelData
{
    public int level;
    public float exp;
    public int skillPoints;
}

[System.Serializable]
public class QuestSaveData
{
    public List<ActiveQuestData> activeQuests = new();
    public List<string> completedQuestIDs = new(); // chỉ lưu ID là đủ
}

[System.Serializable]
public class ActiveQuestData
{
    public string questID;
    public List<ObjectiveProgressData> objectives = new();
}

[System.Serializable]
public class ObjectiveProgressData
{
    public string objectiveName;
    public int currentAmount;
}


public static class SaveManager
{
    private const string SaveKey = "GameSave";

    public static void Save(PlayerStats playerStats, Inventory inventory, Equipment equipment,
        SkillSystem skill, PlayerLevel playerLevel)
    {
        SaveData data = new SaveData
        {
            inventory = inventory.ToData(),
            equipment = equipment.ToData(),
            skill = skill.ToData(),
            levelData = new LevelData
            {
                level = playerLevel.levelSystem.level,
                exp = playerLevel.levelSystem.exp,
                skillPoints = playerLevel.skillPoints
            },
            questData = QuestManager.Instance.ToData()
        };

        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        Debug.Log("[SaveManager] Saved to PlayerPrefs");
    }

    public static void Load(PlayerStats playerStats, Inventory inventory, Equipment equipment, ItemDatabase db,
        SkillSystem skill, PlayerLevel playerLevel)
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.LogWarning("[SaveManager] No save data found in PlayerPrefs!");
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        inventory.FromData(data.inventory, db);
        equipment.FromData(data.equipment, db, playerStats);
        skill.FromData(data.skill);
        
        equipment.ReapplyEquipmentStats(playerStats);
        skill.ReapplyPassiveSkills(playerStats);

        if (data.levelData != null)
        {
            playerLevel.LoadLevel(
                data.levelData.level,
                data.levelData.exp,
                data.levelData.skillPoints
            );

            Debug.Log($"[SaveManager] Loaded Level {data.levelData.level}, EXP {data.levelData.exp}, SP {data.levelData.skillPoints}");
        }

        QuestManager.Instance.FromData(data.questData, QuestManager.Instance.questDatabase);

        Debug.Log("[SaveManager] Save file loaded from PlayerPrefs.");
    }

    public static void Clear()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            PlayerPrefs.DeleteKey(SaveKey);
            Debug.Log("[SaveManager] Save data deleted from PlayerPrefs.");
        }
        else
        {
            Debug.Log("[SaveManager] No save data to delete.");
        }
    }

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }
}
