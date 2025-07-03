/*public class AllySkillDamage : ISkillAlly
{
    public void Execute(AllyBaseAI caster, AllySkillData data)
    {
        Transform target = caster.GetTarget(); // bạn đã có logic tìm target
        if (target != null && target.TryGetComponent(out IDamageable damageable))
        {
            int finalDamage = Mathf.RoundToInt(data.value);
            damageable.TakeDamage(finalDamage);
            Debug.Log($"{caster.name} cast {data.skillName} gây {finalDamage} damage!");
        }
    }

    public bool CanExecute(AllyBaseAI caster, AllySkillData data)
    {
        return caster.GetTarget() != null;
    }
}*/