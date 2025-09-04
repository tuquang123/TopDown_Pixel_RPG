using UnityEngine;
using System.Collections.Generic;

public class DashSkill : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skill)
    {
        var dashComponent = playerStats.GetComponent<PlayerDash>();
        if (dashComponent != null)
        {
            dashComponent.PerformDash(skill, playerStats);
            // Cập nhật thời gian cooldown trong SkillSystem (thay vì trong DashSkill)
            SkillSystem skillSystem = playerStats.GetComponent<SkillSystem>();
            if (skillSystem != null)
            {
                skillSystem.UseSkill(skill.skillID); // Gọi UseSkill để cập nhật cooldown
            }
        }
    }

    public bool CanUse(PlayerStats playerStats, SkillData skill)
    {
        var dashComponent = playerStats.GetComponent<PlayerDash>();
        var controller = playerStats.GetComponent<PlayerController>();

        int currentLevel = playerStats.GetSkillLevel(skill.skillID);
        SkillLevelStat currentLevelStat = skill.GetLevelStat(currentLevel);

        if (currentLevelStat == null)
        {
            Debug.LogError($"Không tìm thấy dữ liệu cấp độ {currentLevel} cho kỹ năng {skill.skillName}");
            return false;
        }

        // Tự kiểm tra mana
        if (playerStats.currentMana < currentLevelStat.manaCost)
            return false;

        if (dashComponent == null) return false;
        if (dashComponent.IsDashing) return false;

        // ✅ Cho phép dash nếu có target enemy HOẶC có input joystick
        bool hasTarget = controller.GetTargetEnemy() != null;
        bool hasInput = controller.MoveInput.sqrMagnitude > 0.01f;

        if (!hasTarget && !hasInput) 
            return false;

        return true;
    }


}