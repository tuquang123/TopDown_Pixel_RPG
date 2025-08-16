using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsSO", menuName = "RPG/Player Stats")]
public class PlayerStatsSO : ScriptableObject
{
    [Header("Level & Points")]
    public int level = 1;
    public int skillPoints = 0;

    [Header("Base Stats")]
    public float maxHealth = 100;
    public float maxMana = 50;
    public float attack = 10;
    public float defense = 5;
    public float speed = 5;
    public float critChance = 0.1f;
    public float lifeSteal = 0f;
    public float attackSpeed = 1f;
}