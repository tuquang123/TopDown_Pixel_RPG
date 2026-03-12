using UnityEngine;

public class CriticalBoost : ISkill
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
            StatType.CritChance,
            stat.value,
            stat.modType
        );

        playerStats.ApplyOrReplaceModifier(skillData.skillID, mod);

        Debug.Log($"CritChance mới: {playerStats.critChance.Value}%");
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        return true;
    }
}