using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyData",
    menuName = "GameData/Enemy"
)]
public class EnemyData : ScriptableObject
{
    [Header("Info")]
    public string enemyId;
    public string enemyName;

    [Header("Stats")]
    public int maxHp;
    public int attack;
    public float moveSpeed;

    [Header("Combat")]
    public float attackRange;
    public float attackCooldown;

    [Header("Reward")]
    public int exp;
    public int gold;
}