using UnityEngine;

public class SlashSkill : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skill)
    {
        var slashComponent = playerStats.GetComponent<PlayerSlash>();
        if (slashComponent != null)
        {
            slashComponent.Slash(skill); // Truyền skill để sử dụng các thông số như cooldown, damage, v.v.
            
            // Cập nhật thời gian cooldown và mana thông qua SkillSystem
            SkillSystem skillSystem = playerStats.GetComponent<SkillSystem>();
            if (skillSystem != null)
            {
                skillSystem.UseSkill(skill.skillID); // Gọi UseSkill để xử lý mana và cooldown
            }
        }
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        // Lấy cấp độ hiện tại từ PlayerStats
        int currentLevel = playerStats.GetSkillLevel(skillData.skillID);
        SkillLevelStat currentLevelStat = skillData.GetLevelStat(currentLevel);

        if (currentLevelStat == null)
        {
            Debug.LogError($"Không tìm thấy dữ liệu cấp độ {currentLevel} cho kỹ năng {skillData.skillName}");
            return false;
        }

        // Kiểm tra mana
        if (playerStats.currentMana < currentLevelStat.manaCost)
            return false;

        // Kiểm tra cooldown và các điều kiện khác thông qua SkillSystem
        SkillSystem skillSystem = playerStats.GetComponent<SkillSystem>();
        if (skillSystem == null || !skillSystem.CanUseSkill(skillData.skillID))
            return false;

        // Kiểm tra component PlayerSlash
        var slashComponent = playerStats.GetComponent<PlayerSlash>();
        if (slashComponent == null)
            return false;

        return true;
    }
}