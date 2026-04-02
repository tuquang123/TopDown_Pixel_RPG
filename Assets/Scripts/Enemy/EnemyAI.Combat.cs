using UnityEngine;
using Object = UnityEngine.Object;

public partial class EnemyAI
{
    protected virtual void MoveToAttackPosition()
    {
        if (target == null || anim == null)
            return;

        Vector2 enemyPos = transform.position;
        Vector2 targetPos = target.position;

        float yDiff = Mathf.Abs(enemyPos.y - targetPos.y);
        float xDiff = Mathf.Abs(enemyPos.x - targetPos.x);

        if (yDiff > 0.1f)
        {
            Vector2 directionY = new Vector2(0f, targetPos.y - enemyPos.y).normalized;
            MoveInDirection(directionY);
        }
        else if (xDiff > attackRange * 0.8f)
        {
            Vector2 directionX = new Vector2(targetPos.x - enemyPos.x, 0f).normalized;
            MoveInDirection(directionX);
            RotateEnemy(directionX.x);
        }
        else
        {
            StopMotion();
            if (Time.time - lastAttackTime >= attackCooldown)
                AttackTarget();
        }
    }

    protected void RotateEnemy(float direction)
    {
        if (Mathf.Approximately(direction, 0f))
            return;

        transform.localScale = new Vector3(Mathf.Sign(direction) * -1f, 1f, 1f);
    }

    protected virtual void AttackTarget()
    {
        if (target == null || isTakingDamage || isDead || anim == null)
            return;

        RotateEnemy(target.position.x - transform.position.x);

        if (Time.time - lastAttackTime < attackCooldown)
            return;

        anim.SetBool(MoveBool, false);
        anim.SetTrigger(isHoldingSpear ? LongAttack : AttackTrigger);
        lastAttackTime = Time.time;
    }

    public void DealDamageToTarget()
    {
        if (isDead || target == null || !target.gameObject.activeInHierarchy)
            return;

        if (Vector2.Distance(transform.position, target.position) > attackRange)
            return;

        if (target.TryGetComponent(out IDamageable damageable))
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
