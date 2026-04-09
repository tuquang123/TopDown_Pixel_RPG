using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public enum SkillType { Passive, Active }
public enum SkillID {
    DamageBoost, ShurikenThrow, Dash, Slash, None,
    HealthBoost, ManaBoost, AttackBoost, DefenseBoost,
    SpeedBoost, CritChanceBoost, AttackSpeedBoost, Invincible,
   
    LifeDrain
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

    public string description;  

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
    public string GetDescription()
    {
        // Ưu tiên description nếu đã điền tay
        if (!string.IsNullOrEmpty(description))
            return description;

        // Fallback về template
        if (!string.IsNullOrEmpty(descriptionTemplate))
            return descriptionTemplate;

        return "Không có mô tả.";
    }

    /// <summary>
    /// Tự động build description từ template + stats của level chỉ định
    /// VD template: "Tăng {value}% sát thương trong {duration}s"
    /// </summary>
    public string GetDescriptionAtLevel(int level)
    {
        if (!string.IsNullOrEmpty(description))
            return description;

        var stat = GetLevelStat(level);
        if (stat == null || string.IsNullOrEmpty(descriptionTemplate))
            return GetDescription();

        return descriptionTemplate
            .Replace("{value}",    stat.value.ToString("0.##"))
            .Replace("{duration}", stat.duration.ToString("0.##"))
            .Replace("{cooldown}", stat.cooldown.ToString("0.##"))
            .Replace("{manaCost}", stat.manaCost.ToString())
            .Replace("{level}",    stat.level.ToString());
    }

    public Color GetRarityColor()
    {
        return skillType switch
        {
            SkillType.Active  => new Color(0.2f, 0.6f, 1f),   // Xanh
            SkillType.Passive => new Color(1f, 0.75f, 0.1f),  // Vàng
            _                 => Color.white
        };
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
    [LabelText("Mode Type")] public StatModType modType = StatModType.Flat;

    [LabelText("Prefab (Override)")]
    public GameObject overridePrefab;
}
