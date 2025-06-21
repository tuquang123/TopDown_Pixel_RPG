using UnityEngine;
using Sirenix.OdinInspector;

public enum SkillType { Passive, Active }
public enum SkillID { DamageBoost, ShurikenThrow, Dash, Slash, None,
    HealthBoost,
    ManaBoost,
    AttackBoost,
    DefenseBoost,
    SpeedBoost,
    CritChanceBoost,
    AttackSpeedBoost,
    Invincible
}

[CreateAssetMenu(fileName = "New Skill", menuName = "Skill System/Skill Data")]
public class SkillData : ScriptableObject
{
    [Title("General Info")]
    //[EnumToggleButtons]
    public SkillID skillID;

    [HorizontalGroup("SkillNameAndType")]
    //[LabelWidth(90)]
    public string skillName;

    [HorizontalGroup("SkillNameAndType")]
    //[LabelWidth(70)]
    public SkillType skillType;

    [Range(1, 100)]
    public int requiredLevel;

    [PreviewField(75), HideLabel]
    [HorizontalGroup("IconAndStats", 75)]
    public Sprite icon;

    [VerticalGroup("IconAndStats/Stats")]
    [LabelText("Mana Cost")]
    //[Range(0, 100)]
    public int manaCost;

    [VerticalGroup("IconAndStats/Stats")]
    [LabelText("Value / Power")]
    //[Range(1, 999)]
    public int value;

    [VerticalGroup("IconAndStats/Stats")]
    //[Range(0.1f, 100f)]
    public float cooldown;

    [VerticalGroup("IconAndStats/Stats")]
    //[Range(1, 10)]
    public int maxLevel;

    [Title("References")]
    [LabelText("Skill Effect Prefab")]
    public GameObject prefab;

    [Title("Description")]
    public string descriptionTemplate;

    public float duration;
}