using UnityEngine;
using Object = UnityEngine.Object;

public partial class EnemyAI
{
    protected virtual void MoveToAttackPosition()
    {
        if (target == null || anim == null)
            return;

        Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;
        float distance = toTarget.magnitude;
        const float attackBuffer = 0.3f;

        if (distance <= attackRange + attackBuffer)
        {
            StopMotion();
            if (Time.time - lastAttackTime >= attackCooldown)
                AttackTarget();
            return;
        }

        // Di chuyển thẳng về phía target — không bị kẹt do logic trục X/Y
        MoveInDirection(toTarget.normalized);
        RotateEnemy(toTarget.x);
    }

    protected void RotateEnemy(float direction)
    {
        if (Mathf.Approximately(direction, 0f))
            return;

        transform.localScale = new Vector3(Mathf.Sign(direction) * -1f, 1f, 1f);
    }

    private Transform _attackSnapshot;

    protected virtual void AttackTarget()
    {
        if (target == null || isTakingDamage || isDead || anim == null)
            return;

        RotateEnemy(target.position.x - transform.position.x);

        if (Time.time - lastAttackTime < attackCooldown)
            return;

        _attackSnapshot = target; // lưu target tại thời điểm đánh
        anim.SetBool(MoveBool, false);
        anim.SetTrigger(isHoldingSpear ? LongAttack : AttackTrigger);
        lastAttackTime = Time.time;
    }

    public void DealDamageToTarget()
    {
        if (isDead) return;

        // Dùng snapshot — target đã được validate lúc AttackTarget() gọi
        // Không check distance lại vì enemy có thể dịch chuyển trong lúc animation play
        Transform attackTarget = _attackSnapshot != null ? _attackSnapshot : target;

        if (attackTarget == null || !attackTarget.gameObject.activeInHierarchy)
            return;

        if (attackTarget.TryGetComponent(out IDamageable damageable))
            damageable.TakeDamage(attackDamage);
    }

    protected void RegisterRangedPressure()
    {
        if (Time.time - lastRangedHitTime > rangedHitWindow)
            rangedHitCount = 0;

        rangedHitCount++;
        lastRangedHitTime = Time.time;

        if (rangedHitCount >= rangedHitThreshold)
            isUnderRangedPressure = true;
    }

    public void SpawnBloodVFX()
    {
        if (cachedCollider == null || ObjectPooler.Instance == null || CommonReferent.Instance?.bloodVfxPrefab == null)
            return;

        Vector3 basePosition = cachedCollider.bounds.center;
        Vector3 flippedOffset = bloodVFXOffset;
        flippedOffset.x *= Mathf.Sign(transform.localScale.x);
        Vector3 vfxSpawnPos = basePosition + flippedOffset;

        GameObject vfx = ObjectPooler.Instance.Get(
            CommonReferent.Instance.bloodVfxPrefab.name,
            CommonReferent.Instance.bloodVfxPrefab,
            vfxSpawnPos,
            Quaternion.identity);

        if (vfx == null)
            return;

        Vector3 scale = vfx.transform.localScale;
        scale.x = Mathf.Sign(transform.localScale.x) * Mathf.Abs(scale.x);
        vfx.transform.localScale = scale;
    }

    private void EndDamageStun()
    {
        if (isDead)
            return;

        isTakingDamage = false;
    }

    public virtual void TakeDamage(int damage, bool isCrit = false)
    {
        if (isDead)
            return;

        EnemyInfoPopupUI.Instance?.Refresh();

        currentHealth -= damage;
        enemyHealthUI?.UpdateHealth(currentHealth);
        ShowUIOnHit();

        RegisterRangedPressure();
        isAggro = true;

        if (!skipHurtAnimation && anim != null)
            anim.SetTrigger(DamagedTrigger);

        isTakingDamage = true;

        string damageText = isCrit ? $"CRIT -{damage}" : $"-{damage}";
        Color damageColor = isCrit ? new Color(1f, 0.84f, 0.2f) : Color.white;
        FloatingTextSpawner.Instance?.SpawnText(damageText, transform.position + Vector3.up * 1.2f, damageColor);

        SpawnBloodVFX();

        CancelInvoke(nameof(EndDamageStun));

        if (currentHealth <= 0)
        {
            // Reset toàn bộ trigger tấn công trong queue
            // Nếu không reset, Animator sẽ vẫn play attack animation dù isDead = true
            anim?.ResetTrigger(AttackTrigger);
            anim?.ResetTrigger(LongAttack);
            anim?.ResetTrigger(DamagedTrigger);
            Die();
        }
        else
        {
            Invoke(nameof(EndDamageStun), damagedStunTime);
        }

        lastAggroTime = Time.time;
        SetAggroIcon(true);
    }
}