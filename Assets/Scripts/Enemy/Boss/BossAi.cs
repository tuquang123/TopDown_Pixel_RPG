using UnityEngine;

public class BossAI : EnemyAI
{
    [Header("Boss Special Settings")]
    [SerializeField] private float specialAttackCooldown = 5f;
    [SerializeField] BossHealthUI bossHealthUI;
    private float _lastSpecialAttackTime;
    
    protected override void Start()
    {
        base.Start();
        bossHealthUI?.SetMaxHealth(maxHealth);
    }

    protected override void AttackPlayer()
    {
        base.AttackPlayer(); 
        
        if (Time.time - _lastSpecialAttackTime >= specialAttackCooldown)
        {
            PerformSpecialAttack();
            _lastSpecialAttackTime = Time.time;
        }
    }

    private void PerformSpecialAttack()
    {
       
    }
    public override void TakeDamage(int damage, bool isCrit = false)
    {
        base.TakeDamage(damage, isCrit);
        bossHealthUI?.UpdateHealth(currentHealth);
    }

    protected override void Die()
    {
        base.Die();
        bossHealthUI?.Hide();
    }

}
