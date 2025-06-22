using UnityEngine;

public class ManaBoost : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skillData)
    {
        // Lấy cấp độ hiện tại từ PlayerStats
        int currentLevel = playerStats.GetSkillLevel(skillData.skillID);
        SkillLevelStat currentLevelStat = skillData.GetLevelStat(currentLevel);

        if (currentLevelStat == null)
        {
            Debug.LogError($"Không tìm thấy dữ liệu cấp độ {currentLevel} cho kỹ năng {skillData.skillName}");
            return;
        }

        // Áp dụng modifier với value đúng
        playerStats.maxMana.AddModifier(new StatModifier(StatType.MaxMana, (int)currentLevelStat.value));
        Debug.Log($"Áp dụng kỹ năng bị động {skillData.skillName}: +{(int)currentLevelStat.value} mana tối đa");
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        // Kỹ năng bị động không cần thi triển thủ công
        return false;
    }
}