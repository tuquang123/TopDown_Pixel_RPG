using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(PlayerStats playerStats)
    {
        PlayerStatsData data = playerStats.ToData();
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[SaveManager] Saved to {SavePath}");
    }

    public static void Load(PlayerStats playerStats)
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[SaveManager] No save file found!");
            return;
        }

        string json = File.ReadAllText(SavePath);
        PlayerStatsData data = JsonUtility.FromJson<PlayerStatsData>(json);
        playerStats.FromData(data);
        Debug.Log("[SaveManager] Save file loaded.");
    }
}