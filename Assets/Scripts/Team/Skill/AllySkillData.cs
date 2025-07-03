using UnityEngine;

public enum AllySkillType { Passive, Active }

[CreateAssetMenu(fileName = "New Ally Skill", menuName = "Skill System/Ally Skill")]
public class AllySkillData : ScriptableObject
{
    public string skillName;
    public AllySkillType type;
    public Sprite icon;
    [TextArea] public string description;

    public float value;
    public float cooldown;
    public GameObject effectPrefab;
    
    public SkillEffectType effectType; 

    public int level;
}
public enum SkillEffectType
{
    Damage,
    Heal,
    Buff
}

