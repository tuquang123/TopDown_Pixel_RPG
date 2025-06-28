using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerStats : Singleton<PlayerStats>, IGameEventListener
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
    public event Action OnManaChanged;
    
    private static readonly int DeathHash = Animator.StringToHash("4_Death");
    private static readonly int HurtAnm = Animator.StringToHash("3_Damaged");
    
    private Animator anim;
    public bool isInvincible;
    public bool isUsingSkill;
    [ReadOnly, ShowInInspector] public bool isDead { get; private set; }


    private void Start()
    {
        LoadSkillLevels();
        anim = GetComponentInChildren<Animator>();
        currentHealth = (int)maxHealth.Value;
        currentMana = (int)maxMana.Value;
        OnHealthChanged?.Invoke();
        OnManaChanged?.Invoke();
    }
    
    public void OnEventRaised()
    {
        Debug.Log("Người chơi đã dùng ngựa.");
        anim = GetComponentInChildren<Animator>();
    }

    private void OnEnable() => GameEvents.OnUpdateAnimation.RegisterListener(this);
    private void OnDisable() => GameEvents.OnUpdateAnimation.UnregisterListener(this);

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

        if (isInvincible)
        {
            actualDamage = 0;
        }

        currentHealth -= actualDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, (int)maxHealth.Value);

        GetComponentInChildren<Animator>().SetTrigger(HurtAnm);
        StartCoroutine(HurtEffect());

        OnHealthChanged?.Invoke();

        FloatingTextSpawner.Instance.SpawnText(
            $"-{actualDamage}",
            transform.position + Vector3.up * 0.85f,
            actualDamage == 0 ? Color.yellow : Color.white);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HurtEffect()
    {
        // Có thể thêm logic như flinch, disable movement tạm thời...
        float stunTime = 0.12f;
        GetComponent<PlayerController>().enabled = false;

        yield return new WaitForSeconds(stunTime);

        GetComponent<PlayerController>().enabled = true;
    }


    [Button("Heal 50 HP")]
    public void TestHeal()
    {
        Heal(50);
    }
    
    public void UseMana(int amount)
    {
        currentMana = Mathf.Clamp(currentMana - amount, 0, (int)maxMana.Value);
        OnManaChanged?.Invoke(); 
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
        yield return new WaitForSeconds(1.5f); 
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
    
    public int HealFromLifeSteal(int damageDealt)
    {
        int healAmount = Mathf.RoundToInt(damageDealt * (lifeSteal.Value / 100f));
        if (healAmount > 0) Heal(healAmount);
        return healAmount;
    }
    
    private Dictionary<SkillID, int> skillLevels = new ();

    public int GetSkillLevel(SkillID skillID)
    {
        return skillLevels.GetValueOrDefault(skillID, 1);
    }

    public void SetSkillLevel(SkillID skillID, int level)
    {
        skillLevels[skillID] = level;
        SaveSkillLevels();
    }
    public void SaveSkillLevels()
    {
        foreach (var pair in skillLevels)
        {
            PlayerPrefs.SetInt($"SkillLevel_{pair.Key}", pair.Value);
        }
        PlayerPrefs.Save();
    }

    public void LoadSkillLevels()
    {
        skillLevels.Clear();
        foreach (SkillID skillID in System.Enum.GetValues(typeof(SkillID)))
        {
            if (PlayerPrefs.HasKey($"SkillLevel_{skillID}"))
            {
                skillLevels[skillID] = PlayerPrefs.GetInt($"SkillLevel_{skillID}");
            }
        }
    }
    private void OnApplicationQuit()
    {
        SaveSkillLevels();
    }

    private void OnDestroy()
    {
        SaveSkillLevels();
    }

}
