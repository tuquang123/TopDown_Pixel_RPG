using System;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

public class PlayerStats : Singleton<PlayerStats> 
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
    
    private static readonly int DeathHash = Animator.StringToHash("4_Death");
    private Animator anim;
    [ReadOnly, ShowInInspector] public bool isDead { get; private set; }


    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
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
        if (isDead) return;
        
        int actualDamage = Mathf.Max(damage - (int)defense.Value, 1);
        currentHealth -= actualDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, (int)maxHealth.Value);
        //.Log($"Nhận {actualDamage} sát thương, HP còn: {currentHealth}");
        OnHealthChanged?.Invoke();

        FloatingTextSpawner.Instance.SpawnText(
            $"-{actualDamage}",
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
    
    public void Revive()
    {
        isDead = false;
        currentHealth = (int)maxHealth.Value;
        OnHealthChanged?.Invoke();
        anim.Rebind();
    }

    
    public void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Player đã chết!");
        anim.SetTrigger(DeathHash);
        StartCoroutine(HandleDeath());
    }
    
    private System.Collections.IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(.5f); 
        GameEvents.OnPlayerDied.Raise(); 
        GameEvents.OnResetGame.Raise();
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
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
