using UnityEngine;

public class WarriorAI : AllyBaseAI
{
    private static readonly int AttackTrigger = Animator.StringToHash("8_Attack");

    protected override void AttackTarget()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger(AttackTrigger);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Enemy", "Destructible"));

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out EnemyAI enemy) && !enemy.IsDead)
            {
                enemy.TakeDamage(stats.Attack);
            }
            else if (hit.TryGetComponent(out DestructibleObject destructible))
            {
                destructible.Hit();
            }
        }
    }
}