using UnityEngine;

[CreateAssetMenu(fileName = "HeroData", menuName = "Game/Hero Data")]
public class HeroData : ScriptableObject
{
    public string id;
    public string name;
    public HeroRole role;
    public Sprite icon;

    [Header("Base Stats")]
    public int baseHP;
    public int baseAttack;
    public int baseDefense;
    public int baseSpeed;
    public int attackSpeed;
    
    [Header("Skill System")]
    public AllySkillData passiveSkill;
    public AllySkillData activeSkill1;
    public AllySkillData activeSkill2;
}