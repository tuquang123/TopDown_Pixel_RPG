using UnityEngine;

[System.Serializable]
public class Hero
{
    public HeroData data;
    public int level;
    public int experience;
    public HeroStats currentStats;

    public Hero(HeroData data, int level)
    {
        this.data = data;
        this.level = level;
        this.experience = 0;
        currentStats = CalculateStats();
    }

    public void LevelUp()
    {
        level++;
        currentStats = CalculateStats();
        experience = 0;
        Debug.Log($"{data.name} đã lên cấp {level}");
    }

    private HeroStats CalculateStats()
    {
        int hp = data.baseHP;
        int atk = data.baseAttack ;
        int def = data.baseDefense;
        int spd = data.baseSpeed;
        int atts = data.attackSpeed;
        return new HeroStats(hp, atk, def, spd , atts);
    }

    public int CalculateDamage(Hero target)
    {
        int rawDamage = currentStats.attack - target.currentStats.defense;
        return Mathf.Max(rawDamage, 1);
    }
}