// ================= PlayerLevel.cs =================
using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    public LevelSystem levelSystem = new LevelSystem();
    public PlayerStats playerStats; // tham chiếu stats
    public int skillPoints = 0;     // điểm kỹ năng sync với levelSystem

    private void Awake()
    {
        if (levelSystem == null)
            levelSystem = new LevelSystem();
    }

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();

        // đăng ký sự kiện
        levelSystem.OnLevelUp += HandleLevelUp;
        EnemyAI.OnEnemyDefeated += GainExp;
    }

    private void OnDestroy()
    {
        levelSystem.OnLevelUp -= HandleLevelUp;
        EnemyAI.OnEnemyDefeated -= GainExp;
    }

    private void HandleLevelUp(int newLevel)
    {
        skillPoints = levelSystem.skillPoints;
        playerStats.skillPoints = skillPoints;
        Debug.Log($"[PlayerLevel] Lên cấp {newLevel}! Tổng điểm kỹ năng: {skillPoints}");
    }

    private void GainExp(float amount)
    {
        levelSystem.AddExp(amount);
        skillPoints = levelSystem.skillPoints;
        playerStats.skillPoints = skillPoints;
        Debug.Log($"[PlayerLevel] Nhận {amount} EXP! Tổng EXP: {levelSystem.exp}/{levelSystem.ExpRequired}");
    }

    // Hàm hỗ trợ load
    public void LoadLevel(int lvl, float exp, int sp)
    {
        levelSystem.SetLevel(lvl, exp, sp);
        skillPoints = sp;
        playerStats.skillPoints = sp;
    }
}