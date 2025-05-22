public class SpeedBoost : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skillData)
    {
        float boost = playerStats.critChance.Value * skillData.value / 100;
        playerStats.critChance.AddModifier(new StatModifier(StatType.CritChance, (int)boost)); 
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        return true; 
    }
}