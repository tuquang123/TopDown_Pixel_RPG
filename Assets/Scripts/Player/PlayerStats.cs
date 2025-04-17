using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int level = 1;
    public int skillPoints = 0;

    public Stat maxHealth = new(100);
    public Stat maxMana = new(50);
    public Stat attack = new(10);
    public Stat defense = new(5);
    public Stat speed = new(5);
    public Stat critChance = new(10);    // phần trăm, ví dụ 10 nghĩa là 10%
    public Stat lifeSteal = new(5);      // phần trăm hút máu
    public Stat attackSpeed = new(1f);

    private int currentHealth;
    public int currentMana;

    public event Action OnStatsChanged;
    public event Action OnHealthChanged;

    public PlayerHealth playerHp;

    private void Start()
    {
        currentHealth = (int)maxHealth.Value;
        currentMana = (int)maxMana.Value;
        OnHealthChanged?.Invoke();
    }

    public void LevelUp()
    {
        level++;
        skillPoints++;
        Debug.Log($"Lên cấp! Level: {level}, Điểm kỹ năng: {skillPoints}");
    }

    public void TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(damage - (int)defense.Value, 1);
        currentHealth -= actualDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, (int)maxHealth.Value);
        Debug.Log($"Nhận {actualDamage} sát thương, HP còn: {currentHealth}");
        OnHealthChanged?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
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
            case StatType.AttackSpeed: lifeSteal.AddModifier(modifier); break;
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
            case StatType.AttackSpeed: lifeSteal.RemoveModifier(modifier); break;
        }
        OnStatsChanged?.Invoke();
    }

    public float GetCurrentHealth() => currentHealth;

    public float GetCritChance() => critChance.Value;

    public float GetLifeStealPercent() => lifeSteal.Value;

    public void HealFromLifeSteal(int damageDealt)
    {
        int healAmount = Mathf.RoundToInt(damageDealt * (lifeSteal.Value / 100f));
        if (healAmount > 0) Heal(healAmount);
    }
}
