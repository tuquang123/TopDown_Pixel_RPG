// ================= SaveManager.cs =================

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int version = 1;
    public long lastSaveTime;

    public PlayerStatsData playerStats;
    public List<ItemInstanceData> inventory = new();
    public List<EquipmentData> equipment = new();
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
    private const int CurrentVersion = 1;
    private static string SaveFolder => Application.persistentDataPath + "/Saves/";

    private static string GetSlotPath(int slot)
    {
        return SaveFolder + $"save_{slot}.json";
    }

    private static string GetBackupPath(int slot)
    {
        return SaveFolder + $"save_{slot}_backup.json";
    }

    public static void Initialize()
    {
        if (!Directory.Exists(SaveFolder))
            Directory.CreateDirectory(SaveFolder);
    }

    public static void Save(int slot,
        PlayerStats playerStats,
        Inventory inventory,
        Equipment equipment,
        SkillSystem skill,
        PlayerLevel playerLevel)
    {
        Initialize();

        SaveData data = new SaveData
        {
            version = CurrentVersion,
            lastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
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

        string path = GetSlotPath(slot);
        string backupPath = GetBackupPath(slot);

        // Backup trước khi ghi
        if (File.Exists(path))
            File.Copy(path, backupPath, true);

        File.WriteAllText(path, json);

        PlayerPrefs.Save(); // flush cho WebGL
        Debug.Log($"[SaveManager] Saved slot {slot}");
    }

    public static bool Load(int slot,
        PlayerStats playerStats,
        Inventory inventory,
        Equipment equipment,
        ItemDatabase db,
        SkillSystem skill,
        PlayerLevel playerLevel)
    {
        Initialize();

        string path = GetSlotPath(slot);
        string backupPath = GetBackupPath(slot);

        if (!File.Exists(path))
        {
            Debug.LogWarning("Save file not found.");
            return false;
        }

        string json = File.ReadAllText(path);

        SaveData data = null;

        try
        {
            data = JsonUtility.FromJson<SaveData>(json);
        }
        catch
        {
            Debug.LogWarning("Save corrupted. Trying backup...");
            if (File.Exists(backupPath))
            {
                json = File.ReadAllText(backupPath);
                data = JsonUtility.FromJson<SaveData>(json);
            }
            else
            {
                return false;
            }
        }

        if (data.version != CurrentVersion)
        {
            Debug.Log("Upgrading save version...");
            UpgradeSave(data);
        }

        inventory.FromData(data.inventory, db);
        equipment.FromData(data.equipment, db, playerStats);
        skill.FromData(data.skill);

        equipment.ReapplyEquipmentStats(playerStats);
        equipment.OnEventTypeWeapon();
        skill.ReapplyPassiveSkills(playerStats);

        playerStats.Revive();

        if (data.levelData != null)
        {
            playerLevel.LoadLevel(
                data.levelData.level,
                data.levelData.exp,
                data.levelData.skillPoints
            );
        }

        QuestManager.Instance.FromData(data.questData, QuestManager.Instance.questDatabase);

        Debug.Log($"[SaveManager] Loaded slot {slot}");
        return true;
    }

    private static void UpgradeSave(SaveData data)
    {
        // Future proof upgrade logic
        data.version = CurrentVersion;
    }

    public static bool HasSave(int slot)
    {
        return File.Exists(GetSlotPath(slot));
    }

    public static void Delete(int slot)
    {
        string path = GetSlotPath(slot);
        if (File.Exists(path))
            File.Delete(path);
    }
    public static void ClearAll()
    {
        Initialize();

        if (!Directory.Exists(SaveFolder))
        {
            Debug.Log("[SaveManager] No save folder found.");
            return;
        }

        var files = Directory.GetFiles(SaveFolder);

        foreach (var file in files)
        {
            try
            {
                File.Delete(file);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to delete file: {file} | {e.Message}");
            }
        }

        PlayerPrefs.Save(); // flush WebGL

        Debug.Log("[SaveManager] All save data cleared.");
    }

}

