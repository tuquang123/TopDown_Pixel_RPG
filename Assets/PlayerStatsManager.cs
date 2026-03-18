using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public PlayerStatsData[] allStats;

    void Start()
    {
        // Load dữ liệu từ CSV
        allStats = PlayerStatsData.LoadFromCSV();

        // In ra thông tin Level 1 để kiểm tra
        if (allStats.Length > 0)
        {
            Debug.Log("Level 1 MaxHealth = " + allStats[0].maxHealth);
        }

        // Ví dụ: sửa attack của level 2
        if (allStats.Length > 1)
        {
            allStats[1].attack += 5;
            Debug.Log("Level 2 attack sau khi tăng: " + allStats[1].attack);
        }

        // Export lại CSV nếu muốn lưu các chỉnh sửa
        PlayerStatsData.ExportToCSV(allStats);
    }
}