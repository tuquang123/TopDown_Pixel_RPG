public class AttackSpeedBoost : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skillData)
    {
        float boost = playerStats.attackSpeed.Value * skillData.value / 100;
        playerStats.attackSpeed.AddModifier(new StatModifier(StatType.AttackSpeed, (int)boost)); 
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        return true; 
    }
}