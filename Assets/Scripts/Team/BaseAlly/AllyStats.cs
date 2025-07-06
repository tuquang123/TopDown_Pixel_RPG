using System.Collections;
using UnityEngine;

public class AllyStats : MonoBehaviour , IDamageable , IBuffableStats
{
    private static readonly int DeadTrigger = Animator.StringToHash("4_Death");
    private static readonly int HurtTrigger = Animator.StringToHash("3_Damaged");

    private int maxHP;
    private int currentHP;
    private int armor;
    private int attack;
    private float attackSpeed;
    private float moveSpeed;
    private string nameHero;

    public bool IsDead => currentHP <= 0;
    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
    public int Armor => armor;
    public int Attack => attack;
    public float AttackSpeed => attackSpeed;
    public float MoveSpeed => moveSpeed;
    public string HeroName => nameHero;
    public EnemyHealthUI HealthUI
    {
        get => healthUI;
        set => healthUI = value;
    }

    private Animator anim;
    private EnemyHealthUI healthUI; 

    private void Start()
    {
        currentHP = maxHP;
        anim = GetComponentInChildren<Animator>();
    }

    public void Initialize(HeroStats data, string nameHero)
    {
        maxHP = data.maxHP;
        currentHP = maxHP;
        armor = data.defense;
        attack = data.attack;
        attackSpeed = data.attackSpeed;
        moveSpeed = data.speed;
        this.nameHero = nameHero;

        healthUI?.UpdateHealth(currentHP);
    }

    public void TakeDamage(int damage , bool isCrit = false)
    {
        int finalDamage = Mathf.Max(damage - armor, 1);
        currentHP -= finalDamage;
        anim?.SetTrigger(HurtTrigger);

        healthUI?.UpdateHealth(currentHP);

        Debug.Log($"{gameObject.name} nhận {finalDamage} sát thương. HP: {currentHP}");

        if (IsDead)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} đã chết.");
        anim?.SetTrigger(DeadTrigger);
        gameObject.SetActive(false);
    }
    public void ModifyAttack(float amount, float duration)
    {
        int intAmount = Mathf.RoundToInt(amount);  
        StartCoroutine(TemporaryStatBoost(() => attack += intAmount, () => attack -= intAmount, duration));
    }

    public void ModifyDefense(float amount, float duration)
    {
        int intAmount = Mathf.RoundToInt(amount);
        StartCoroutine(TemporaryStatBoost(() => armor += intAmount, () => armor -= intAmount, duration));
    }


    public void ModifySpeed(float amount, float duration)
    {
        StartCoroutine(TemporaryStatBoost(() => moveSpeed += amount, () => moveSpeed -= amount, duration));
    }

    private IEnumerator TemporaryStatBoost(System.Action apply, System.Action revert, float duration)
    {
        apply.Invoke();
        yield return new WaitForSeconds(duration);
        revert.Invoke();
    }
}