using UnityEngine;

public class DashSkill : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skill)
    {
        var dashComponent = playerStats.GetComponent<PlayerDash>();
        if (dashComponent != null)
        {
            dashComponent.PerformDash(skill); 
        }
    }

    public bool CanUse(PlayerStats playerStats, SkillData skill)
    {
        var dashComponent = playerStats.GetComponent<PlayerDash>();
        if (playerStats.currentMana < skill.manaCost) return false;
        if (dashComponent == null) return false;
        if (dashComponent.IsDashing) return false;
        if (dashComponent.GetComponent<PlayerController>().GetTargetEnemy() == null) return false;
        return true;
    }

}
