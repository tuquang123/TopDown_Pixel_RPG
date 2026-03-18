using System.IO;
using System.Text;
using UnityEngine;

public static class ExcelExporter
{
    public static void ExportPlayerStats(PlayerStatsData[] stats, string fileName = "PlayerStats.csv")
    {
        StringBuilder sb = new StringBuilder();

        // Header
        sb.AppendLine("Level,SkillPoints,CurrentHealth,CurrentMana,MaxHealth,MaxMana,Attack,Defense,Speed,CritChance,LifeSteal,AttackSpeed");

        // Dữ liệu
        foreach (var s in stats)
        {
            sb.AppendLine($"{s.level},{s.skillPoints},{s.currentHealth},{s.currentMana},{s.maxHealth},{s.maxMana},{s.attack},{s.defense},{s.speed},{s.critChance},{s.lifeSteal},{s.attackSpeed}");
        }

        // Lưu file vào thư mục Assets
        string path = Path.Combine(Application.dataPath, fileName);
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        Debug.Log($"Exported PlayerStats to {path}");
    }
}