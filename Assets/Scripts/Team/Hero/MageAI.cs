using UnityEngine;

public class MageAI : AllyBaseAI
{
    private readonly int _castTrigger = Animator.StringToHash("mage");
    [SerializeField] private Transform castPoint;

    protected override void AttackTarget()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger(_castTrigger);

        if (target == null) return;

        if (target.TryGetComponent(out EnemyAI enemy) && !enemy.IsDead)
        {
            GameObject proj = ObjectPooler.Instance.Get(
                RefVFX.Instance.spellProjectilePrefab.name,
                RefVFX.Instance.spellProjectilePrefab,
                castPoint.position,
                Quaternion.identity
            );

            proj.GetComponent<Projectile>()?.SetTarget(target, stats.Attack);

        }
        else if (target.TryGetComponent(out DestructibleObject destructible))
        {
            destructible.Hit();
        }
    }
}