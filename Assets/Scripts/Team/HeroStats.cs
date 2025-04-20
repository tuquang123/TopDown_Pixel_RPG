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

    public HeroStats(int hp, int atk, int def, int spd)
    {
        maxHP = hp;
        attack = atk;
        defense = def;
        speed = spd;
    }
}
