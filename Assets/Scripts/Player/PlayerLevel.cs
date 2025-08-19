using UnityEngine;
[System.Serializable]
public class LevelData
{
    public int level;
    public float exp;
    public int skillPoints;
}


public class PlayerLevel : MonoBehaviour
{
    public LevelSystem levelSystem = new LevelSystem();
    public PlayerStats playerStats; // Tham chiếu đến hệ thống chỉ số
    public int skillPoints = 0; // Điểm kỹ năng nhận được khi lên cấp

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        levelSystem.OnLevelUp += HandleLevelUp;
        EnemyAI.OnEnemyDefeated += GainExp; // Đăng ký sự kiện khi kẻ địch bị tiêu diệt
    }

    private void OnDestroy()
    {
        levelSystem.OnLevelUp -= HandleLevelUp;
        EnemyAI.OnEnemyDefeated -= GainExp; // Hủy đăng ký sự kiện để tránh lỗi
    }
    private void HandleLevelUp(int newLevel)
    {
        skillPoints++; // Cộng điểm kỹ năng khi lên cấp
        playerStats.skillPoints ++; 
        Debug.Log($"Lên cấp {newLevel}! Bạn nhận được 1 điểm kỹ năng. Tổng điểm: {skillPoints}");
    }

    private void GainExp(float amount)
    {
        levelSystem.AddExp(amount);
        Debug.Log($"Nhận {amount} EXP! Tổng EXP: {levelSystem.exp}/{levelSystem.ExpRequired}");
    }
}