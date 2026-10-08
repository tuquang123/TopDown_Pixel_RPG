using System.Collections;
using UnityEngine;

public class EnemyRangedAI : EnemyAI
{
    [Header("Ranged Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float chaseSpeed = 2f; // tốc độ chạy đến nhân vật

    [Header("Weapon Type")]
    public bool isHoldingBow = false; // nếu true → bắn mũi tên

    protected override void MoveToAttackPosition()
    {
        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.position);

        // Ngoài tầm tìm đến → đứng yên
        if (distance > detectionRange)
        {
            anim.SetBool(MoveBool, false);
            return;
        }

        // Trong tầm tìm đến nhưng chưa tới tầm đánh → chạy lại gần
        if (distance > attackRange)
        {
            if (isTakingDamage)
            {
                anim.SetBool(MoveBool, false);
                return;
            }

            RotateEnemy(target.position.x - transform.position.x);
            anim.SetBool(MoveBool, true);

            transform.position = Vector2.MoveTowards(
                transform.position,
                target.position,
                chaseSpeed * Time.deltaTime
            );
            return;
        }

        // Trong tầm đánh → đứng lại và bắn
        anim.SetBool(MoveBool, false);

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            AttackTarget();
        }
    }

    protected override void AttackTarget()
    {
        if (target == null || isTakingDamage) return;

        RotateEnemy(target.position.x - transform.position.x);

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            anim.SetBool(MoveBool, false);

            if (isHoldingBow)
            {
                anim.SetTrigger("7_Shoot"); // animation bắn cung
            }
            else
            {
                anim.SetTrigger(AttackTrigger); // trigger mặc định (ví dụ ném đá)
            }

            lastAttackTime = Time.time;
        }
    }

    // Gọi từ Animation Event
    public void FireProjectile()
    {
        if (target == null || firePoint == null || projectilePrefab == null) return;

        Vector2 dir = (target.position - firePoint.position).normalized;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0, 0, angle);

        if (proj.TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = dir * projectileSpeed;
        }

        if (proj.TryGetComponent(out EnemyProjectile projectile))
        {
            projectile.Init(attackDamage, gameObject, criticalChance, criticalDamageMultiplier);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}