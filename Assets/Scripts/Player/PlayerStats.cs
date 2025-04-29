using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerStats : MonoBehaviour
{
    [Title("Level & Skill Points")]
    [ReadOnly, ShowInInspector] public int level = 1;
    [ReadOnly, ShowInInspector] public int skillPoints = 0;

    [Title("Stats")]
    [BoxGroup("Stats")]
    public Stat maxHealth = new(100);

    [BoxGroup("Stats")]
    public Stat maxMana = new(50);

    [BoxGroup("Stats")]
    public Stat attack = new(10);

    [BoxGroup("Stats")]
    public Stat defense = new(5);

    [BoxGroup("Stats")]
    public Stat speed = new(5);

    [BoxGroup("Stats")]
    public Stat critChance = new(10); // %

    [BoxGroup("Stats")]
    public Stat lifeSteal = new(5); // %

    [BoxGroup("Stats")]
    public Stat attackSpeed = new(1f);

    [Title("Current Values (Runtime Only)")]
    [ReadOnly, ShowInInspector] private int currentHealth;
    [ReadOnly, ShowInInspector] public int currentMana;

    public event Action OnStatsChanged;
    public event Action OnHealthChanged;

    [Required] // Bắt buộc phải gán playerHp trên Inspector
    public PlayerHealth playerHp;

    private void Start()
    {
        currentHealth = (int)maxHealth.Value;
        currentMana = (int)maxMana.Value;
        OnHealthChanged?.Invoke();
    }

    [Button("Level Up")]
    public void LevelUp()
    {
        level++;
        skillPoints++;
        Debug.Log($"Lên cấp! Level: {level}, Điểm kỹ năng: {skillPoints}");
    }

    [Button("Take Test Damage (10)")]
    public void TakeTestDamage()
    {
        TakeDamage(10);
    }

    public void TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(damage - (int)defense.Value, 1);
        currentHealth -= actualDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, (int)maxHealth.Value);
        Debug.Log($"Nhận {actualDamage} sát thương, HP còn: {currentHealth}");
        OnHealthChanged?.Invoke();

        FloatingTextSpawner.Instance.SpawnText(
            $"-{damage}",
            transform.position + Vector3.up * 0.5f,
            Color.white);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    [Button("Heal 50 HP")]
    public void TestHeal()
    {
        Heal(50);
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, (int)maxHealth.Value);
        Debug.Log($"Hồi {amount} HP, HP hiện tại: {currentHealth}");
        OnHealthChanged?.Invoke();
    }

    private void Die()
    {
        Debug.Log("Player đã chết!");
        playerHp.Die();
    }

    public void ApplyStatModifier(StatModifier modifier)
    {
        switch (modifier.statType)
        {
            case StatType.MaxHealth: maxHealth.AddModifier(modifier); break;
            case StatType.MaxMana: maxMana.AddModifier(modifier); break;
            case StatType.Attack: attack.AddModifier(modifier); break;
            case StatType.Defense: defense.AddModifier(modifier); break;
            case StatType.Speed: speed.AddModifier(modifier); break;
            case StatType.CritChance: critChance.AddModifier(modifier); break;
            case StatType.LifeSteal: lifeSteal.AddModifier(modifier); break;
            case StatType.AttackSpeed: attackSpeed.AddModifier(modifier); break;
        }
        OnStatsChanged?.Invoke();
    }

    public void RemoveStatModifier(StatModifier modifier)
    {
        switch (modifier.statType)
        {
            case StatType.MaxHealth: maxHealth.RemoveModifier(modifier); break;
            case StatType.MaxMana: maxMana.RemoveModifier(modifier); break;
            case StatType.Attack: attack.RemoveModifier(modifier); break;
            case StatType.Defense: defense.RemoveModifier(modifier); break;
            case StatType.Speed: speed.RemoveModifier(modifier); break;
            case StatType.CritChance: critChance.RemoveModifier(modifier); break;
            case StatType.LifeSteal: lifeSteal.RemoveModifier(modifier); break;
            case StatType.AttackSpeed: attackSpeed.RemoveModifier(modifier); break;
        }
        OnStatsChanged?.Invoke();
    }

    [Button("Show Current Health")]
    public float GetCurrentHealth() => currentHealth;

    public float GetCritChance() => critChance.Value;

    public float GetLifeStealPercent() => lifeSteal.Value;

    public void HealFromLifeSteal(int damageDealt)
    {
        int healAmount = Mathf.RoundToInt(damageDealt * (lifeSteal.Value / 100f));
        if (healAmount > 0) Heal(healAmount);
    }
}
