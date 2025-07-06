using System;
using UnityEngine;

public class BossAI : EnemyAI
{
    [Header("Boss Special Settings")]
    [SerializeField] private float specialAttackCooldown = 5f;
    [SerializeField] BossHealthUI bossHealthUI;
    private float _lastSpecialAttackTime;
    
    protected override void Start()
    {
        bossHealthUI = RefVFX.Instance.bossHealthUI;
        
        if (bossHealthUI == null)
        {
            bossHealthUI = FindFirstObjectByType<BossHealthUI>(); 
            if (bossHealthUI == null)
            {
                Debug.LogWarning("Không tìm thấy BossHealthUI trong scene!");
            }
        }
        
        base.Start();
        bossHealthUI?.SetMaxHealth(maxHealth);
        skipHurtAnimation = true;
    }
    
    private void Update()
    {
        if (isDead || isTakingDamage) return;

        if (target == null) return;
        if (Vector2.Distance(transform.position, target.position) > detectionRange) return;

        Rotate(target.position.x - transform.position.x);

        if (Time.time - _lastSpecialAttackTime >= specialAttackCooldown)
        {
            PerformSpecialAttack();
            _lastSpecialAttackTime = Time.time;
        }
    }


    protected override void AttackTarget()
    {
       
    }

    private void PerformSpecialAttack()
    {
        anim.SetTrigger(AttackTrigger);
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
