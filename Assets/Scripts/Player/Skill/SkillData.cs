using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public enum SkillType { Passive, Active }
public enum SkillID {
    DamageBoost, ShurikenThrow, Dash, Slash, None,
    HealthBoost, ManaBoost, AttackBoost, DefenseBoost,
    SpeedBoost, CritChanceBoost, AttackSpeedBoost, Invincible
}

[CreateAssetMenu(fileName = "New Skill", menuName = "Skill System/Skill Data")]
public class SkillData : ScriptableObject
{
    [Title("General Info")]
    public SkillID skillID;
    
    [Title("Skill Name")]
    public string skillName;
    
    [Title("Skill Type")]
    public SkillType skillType;
    
    [Title("Required Level")]
    public int requiredLevel;

    [PreviewField(75), HideLabel]
    [HorizontalGroup("IconAndStats", 75)]
    public Sprite icon;

    [Title("Skill Stats Per Level")]
    [TableList]
    public List<SkillLevelStat> levelStats = new();

    [Title("References")]
    [LabelText("Skill Effect Prefab")]
    public GameObject prefab;

    [Title("Description")]
    [TextArea(3, 5)]  // Min 3 dòng, max 5 dòng (có thể điều chỉnh)
    public string descriptionTemplate;
    
    [HideInInspector] public int maxLevel => levelStats.Count;
    
    public SkillLevelStat GetLevelStat(int level)
    {
        return levelStats.Find(stat => stat.level == level);
    }
    public GameObject GetPrefabAtLevel(int level)
    {
        var stat = GetLevelStat(level);
        return stat != null && stat.overridePrefab != null ? stat.overridePrefab : prefab;
    }
}

[System.Serializable]
public class SkillLevelStat
{
    [LabelText("Level")] public int level;

    [LabelText("Mana Cost")] public int manaCost;
    [LabelText("Cooldown")] public float cooldown;
    [LabelText("Duration")] public float duration;
    [LabelText("Power Value")] public float value;

    [LabelText("Prefab (Override)")]
    public GameObject overridePrefab;
}
