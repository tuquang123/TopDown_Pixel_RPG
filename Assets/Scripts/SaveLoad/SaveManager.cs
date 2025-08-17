using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public PlayerStatsData playerStats;
    public List<ItemInstanceData> inventory;
    public List<EquipmentData> equipment;
    public SkillSaveData skill;
}

public static class SaveManager
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(PlayerStats playerStats, Inventory inventory, Equipment equipment , SkillSystem skill)
    {
        SaveData data = new SaveData
        {
            inventory = inventory.ToData(),
            equipment = equipment.ToData(),
            skill = skill.ToData()
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[SaveManager] Saved to {SavePath}");
    }

    public static void Load(PlayerStats playerStats, Inventory inventory, Equipment equipment, ItemDatabase db,
        SkillSystem skill)
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[SaveManager] No save file found!");
            return;
        }

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        
        inventory.FromData(data.inventory, db);
        equipment.FromData(data.equipment, db, playerStats);
        skill.FromData(data.skill);
        
        equipment.ReapplyEquipmentStats(playerStats);
        
        skill.ReapplyPassiveSkills(playerStats);

        Debug.Log("[SaveManager] Save file loaded.");
    }
    
    public static void Clear()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("[SaveManager] Save file deleted.");
        }
        else
        {
            Debug.Log("[SaveManager] No save file to delete.");
        }
    }


}
