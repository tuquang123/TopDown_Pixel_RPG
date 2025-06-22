using UnityEngine;

public class SpeedBoost : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skillData)
    {
        int currentLevel = playerStats.GetSkillLevel(skillData.skillID); 
        SkillLevelStat currentLevelStat = skillData.GetLevelStat(currentLevel);

        if (currentLevelStat == null)
        {
            Debug.LogError($"Không tìm thấy dữ liệu cấp độ {currentLevel} cho kỹ năng {skillData.skillName}");
            return; 
        }
        float boost = Mathf.CeilToInt(playerStats.critChance.Value * currentLevelStat.value / 100);
        playerStats.critChance.AddModifier(new StatModifier(StatType.CritChance, (int)boost)); 
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        return true; 
    }
}