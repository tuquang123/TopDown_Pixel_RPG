using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsSO", menuName = "RPG/Player Stats")]
public class PlayerStatsSO : ScriptableObject
{
    [Header("Initial Values (used only when initializing runtime instance)")]
    [Tooltip("Chỉ dùng để khởi tạo PlayerStats runtime, không phải current level realtime.")]
    public int initialLevel = 1;

    [Tooltip("Chỉ dùng để khởi tạo runtime skill points.")]
    public int initialSkillPoints = 0;

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