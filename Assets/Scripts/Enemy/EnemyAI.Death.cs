using System.Collections;
using UnityEngine;

public partial class EnemyAI
{
    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;
        target = null;
        CancelInvoke();
        SetSelected(false);
        StopMotion(disablePhysics: true);
        HandleDieAnimation();
        DisableComponents();
        HandleHealthUI();
        NotifySystemsBeforeDrop();
        HandleDrops();
        NotifySystemsAfterDrop();
        StartCoroutine(DelayDestroy());
    }

    private IEnumerator DelayDestroy()
    {
        yield return new WaitForSeconds(timeDieDelay);

        if (anim != null)
            anim.enabled = false;

        Destroy(gameObject);
    }

    protected virtual void HandleDieAnimation()
    {
        if (anim == null)
            return;

        anim.ResetTrigger(AttackTrigger);
        anim.ResetTrigger(LongAttack);
        anim.ResetTrigger(DamagedTrigger);
        anim.SetBool(MoveBool, false);
        anim.SetTrigger(DieTrigger);
    }

    protected virtual void DisableComponents()
    {
        if (cachedCollider != null)
            cachedCollider.enabled = false;

        enabled = false;
    }

    protected virtual void HandleHealthUI()
    {
        if (enemyHealthUI != null)
        {
            Destroy(enemyHealthUI.gameObject);
            enemyHealthUI = null;
        }

        if (infoUIInstance != null)
        {
            Destroy(infoUIInstance.gameObject);
            infoUIInstance = null;
        }
    }

    protected virtual void NotifySystemsBeforeDrop()
    {
        RaiseDeathEvent();
        QuestManager.Instance?.ReportProgress("NV2", enemyName, 1);
        QuestManager.Instance?.ReportProgress("NV4", enemyName, 1);
       
    }

    protected virtual void HandleDrops()
    {
        DropGold();
        DropItems();
    }

    protected virtual void DropGold()
    {
        if (CommonReferent.Instance?.goldPrefab == null)
            return;

        int goldAmount = UnityEngine.Random.Range(goldRange.x, goldRange.y + 1);
        if (goldAmount > 0)
            GoldDropHelper.SpawnGoldBurst(transform.position, goldAmount, CommonReferent.Instance.goldPrefab);
    }

    protected virtual void DropItems()
    {
        if (UnityEngine.Random.value > dropRate || dropItems.Count == 0 || CommonReferent.Instance?.itemDropPrefab == null || ObjectPooler.Instance == null)
            return;

        EnemyDropItem chosen = dropItems[UnityEngine.Random.Range(0, dropItems.Count)];
        if (chosen.item == null)
            return;

        GameObject prefab = CommonReferent.Instance.itemDropPrefab;
        GameObject dropObj = ObjectPooler.Instance.Get(prefab.name, prefab, transform.position, Quaternion.identity);
        dropObj?.GetComponent<ItemDrop>()?.Setup(new ItemInstance(chosen.item, 0), 1);
    }

    protected virtual void NotifySystemsAfterDrop()
    {
        OnEnemyDefeated?.Invoke(exp);
    }

    public void ResetEnemy()
    {
        CancelInvoke();
        EnsureCachedComponents();

        patrolWaitTimer = 0f;
        currentHealth = maxHealth;
        isDead = false;
        isTakingDamage = false;
        isKnockbacked = false;
        enabled = true;
        isAggro = false;
        isUnderRangedPressure = false;
        rangedHitCount = 0;
        target = null;
        lastAttackTime = 0f;
        lastRangedHitTime = -999f;
        lastAggroTime = 0f;
        SetAggroIcon(false);

        if (cachedCollider != null)
            cachedCollider.enabled = true;

        StopMotion();
        ChooseNewPatrolPoint();
        RefreshHealthUI();
    }
}
