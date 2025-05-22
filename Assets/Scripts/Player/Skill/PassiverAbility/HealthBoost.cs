public class HealthBoost : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skillData)
    {
        playerStats.maxHealth.AddModifier(new StatModifier(StatType.MaxHealth, (int)skillData.value));
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        return true;
    }
}