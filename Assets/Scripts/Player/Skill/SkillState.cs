using System;
using UnityEngine;

[Serializable]
public class SkillState
{
    public SkillData skillData;
    public int level;

    public SkillState(SkillID skillID, SkillData data)
    {
        skillData = data;
        level = 1;
    }

    public SkillState()
    {
        level = 1; // Mặc định học kỹ năng sẽ ở cấp 1
    }

    public int GetValue()
    {
        return skillData.value * level; // Scale theo level
    }

    public int GetManaCost()
    {
        if (skillData == null) return 0;
        return skillData.manaCost + (level - 1) * 1; // ví dụ
    }


    public float GetCooldown()
    {
        if (skillData == null)
        {
            return 1f; // fallback cooldown
        }

        return Mathf.Max(0.5f, skillData.cooldown - (level - 1) * 0.1f);
    }

}