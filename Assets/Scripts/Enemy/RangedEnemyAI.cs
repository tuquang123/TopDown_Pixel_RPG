using UnityEngine;

public class RangedEnemyAI : EnemyAI
{
    [Header("Ranged Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 5f;

    protected override void AttackPlayer()
    {
        if (isTakingDamage) return;

        FacePlayer();

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            anim.SetBool(MoveBool, false);
            anim.SetTrigger(AttackTrigger);
            lastAttackTime = Time.time;
            ShootProjectile();
        }
    }

    private void ShootProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        if (proj.TryGetComponent(out EnemyProjectile enemyProj))
        {
            enemyProj.speed = projectileSpeed;
            enemyProj.Init(player.position, attackDamage);
        }
    }

    private void FacePlayer()
    {
        if (player == null) return;

        if (player.position.x > transform.position.x)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            transform.rotation = Quaternion.Euler(0, 180, 0);
    }
}