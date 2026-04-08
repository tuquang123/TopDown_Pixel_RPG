// ================= PlayerLevel.cs =================
using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    public LevelSystem levelSystem = new LevelSystem();
    public PlayerStats playerStats; // tham chiếu stats
    public int skillPoints = 0;     // điểm kỹ năng sync với levelSystem
    [Header("Level Up Skill Popup")]
    public LevelUpSkillPopup levelUpSkillPopup;   // Kéo script LevelUpSkillPopup vào đây
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
        playerStats.CalculatePower();
        QuestManager.Instance.ReportLevelUp(newLevel);

        Debug.Log($"Level Up → Level {newLevel}");

        // === DÙNG UIMANAGER ĐỂ MỞ POPUP ===
        if (UIManager.Instance != null)
        {
            var popup = UIManager.Instance.ShowPopupByType(PopupType.LevelUpSkill) as LevelUpSkillPopup;
        
            if (popup != null)
            {
                popup.ShowLevelUpPopup(newLevel);
            }
        }
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