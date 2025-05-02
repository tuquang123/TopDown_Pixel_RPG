using UnityEngine;

public class DashSkill : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skill)
    {
        var dashComponent = playerStats.GetComponent<PlayerDash>();
        if (dashComponent != null)
        {
            dashComponent.PerformDash(skill); // truyền skill để có thể dùng cooldown, thời gian dash, v.v.
        }
    }

    public bool CanUse(PlayerStats playerStats, SkillData skill)
    {
        return playerStats.currentMana >= skill.manaCost;
    }
}
