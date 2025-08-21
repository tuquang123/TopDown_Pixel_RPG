using UnityEngine;

public class HealthBoost : ISkill
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
        
        var mod = new StatModifier(StatType.MaxHealth, (int)currentLevelStat.value);
        playerStats.ApplyOrReplaceModifier(skillData.skillID, mod);
        
        Debug.Log($"Áp dụng kỹ năng bị động {skillData.skillName}: +{(int)currentLevelStat.value} hp tối đa");
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        return true;
    }
}