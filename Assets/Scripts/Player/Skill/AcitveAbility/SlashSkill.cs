using UnityEngine;

public class SlashSkill : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skill)
    {
        var slashComponent = playerStats.GetComponent<PlayerSlash>();
        if (slashComponent != null)
        {
            slashComponent.Slash(skill); 
            
            SkillSystem skillSystem = playerStats.GetComponent<SkillSystem>();
            if (skillSystem != null)
            {
                skillSystem.UseSkill(skill.skillID); 
            }
        }
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        int currentLevel = playerStats.GetSkillLevel(skillData.skillID);
        SkillLevelStat currentLevelStat = skillData.GetLevelStat(currentLevel);

        if (currentLevelStat == null)
        {
            Debug.LogError($"Không tìm thấy dữ liệu cấp độ {currentLevel} cho kỹ năng {skillData.skillName}");
            return false;
        }

        if (playerStats.currentMana < currentLevelStat.manaCost)
            return false;

        var slashComponent = playerStats.GetComponent<PlayerSlash>();
        if (slashComponent == null)
            return false;

        return true;
    }

}