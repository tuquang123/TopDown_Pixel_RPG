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

        GameObject proj = ObjectPooler.Instance.Get(
            CommonReferent.Instance.spellProjectilePrefab.name,
            CommonReferent.Instance.spellProjectilePrefab,
            castPoint.position,
            Quaternion.identity
        );

        proj.GetComponent<Projectile>()?.SetTarget(target, stats.Attack);
    }
}