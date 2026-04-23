using UnityEngine;

public class PlayerCombat
{
    private readonly PlayerController owner;

    public PlayerCombat(PlayerController owner)
    {
        this.owner = owner;
    }

    public void ClearTargetsAndSelection()
    {
        owner.TargetEnemy        = null;
        owner.TargetDestructible = null;

        if (owner.CurrentSelectedEnemy != null)
        {
            owner.CurrentSelectedEnemy.SetSelected(false);
            owner.CurrentSelectedEnemy = null;
        }

        if (owner.CurrentSelectedDestructible != null)
        {
            owner.CurrentSelectedDestructible.SetSelected(false);
            owner.CurrentSelectedDestructible = null;
        }
    }

    public void TryAttack()
    {
        if (owner.Stats == null || owner.Stats.isUsingSkill || owner.Animator == null)
            return;

        // Clear trigger nếu target vừa die
        if (!HasValidTarget())
        {
            owner.Animator.ResetTrigger(PlayerController.AttackTrigger);
            owner.Animator.ResetTrigger(PlayerController.AttackBow);
            owner.Animator.ResetTrigger(PlayerController.AttackRange);
            return;
        }

        float finalAttackSpeed = owner.Stats.GetAttackSpeed() * owner.GetWeaponAttackSpeedMultiplier();
        if (finalAttackSpeed <= 0f || Time.time - owner.LastAttackTime < 1f / finalAttackSpeed)
            return;

        owner.LastAttackTime = Time.time;

        if (owner.typeWeapon == WeaponCategory.Bow)
            owner.Animator.SetTrigger(PlayerController.AttackBow);
        else if (owner.typeWeapon == WeaponCategory.Ranged)
            owner.Animator.SetTrigger(PlayerController.AttackRange);
        else
            owner.Animator.SetTrigger(PlayerController.AttackTrigger);
    }

    public void ThrowShuriken()
    {
        if (owner.AttackPoint == null || CommonReferent.Instance?.surikenPrefab == null || owner.Stats == null)
            return;

        Transform target = owner.TargetEnemy != null ? owner.TargetEnemy : owner.TargetDestructible;
        Vector2 dir = target != null
            ? ((Vector2)target.position - (Vector2)owner.AttackPoint.position).normalized
            : (owner.transform.localScale.x < 0f ? Vector2.right : Vector2.left);

        GameObject shuriken = Object.Instantiate(
            CommonReferent.Instance.surikenPrefab,
            owner.AttackPoint.position,
            Quaternion.identity);

        shuriken.GetComponent<ShurikenProjectile>()?.Init(
            (int)owner.Stats.attack.Value,
            dir,
            owner.Stats.GetCritChance(),
            owner.Stats.lifeSteal.Value);
    }

    public void FireArrow()
    {
        if (owner.AttackPoint == null || CommonReferent.Instance?.arrowProjectile == null || owner.Stats == null)
            return;

        Transform target = owner.TargetEnemy != null ? owner.TargetEnemy : owner.TargetDestructible;
        Vector2 dir = target != null
            ? ((Vector2)target.position - (Vector2)owner.AttackPoint.position).normalized
            : (owner.transform.localScale.x < 0f ? Vector2.right : Vector2.left);

        if (target != null)
            owner.RotateCharacter(dir.x);

        GameObject arrow = Object.Instantiate(
            CommonReferent.Instance.arrowProjectile,
            owner.AttackPoint.position,
            Quaternion.identity);

        arrow.GetComponent<ArrowProjectile>()?.Init(
            (int)owner.Stats.attack.Value,
            dir,
            owner.Stats.GetCritChance());
    }

    public void ApplyAttackDamage(bool isSkill, SkillData skill = null)
    {
        if (owner.Stats == null || owner.Stats.isDead)
            return;

        AudioManager.Instance?.PlaySFX("Attack");

        // ── Enemy target ─────────────────────────────────────────────────────
        if (owner.TargetEnemy != null)
        {
            EnemyAI enemy = owner.TargetEnemy.GetComponent<EnemyAI>();

            // Guard: enemy vừa die giữa lúc animation chạy → skip
            if (enemy == null || enemy.IsDead)
            {
                ClearTargetsAndSelection();
                return;
            }

            int  damage = (int)owner.Stats.attack.Value;
            bool isCrit = Random.Range(0f, 100f) < owner.Stats.GetCritChance();
            if (isCrit) damage = Mathf.RoundToInt(damage * 1.5f);

            enemy.TakeDamage(damage, isCrit);

            int healed = Mathf.Max(0, owner.Stats.HealFromLifeSteal(damage));
            if (healed > 0)
            {
                FloatingTextSpawner.Instance?.SpawnText(
                    $"+{healed}",
                    owner.transform.position + Vector3.up,
                    new Color(0.3f, 1f, 0.3f));
            }

            return;
        }

        // ── Destructible (chỉ khi không có enemy target) ─────────────────────
        if (!isSkill && owner.TargetDestructible != null)
        {
            owner.TargetDestructible.GetComponent<DestructibleObject>()?.Hit();
        }
    }

    public void FindClosestEnemy()
    {
        EnemyTracker tracker = EnemyTracker.Instance;
        if (tracker == null)
        {
            owner.TargetEnemy = null;
            return;
        }

        float detectionRange = owner.GetDetectionRange();
        if (owner.CurrentSelectedEnemy != null && IsEnemyValid(owner.CurrentSelectedEnemy, detectionRange))
        {
            owner.TargetEnemy = owner.CurrentSelectedEnemy.transform;
            return;
        }

        EnemyAI bestEnemy = tracker.GetClosestEnemy(owner.transform.position, detectionRange);
        owner.TargetEnemy = bestEnemy != null ? bestEnemy.transform : null;
        UpdateEnemySelection(bestEnemy);
    }

    public void FindClosestDestructible()
    {
        owner.TargetDestructible = null;
        DestructibleObject best  = null;
        float minSqrDist         = float.MaxValue;

        if (DestructibleTracker.Instance != null)
        {
            foreach (DestructibleObject obj in DestructibleTracker.Instance.GetInRange(owner.transform.position, owner.GetDetectionRange()))
            {
                if (obj == null) continue;

                float sqrDist = ((Vector2)obj.transform.position - (Vector2)owner.transform.position).sqrMagnitude;
                if (sqrDist < minSqrDist)
                {
                    minSqrDist = sqrDist;
                    best = obj;
                }
            }
        }

        owner.TargetDestructible = best != null ? best.transform : null;
        UpdateDestructibleSelection(best);
    }

    private bool HasValidTarget()
    {
        if (owner.TargetEnemy != null)
        {
            EnemyAI enemy = owner.TargetEnemy.GetComponent<EnemyAI>();
            return enemy != null && !enemy.IsDead && enemy.CurrentHealth > 0;
        }

        if (owner.TargetDestructible != null)
            return owner.TargetDestructible.GetComponent<DestructibleObject>() != null;

        return false;
    }

    private bool IsEnemyValid(EnemyAI enemy, float range)
    {
        if (enemy == null || enemy.IsDead || !enemy.gameObject.activeInHierarchy)
            return false;

        return ((Vector2)enemy.transform.position - (Vector2)owner.transform.position).sqrMagnitude <= range * range;
    }

    private void UpdateEnemySelection(EnemyAI bestEnemy)
    {
        if (owner.CurrentSelectedEnemy == bestEnemy) return;

        if (owner.CurrentSelectedEnemy != null)
            owner.CurrentSelectedEnemy.SetSelected(false);

        owner.CurrentSelectedEnemy = bestEnemy;

        if (owner.CurrentSelectedEnemy != null)
            owner.CurrentSelectedEnemy.SetSelected(true);
    }

    private void UpdateDestructibleSelection(DestructibleObject best)
    {
        if (owner.CurrentSelectedDestructible == best) return;

        if (owner.CurrentSelectedDestructible != null)
            owner.CurrentSelectedDestructible.SetSelected(false);

        owner.CurrentSelectedDestructible = best;

        if (owner.CurrentSelectedDestructible != null)
            owner.CurrentSelectedDestructible.SetSelected(true);
    }
}