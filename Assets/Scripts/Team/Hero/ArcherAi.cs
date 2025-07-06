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

        if (target.TryGetComponent(out EnemyAI enemy) && !enemy.IsDead)
        {
            GameObject arrowObj = ObjectPooler.Instance.Get(
                RefVFX.Instance.arrowPrefab.name,
                RefVFX.Instance.arrowPrefab,
                shootPoint.position,
                Quaternion.identity
            );

            Arrow arrow = arrowObj.GetComponent<Arrow>();
            if (arrow != null)
            {
                arrow.SetTarget(target, stats.Attack);
            }

        }
        else if (target.TryGetComponent(out DestructibleObject destructible))
        {
            destructible.Hit();
        }
    }
}