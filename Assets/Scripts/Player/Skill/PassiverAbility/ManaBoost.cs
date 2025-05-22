public class ManaBoost : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skillData)
    {
        playerStats.maxMana.AddModifier(new StatModifier(StatType.MaxMana, (int)skillData.value));
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        return true;
    }
}