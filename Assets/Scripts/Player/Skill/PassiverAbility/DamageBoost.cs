using UnityEngine;

public class DamageBoost : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skillData)
    {
        int damageIncrease = Mathf.CeilToInt(playerStats.attack.Value * skillData.value / 100f);
        playerStats.attack.AddModifier(new StatModifier(StatType.Attack, damageIncrease));
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        return true; 
    }
}