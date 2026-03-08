using UnityEngine;

public class LifeDrainSkill : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skillData)
    {
        int level = playerStats.GetSkillLevel(skillData.skillID);
        SkillLevelStat stat = skillData.GetLevelStat(level);

        if (stat == null)
        {
            Debug.LogError($"Không tìm thấy dữ liệu cấp {level} cho {skillData.skillName}");
            return;
        }

        var mod = new StatModifier(
            StatType.LifeSteal,
            stat.value,
            stat.modType
        );

        playerStats.ApplyOrReplaceModifier(skillData.skillID, mod);

        Debug.Log($"LifeSteal mới: {playerStats.lifeSteal.Value}%");
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        return false;
    }
}