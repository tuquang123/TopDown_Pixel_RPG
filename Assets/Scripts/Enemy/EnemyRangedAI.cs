using UnityEngine;

public class EnemyRangedAI : EnemyAI
{
    [Header("Ranged Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;

    protected override void AttackTarget()
    {
        if (target == null || isTakingDamage) return;

        RotateEnemy(target.position.x - transform.position.x);

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            anim.SetBool(MoveBool, false);
            anim.SetTrigger(AttackTrigger);
            lastAttackTime = Time.time;
        }
    }
    
    protected override void MoveToAttackPosition()
    {
        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        if (distanceToTarget <= detectionRange)
        {
            anim.SetBool(MoveBool, false);

            if (distanceToTarget <= attackRange && Time.time - lastAttackTime >= attackCooldown)
            {
                AttackTarget();
            }
        }
        else
        {
            anim.SetBool(MoveBool, false);
        }
    }


    // Gọi từ animation event
    public void FireProjectile()
    {
        if (target == null) return;

        Vector2 dir = (target.position - firePoint.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        if (proj.TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = dir * projectileSpeed;
        }

        if (proj.TryGetComponent(out EnemyProjectile projectile))
        {
            projectile.Init(attackDamage, gameObject);
        }
    }
}