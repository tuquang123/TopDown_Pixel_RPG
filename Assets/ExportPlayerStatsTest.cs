using UnityEngine;

public class ExportPlayerStatsTest : MonoBehaviour
{
    // Kéo thả các ScriptableObject PlayerStatsSO vào đây
    public PlayerStatsSO[] allPlayerStatsSO;

    [ContextMenu("Export Player Stats")] // Cho phép click trực tiếp trong Inspector
    public void ExportStats()
    {
        // Tạo mảng PlayerStatsData
        PlayerStatsData[] allStats = new PlayerStatsData[allPlayerStatsSO.Length];

        for (int i = 0; i < allPlayerStatsSO.Length; i++)
        {
            allStats[i] = new PlayerStatsData(allPlayerStatsSO[i]);
        }

        // Gọi hàm xuất CSV
        ExcelExporter.ExportPlayerStats(allStats);
    }
}