using UnityEngine;

public class AllySkillBuff : ISkillAlly
{
    public void Execute(AllyBaseAI caster, AllySkillData data)
    {
        IBuffableStats stats = data.targetType switch
        {
            BuffTargetType.Self => caster.GetStats(),
            BuffTargetType.Ally => caster.FindNearestAlly(),
            BuffTargetType.Player => caster.FindPlayerStats(),
            _ => null
        };

        stats?.ModifyAttack(data.value, data.duration);


        if (stats == null) return;

        switch (data.buffTargetStat)
        {
            case BuffTargetStat.Attack:
                stats.ModifyAttack(data.value, data.duration);
                break;
            case BuffTargetStat.Defense:
                stats.ModifyDefense(data.value, data.duration);
                break;
            case BuffTargetStat.Speed:
                stats.ModifySpeed(data.value, data.duration);
                break;
        }

        Debug.Log($"{caster.name} sử dụng buff {data.skillName} tăng {data.buffTargetStat} thêm {data.value} trong {data.duration} giây!");
    }

    public bool CanExecute(AllyBaseAI caster, AllySkillData data)
    {
        return true; 
    }
}