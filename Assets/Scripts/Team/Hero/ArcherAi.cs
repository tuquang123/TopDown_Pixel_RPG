using UnityEngine;

public class ArcherAI : AllyBaseAI
{
    private static readonly int ShotTrigger = Animator.StringToHash("7_Shoot");

    [SerializeField] private Transform shootPoint;

    protected override void AttackTarget()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger(ShotTrigger);

        if (target == null) return;

        GameObject arrowObj = ObjectPooler.Instance.Get(
            RefVFX.Instance.arrowPrefab.name,
            RefVFX.Instance.arrowPrefab,
            shootPoint.position,
            Quaternion.identity
        );

        Arrow arrow = arrowObj.GetComponent<Arrow>();
        if (arrow == null) return;

        // Damage truyền vào tùy loại target
        int damage = stats.Attack;
        arrow.SetTarget(target, damage);
    }
}