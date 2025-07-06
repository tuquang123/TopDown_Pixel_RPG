public interface ISkillAlly
{
    void Execute(AllyBaseAI caster, AllySkillData skillData);
    bool CanExecute(AllyBaseAI caster, AllySkillData skillData);
}

public class AllySkillFactory
{
    public static ISkillAlly GetSkillImplementation(AllySkillData data)
    {
        return data.effectType switch
        {
            SkillEffectType.Buff => new AllySkillBuff(),
            //SkillEffectType.Heal => new AllySkillHeal(),
            //SkillEffectType.Buff => new AllySkillBuff(),
            _ => null
        };
    }
}


