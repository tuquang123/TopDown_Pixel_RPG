using UnityEngine;

[System.Serializable]
public class PlayerStatsData
{
    public int level;
    public int skillPoints;
    public int currentHealth;
    public int currentMana;

    public float maxHealth;
    public float maxMana;
    public float attack;
    public float defense;
    public float speed;
    public float critChance;
    public float lifeSteal;
    public float attackSpeed;

    // Khởi tạo từ ScriptableObject
    public PlayerStatsData(PlayerStatsSO so)
    {
        level = so.level;
        skillPoints = so.skillPoints;

        maxHealth = so.maxHealth;
        maxMana = so.maxMana;
        attack = so.attack;
        defense = so.defense;
        speed = so.speed;
        critChance = so.critChance;
        lifeSteal = so.lifeSteal;
        attackSpeed = so.attackSpeed;

        currentHealth = Mathf.RoundToInt(maxHealth);
        currentMana = Mathf.RoundToInt(maxMana);
    }

    // Khởi tạo rỗng để JsonUtility hoạt động
    public PlayerStatsData() { }
}