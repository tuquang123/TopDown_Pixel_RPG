public class DefenseBoost : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skillData)
    {
        float boost = playerStats.defense.Value * skillData.value / 100;
        playerStats.attack.AddModifier(new StatModifier(StatType.Defense, (int)boost)); 
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        return true; 
    }
}