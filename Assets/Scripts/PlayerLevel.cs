using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    public LevelSystem levelSystem = new LevelSystem();

    private void Start()
    {
        levelSystem.OnLevelUp += HandleLevelUp;
        EnemyAI.OnEnemyDefeated += GainExp; // Đăng ký sự kiện khi kẻ địch bị tiêu diệt
    }

    private void OnDestroy()
    {
        EnemyAI.OnEnemyDefeated -= GainExp; // Hủy đăng ký sự kiện để tránh lỗi
    }

    private void HandleLevelUp(int newLevel)
    {
        Debug.Log($"Lên cấp {newLevel}! Bạn nhận được 1 điểm kỹ năng.");
    }

    private void GainExp(float amount)
    {
        levelSystem.AddExp(amount);
        Debug.Log($"Nhận {amount} EXP! Tổng EXP: {levelSystem.exp}/{levelSystem.ExpRequired}");
    }
}