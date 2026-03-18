using System.IO;
using System.Text;
using System.Globalization;
using UnityEngine;

[System.Serializable]
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

    // Khởi tạo rỗng
    public PlayerStatsData() { }

    // Khởi tạo từ ScriptableObject
    public PlayerStatsData(PlayerStatsSO so)
    {
        level = so.level;
        skillPoints = so.skillPoints;
        maxHealth = so.maxHealth;
        maxMana = so.maxMana;
        attack = so.attack;
        defense = so.defense;
        speed = so.speed;
        critChance = so.critChance;
        lifeSteal = so.lifeSteal;
        attackSpeed = so.attackSpeed;

        currentHealth = Mathf.RoundToInt(maxHealth);
        currentMana = Mathf.RoundToInt(maxMana);
    }

    // ---------------- Export CSV ----------------
    public static void ExportToCSV(PlayerStatsData[] stats, string fileName = "PlayerStats.csv")
    {
        StringBuilder sb = new StringBuilder();

        // Header
        sb.AppendLine("Level,SkillPoints,CurrentHealth,CurrentMana,MaxHealth,MaxMana,Attack,Defense,Speed,CritChance,LifeSteal,AttackSpeed");

        // Data
        foreach (var s in stats)
        {
            sb.AppendLine($"{s.level},{s.skillPoints},{s.currentHealth},{s.currentMana},{s.maxHealth},{s.maxMana},{s.attack},{s.defense},{s.speed},{s.critChance},{s.lifeSteal},{s.attackSpeed}");
        }

        string path = Path.Combine(Application.dataPath, fileName);
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        Debug.Log($"Exported PlayerStats to {path}");
    }

    // ---------------- Load CSV ----------------
    public static PlayerStatsData[] LoadFromCSV(string fileName = "PlayerStats.csv")
    {
        string path = Path.Combine(Application.dataPath, fileName);

        if (!File.Exists(path))
        {
            Debug.LogError("File CSV không tồn tại: " + path);
            return new PlayerStatsData[0];
        }

        string[] lines = File.ReadAllLines(path);
        if (lines.Length <= 1)
        {
            Debug.LogError("CSV rỗng hoặc chỉ có header");
            return new PlayerStatsData[0];
        }

        PlayerStatsData[] allStats = new PlayerStatsData[lines.Length - 1];

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');

            PlayerStatsData s = new PlayerStatsData();
            s.level = int.Parse(values[0]);
            s.skillPoints = int.Parse(values[1]);
            s.currentHealth = int.Parse(values[2]);
            s.currentMana = int.Parse(values[3]);
            s.maxHealth = float.Parse(values[4], CultureInfo.InvariantCulture);
            s.maxMana = float.Parse(values[5], CultureInfo.InvariantCulture);
            s.attack = float.Parse(values[6], CultureInfo.InvariantCulture);
            s.defense = float.Parse(values[7], CultureInfo.InvariantCulture);
            s.speed = float.Parse(values[8], CultureInfo.InvariantCulture);
            s.critChance = float.Parse(values[9], CultureInfo.InvariantCulture);
            s.lifeSteal = float.Parse(values[10], CultureInfo.InvariantCulture);
            s.attackSpeed = float.Parse(values[11], CultureInfo.InvariantCulture);

            allStats[i - 1] = s;
        }

        Debug.Log("Đã load " + allStats.Length + " PlayerStats từ CSV.");
        return allStats;
    }
}