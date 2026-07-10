using System.Collections;
using UnityEngine;

public class EnemyRangedAI : EnemyAI
{
    [Header("Ranged Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;

    [Header("Weapon Type")]
    public bool isHoldingBow = false; // nếu true → bắn mũi tên

    protected override void MoveToAttackPosition()
    {
        float distance = Vector2.Distance(transform.position, target.position);

        // trong detection → đứng lại
        if (distance <= detectionRange)
        {
            anim.SetBool(MoveBool, false);

            // nếu trong attack range → bắn
            if (distance <= attackRange && Time.time - lastAttackTime >= attackCooldown)
            {
                AttackTarget();
            }
        }
        else
        {
            anim.SetBool(MoveBool, false);
        }
    }

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

  
    public void FireProjectile()
    {
        if (target == null) return;

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
}
