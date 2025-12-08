using System.Collections;
using UnityEngine;

public class EnemyRangedAI : EnemyAI
{
    [Header("Ranged Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;
    
    [Header("Retreat Settings (Ranged Only)")]
    [SerializeField] private float retreatDistance = 2.5f;
    [SerializeField] private float retreatSpeed = 4f;
    [SerializeField] private float retreatCooldown = 5f;

    private float lastRetreatTime = -999f;
    private bool isRetreating = false;


    [Header("Weapon Type")]
    public bool isHoldingBow = false; // nếu true → bắn mũi tên
    
    public override void TakeDamage(int damage, bool isCrit)
    {
        base.TakeDamage(damage);  // vẫn xử lý máu, UI, animation

        TryRetreat(CommonReferent.Instance.playerPrefab.transform);               // thêm cơ chế né riêng ranged
    }
    
    private void TryRetreat(Transform attacker)
    {
        if (Time.time < lastRetreatTime + retreatCooldown)
            return;

        if (isRetreating)
            return;

        StartCoroutine(RetreatFrom(attacker));
    }
    
    private IEnumerator RetreatFrom(Transform attacker)
    {
        isRetreating = true;
        lastRetreatTime = Time.time;

        anim.SetBool(MoveBool, true);

        Vector3 dir = (transform.position - attacker.position).normalized;
        Vector3 targetPos = transform.position + dir * retreatDistance;

        float duration = retreatDistance / retreatSpeed;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(transform.position, targetPos, t);
            yield return null;
        }

        isRetreating = false;
    }


    protected override void MoveToAttackPosition()
    {
        if (isRetreating)
        {
            anim.SetBool(MoveBool, true);
            return;        
        }

        // Logic bắn giữ khoảng cách của ranged 
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

    // Gọi từ animation event
    public void FireProjectile()
    {
        if (target == null) return;

        Vector2 dir = (target.position - firePoint.position).normalized;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // ==========================
        // 1. Tính góc xoay
        // ==========================
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Nếu sprite mũi tên đang hướng sang phải → giữ nguyên
        proj.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Nếu sprite hướng lên → cần thêm offset -90°:
        // proj.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        // ==========================
        // 2. Set vận tốc
        // ==========================
        if (proj.TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = dir * projectileSpeed;
        }

        // ==========================
        // 3. Damage
        // ==========================
        if (proj.TryGetComponent(out EnemyProjectile projectile))
        {
            projectile.Init(attackDamage, gameObject);
        }
    }

}
