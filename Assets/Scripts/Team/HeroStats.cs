public enum HeroRole
{
    DPS,
    Tank,
    Support
}

[System.Serializable]
public class HeroStats
{
    public int maxHP;
    public int attack;
    public int defense;
    public int speed;
    public float attackSpeed;

    public HeroStats(int hp, int atk, int def, int spd , int atts)
    {
        maxHP = hp;
        attack = atk;
        defense = def;
        speed = spd;
        attackSpeed = atts;
    }
}
