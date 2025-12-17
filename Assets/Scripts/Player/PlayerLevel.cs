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
        
        levelSystem.OnLevelUp -= HandleLevelUp;
        EnemyAI.OnEnemyDefeated -= GainExp;
        
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

        // Gọi floating text Level Up
        FloatingTextSpawner.Instance.SpawnText(
            $"<size=50>LEVEL UP!</size>\n<size=30>Lv Up {newLevel}</size>",
            transform.position + Vector3.up,
            new Color(1f, 0.85f, 0f)
        );


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
        levelSystem.SetLevelDirectly(lvl, exp, sp);
        skillPoints = sp;
        playerStats.skillPoints = sp;
    }

    
}