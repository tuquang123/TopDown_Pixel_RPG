using UnityEngine;

public class DamageBoost : ISkill
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
        int damageIncrease = Mathf.CeilToInt(playerStats.attack.Value * currentLevelStat.value / 100f);
        playerStats.attack.AddModifier(new StatModifier(StatType.Attack, damageIncrease));
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        return true; 
    }
}