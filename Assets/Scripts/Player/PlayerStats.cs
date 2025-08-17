using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerStats : Singleton<PlayerStats>, IGameEventListener , IDamageable , IBuffableStats
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
    [SerializeField] private PlayerStatsSO baseStatsSO;

    public PlayerStatsData RuntimeStats { get; private set; }
    
    private void Awake()
    {
        base.Awake(); 
        /*if (baseStatsSO != null)
        {
            ResetFromSO(baseStatsSO);
        }*/
    }
    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        currentHealth = (int)maxHealth.Value;
        currentMana = (int)maxMana.Value;

        LoadSkillLevels();
        OnHealthChanged?.Invoke();
        OnManaChanged?.Invoke();
    }
    
    private void OnApplicationQuit()
    {
        SaveSkillLevels();
    }

    private void OnDestroy()
    {
        SaveSkillLevels();
    }
    
    private void ResetFromSO(PlayerStatsSO so)
    {
        level = so.level;
        skillPoints = so.skillPoints;

        maxHealth = new Stat(so.maxHealth);
        maxMana = new Stat(so.maxMana);
        attack = new Stat(so.attack);
        defense = new Stat(so.defense);
        speed = new Stat(so.speed);
        critChance = new Stat(so.critChance);
        lifeSteal = new Stat(so.lifeSteal);
        attackSpeed = new Stat(so.attackSpeed);

        currentHealth = (int)so.maxHealth;
        currentMana = (int)so.maxMana;
    }
    
    public void OnEventRaised()
    {
        Debug.Log("Người chơi đã dùng ngựa.");
        anim = GetComponentInChildren<Animator>();
    }

    private void OnEnable() => GameEvents.OnUpdateAnimation.RegisterListener(this);
    private void OnDisable() => GameEvents.OnUpdateAnimation.UnregisterListener(this);
    
    public void TakeDamage(int damage , bool isCrit = false)
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
        float stunTime = 0.12f;
        GetComponent<PlayerController>().enabled = false;

        yield return new WaitForSeconds(stunTime);

        GetComponent<PlayerController>().enabled = true;
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
        currentMana = (int)maxMana.Value; 
        OnHealthChanged?.Invoke();
        OnManaChanged?.Invoke();
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
    
    private IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(1.5f); 
    
        GameEvents.OnPlayerDied.Raise(); 
        
        LevelManager.Instance.LoadSpecificLevel(0, LevelManager.TravelDirection.Default);
        
        yield return new WaitForSeconds(0.1f); 
        Revive(); 
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
    
    public void ApplyTemporaryBuff(StatModifier modifier, float duration)
    {
        StartCoroutine(TemporaryStatModifierRoutine(modifier, duration));
    }

    private IEnumerator TemporaryStatModifierRoutine(StatModifier modifier, float duration)
    {
        ApplyStatModifier(modifier);

        yield return new WaitForSeconds(duration);

        RemoveStatModifier(modifier);
    }
    public void ModifyAttack(float amount, float duration)
    {
        StatModifier mod = new StatModifier(StatType.Attack, amount);
        ApplyTemporaryBuff(mod, duration);
    }

    public void ModifyDefense(float amount, float duration)
    {
        StatModifier mod = new StatModifier(StatType.Defense, amount);
        ApplyTemporaryBuff(mod, duration);
    }

    public void ModifySpeed(float amount, float duration)
    {
        StatModifier mod = new StatModifier(StatType.Speed, amount);
        ApplyTemporaryBuff(mod, duration);
    }
    

    
}
