using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class PlayerStatsData
{
    public int level;
    public int skillPoints;
    public int currentHealth;
    public int currentMana;

    public float maxHealth;
    public float maxMana;
    public float attack;
    public float defense;
    public float speed;
    public float critChance;
    public float lifeSteal;
    public float attackSpeed;

    public static PlayerStatsData[] LoadFromCSV(string csvText)
    {
        string[] lines = csvText.Split('\n');
        List<PlayerStatsData> result = new List<PlayerStatsData>();

        for (int i = 1; i < lines.Length; i++) // skip header
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] data = lines[i].Trim().Split(',');

            try
            {
                PlayerStatsData s = new PlayerStatsData
                {
                    level = int.Parse(data[0]),
                    skillPoints = int.Parse(data[1]),
                    currentHealth = int.Parse(data[2]),
                    currentMana = int.Parse(data[3]),

                    maxHealth = float.Parse(data[4], CultureInfo.InvariantCulture),
                    maxMana = float.Parse(data[5], CultureInfo.InvariantCulture),
                    attack = float.Parse(data[6], CultureInfo.InvariantCulture),
                    defense = float.Parse(data[7], CultureInfo.InvariantCulture),
                    speed = float.Parse(data[8], CultureInfo.InvariantCulture),
                    critChance = float.Parse(data[9], CultureInfo.InvariantCulture),
                    lifeSteal = float.Parse(data[10], CultureInfo.InvariantCulture),
                    attackSpeed = float.Parse(data[11], CultureInfo.InvariantCulture)
                };

                result.Add(s);
            }
            catch (Exception e)
            {
                Debug.LogError($"Parse error line {i}: {lines[i]} \n{e.Message}");
            }
        }

        Debug.Log($"Loaded {result.Count} PlayerStats");
        return result.ToArray();
    }
}

// =====================================================

public class PlayerStatsManager : MonoBehaviour
{
    public PlayerStatsData[] allStats;

    [Header("Google Sheet CSV URL")]
    [SerializeField]
    private string csvUrl =
        "https://docs.google.com/spreadsheets/d/1oLroxnKhmpgLZPp4QhQVlVCFIfsWQkrdvNpIzCsoEQk/export?format=csv&gid=904143425";

    void Start()
    {
        StartCoroutine(LoadData());
    }

    IEnumerator LoadData()
    {
        Debug.Log("Downloading PlayerStats from Google Sheet...");

        using (UnityWebRequest www = UnityWebRequest.Get(csvUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Download failed: " + www.error);
                yield break;
            }

            string csvText = www.downloadHandler.text;

            // Check sai link (HTML)
            if (csvText.StartsWith("<"))
            {
                Debug.LogError("Link không phải CSV! Kiểm tra lại Google Sheet public/export.");
                yield break;
            }

            allStats = PlayerStatsData.LoadFromCSV(csvText);

            // Test
            if (allStats.Length > 0)
            {
                Debug.Log("Level 1 Attack = " + allStats[0].attack);
            }
        }
    }

    // Helper lấy stat theo level
    public PlayerStatsData GetStats(int level)
    {
        foreach (var s in allStats)
        {
            if (s.level == level)
                return s;
        }
        return null;
    }
}