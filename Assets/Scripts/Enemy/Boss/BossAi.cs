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
        skipHurtAnimation = true;
    }
    
    private void Update()
    {
        if (isDead || isTakingDamage) return;

        if (player == null) return;
        if (Vector2.Distance(transform.position, player.position) > detectionRange) return;

        RotateEnemy(player.position.x - transform.position.x);

        if (Time.time - _lastSpecialAttackTime >= specialAttackCooldown)
        {
            PerformSpecialAttack();
            _lastSpecialAttackTime = Time.time;
        }
    }


    protected override void AttackPlayer()
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
