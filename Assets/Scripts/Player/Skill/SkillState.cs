using System;

[Serializable]
public class SkillState
{
    public SkillID skillID;
    public SkillData skillData;
    public int level;

    public SkillState(SkillID skillID, SkillData skillData, int level = 1)
    {
        this.skillID = skillID;
        this.skillData = skillData;
        this.level = level; 
    }

    public float GetCooldown()
    {
        SkillLevelStat stat = skillData.GetLevelStat(level);
        return stat?.cooldown ?? 0f;
    }

    public int GetManaCost()
    {
        SkillLevelStat stat = skillData.GetLevelStat(level);
        return stat?.manaCost ?? 0;
    }
}