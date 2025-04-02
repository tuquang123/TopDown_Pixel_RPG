using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int level = 1;
    public int skillPoints = 0; // Điểm kỹ năng mỗi lần lên cấp

    // Các chỉ số cơ bản
    public Stat maxHealth = new Stat(100);
    public Stat maxMana = new Stat(50);
    public Stat attack = new Stat(10);
    public Stat defense = new Stat(5);
    public Stat speed = new Stat(5);

    // Giá trị hiện tại của HP và Mana
    private int currentHealth;
    public int currentMana;
    public int baseDamage => attack.Value; 

    public event Action OnStatsChanged; // Sự kiện khi stats thay đổi
    public int Mana { get; set; }

    private void Start()
    {
        currentHealth = maxHealth.Value;
        currentMana = maxMana.Value;
    }

    public void LevelUp()
    {
        level++;
        skillPoints++;
        Debug.Log($"Lên cấp! Level: {level}, Điểm kỹ năng: {skillPoints}");
    }

    public void TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(damage - defense.Value, 1);
        currentHealth -= actualDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth.Value);
        Debug.Log($"Nhận {actualDamage} sát thương, HP còn: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth.Value);
        Debug.Log($"Hồi {amount} HP, HP hiện tại: {currentHealth}");
    }

    private void Die()
    {
        Debug.Log("Player đã chết!");
        // Thêm logic hồi sinh hoặc game over tại đây
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
        }

        OnStatsChanged?.Invoke();
    }

    public void UseMana(int skillManaCost)
    {
        throw new NotImplementedException();
    }
}
