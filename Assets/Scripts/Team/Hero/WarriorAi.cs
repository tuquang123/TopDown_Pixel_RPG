using UnityEngine;

public class WarriorAI : AllyBaseAI
{
    private static readonly int AttackTrigger = Animator.StringToHash("8_Attack");

    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.8f;

    protected override void AttackTarget()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger(AttackTrigger);
    }

    // Gọi từ Animation Event
    public void ApplyAttackDamage()
    {
        AudioManager.Instance.PlaySFX("Attack");
        
        // Tìm enemy trong phạm vi đánh
        var enemies = EnemyTracker.Instance.GetEnemiesInRange(attackPoint.position, attackRadius);
        foreach (var enemy in enemies)
        {
            if (enemy == null || enemy.IsDead) continue;

            int damage = (int)stats.Attack;
            enemy.TakeDamage(damage);
        }

        // Tìm destructible
        var destructibles = DestructibleTracker.Instance.GetInRange(attackPoint.position, attackRadius);
        foreach (var destructible in destructibles)
        {
            destructible.Hit();
        }
    }
}