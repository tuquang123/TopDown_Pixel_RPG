using UnityEngine;

public class RangedEnemyAI : EnemyAI
{
    [Header("Ranged Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 5f;

    protected override void AttackTarget()
    {
        if (target.TryGetComponent(out PlayerStats playerStats))
        {
            if(playerStats.isDead) return;
        }
        if (isTakingDamage) return;
        
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            anim.SetBool(MoveBool, false);
            anim.SetTrigger(AttackTrigger);
            lastAttackTime = Time.time;
            ShootProjectile();
        }
    }
    protected override void MoveToAttackPosition()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (distanceToPlayer > detectionRange)
        {
            // Ngoài phạm vi phát hiện -> không làm gì cả
            anim.SetBool(MoveBool, false);
            return;
        }

        RotateEnemy(target.position.x - transform.position.x);

        if (distanceToPlayer > attackRange)
        {
            // Trong detection nhưng chưa tới attack range -> di chuyển lại gần
            Vector2 direction = (target.position - transform.position).normalized;
            transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
            anim.SetBool(MoveBool, true);
        }
        else
        {
            // Trong attack range -> dừng lại và bắn
            anim.SetBool(MoveBool, false);
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                AttackTarget();
            }
        }
    }

    
    private void ShootProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        if (proj.TryGetComponent(out EnemyProjectile enemyProj))
        {
            enemyProj.speed = projectileSpeed;
            enemyProj.Init(target.position, attackDamage);
        }
    }
}