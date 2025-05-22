public class DamageBoost : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skillData)
    {
        float damageIncrease = playerStats.attack.Value * skillData.value / 100;
        playerStats.attack.AddModifier(new StatModifier(StatType.Attack, (int)damageIncrease)); 
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        return true; 
    }
}