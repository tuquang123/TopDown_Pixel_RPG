using UnityEngine;

public class EnemyRangedAI : EnemyAI
{
    [Header("Ranged Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;

    [Header("Weapon Type")]
    public bool isHoldingBow = false; // nếu true → bắn mũi tên

    protected override void AttackTarget()
    {
        if (target == null || isTakingDamage) return;

        RotateEnemy(target.position.x - transform.position.x);

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            anim.SetBool(MoveBool, false);

            // Kiểm tra cầm cung
            if (isHoldingBow)
            {
                anim.SetTrigger("7_Shoot"); // trigger mới, animation bắn cung
            }
            else
            {
                anim.SetTrigger(AttackTrigger); // trigger mặc định (ví dụ ném đá)
            }

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
