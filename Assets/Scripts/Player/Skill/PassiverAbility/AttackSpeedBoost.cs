using UnityEngine;

public class AttackSpeedBoost : ISkill
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

        float boost = Mathf.CeilToInt(playerStats.attackSpeed.Value * currentLevelStat.value / 100);
        playerStats.attackSpeed.AddModifier(new StatModifier(StatType.AttackSpeed, (int)boost));
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        return true; 
    }
}