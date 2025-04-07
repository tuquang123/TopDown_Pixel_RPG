using System;
using UnityEngine;

public enum SkillType { Passive, Active }
public enum SkillID { DamageBoost, ShurikenThrow,
    None
}

[Serializable]
[CreateAssetMenu(fileName = "New Skill", menuName = "Skill System/Skill Data")]
public class SkillData : ScriptableObject
{
    public SkillID skillID;
    public string skillName;
    public SkillType skillType;
    public int requiredLevel;
    public int manaCost;
    public int value;
    public float cooldown;
    public GameObject prefab;
    public Sprite icon;
}