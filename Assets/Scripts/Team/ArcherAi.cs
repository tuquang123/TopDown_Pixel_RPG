using UnityEngine;

public class ArcherAI : AllyBaseAI
{
    private static readonly int ShotTrigger = Animator.StringToHash("7_Shoot");

    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint;

    protected override void AttackTarget()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger(ShotTrigger);

        if (target == null) return;

        if (target.TryGetComponent(out EnemyAI enemy) && !enemy.IsDead)
        {
            GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);
            arrow.GetComponent<Arrow>()?.SetTarget(target, stats.Attack);
        }
        else if (target.TryGetComponent(out DestructibleObject destructible))
        {
            destructible.Hit();
        }
    }
}