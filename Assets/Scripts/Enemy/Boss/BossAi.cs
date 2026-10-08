using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossSkillProfile
{
    Basic,
    SummonGravok,
    LightningStorm,
    CriticalBite,
    SummonSlimes,
    StoneSpikes,
    PhaseVanish
}

public class BossAI : EnemyAI
{
    [Header("Boss Special Settings")]
    [SerializeField] private BossHealthUI bossHealthUI;
    [SerializeField] private string bossSoundId = "default";

    [Header("Boss Skills")]
    [SerializeField] private BossSkillProfile skillProfile = BossSkillProfile.Basic;
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private GameObject slimeMinionPrefab;
    [SerializeField] private GameObject lightningPrefab;
    [SerializeField] private GameObject stoneSpikePrefab;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private bool useTargetedProjectileAttack;
    [SerializeField] private float projectileStandOffRange = 2.25f;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileReleaseDelay = 0.35f;
    [SerializeField] private bool playMeleeSlashVfx;
    [SerializeField] private Color meleeSlashColor = new Color(1f, 0.82f, 0.28f, 0.9f);
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float specialCooldown = 8f;
    [SerializeField] private float minionCooldown = 10f;
    [SerializeField] private float dashCooldown = 8f;
    [SerializeField] private float shootCooldown = 6f;
    [SerializeField] private int gravokSummonCount = 2;
    [SerializeField] private int slimeSummonCount = 5;
    [SerializeField] private float areaSkillRadius = 1.15f;
    [SerializeField] private float vanishDuration = 2.2f;

    private bool isPerformingSkill;
    private bool isPhased;
    private bool isProjectileAttack;
    private float lastSpecialTime;
    private Collider2D bossCollider;
    private SpriteRenderer[] renderers;
    private Color[] normalRendererColors;
    private readonly List<EnemyAI> spawnedMinions = new List<EnemyAI>();

    protected override void Start()
    {
        bossHealthUI = CommonReferent.Instance != null ? CommonReferent.Instance.bossHealthUI : null;
        isBoss = true;

        if (bossHealthUI == null)
        {
            bossHealthUI = FindFirstObjectByType<BossHealthUI>();
            if (bossHealthUI == null)
                Debug.LogWarning("BossAI: khong tim thay BossHealthUI trong scene.");
        }

        base.Start();
        bossHealthUI?.SetMaxHealth(maxHealth);
        skipHurtAnimation = true;
        bossCollider = GetComponent<Collider2D>();
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        normalRendererColors = CaptureRendererColors(renderers);
        PlayBossSound(BossSoundEvent.Spawn);
    }

    private void Update()
    {
        if (isDead || isTakingDamage || isPerformingSkill)
            return;

        FindClosestTarget();

        if (target == null)
        {
            anim.SetBool(MoveBool, false);
            return;
        }

        if (target.TryGetComponent(out PlayerStats playerStats) && playerStats.isDead)
        {
            anim.SetBool(MoveBool, false);
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        if (distanceToTarget <= detectionRange)
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
                MoveToAttackPosition();

            RotateEnemy(target.position.x - transform.position.x);
        }
        else
        {
            anim.SetBool(MoveBool, false);
        }

        TryUseSpecialSkill(distanceToTarget);
    }

    private void TryUseSpecialSkill(float distanceToTarget)
    {
        if (skillProfile == BossSkillProfile.Basic || Time.time - lastSpecialTime < specialCooldown)
            return;

        bool started = skillProfile switch
        {
            BossSkillProfile.SummonGravok => StartSkill(Skill_SpawnMinions(minionPrefab, gravokSummonCount, 1.5f)),
            BossSkillProfile.LightningStorm => StartSkill(Skill_LightningStorm()),
            BossSkillProfile.CriticalBite => distanceToTarget <= attackRange + 1.2f && StartSkill(Skill_CriticalBite()),
            BossSkillProfile.SummonSlimes => StartSkill(Skill_SpawnMinions(slimeMinionPrefab, slimeSummonCount, 2f)),
            BossSkillProfile.StoneSpikes => StartSkill(Skill_StoneSpikes()),
            BossSkillProfile.PhaseVanish => StartSkill(Skill_PhaseVanish()),
            _ => false
        };

        if (started)
            lastSpecialTime = Time.time;
    }

    private bool StartSkill(IEnumerator routine)
    {
        StartCoroutine(routine);
        return true;
    }

    private IEnumerator Skill_SpawnMinions(GameObject spawnPrefab, int count, float radius)
    {
        isPerformingSkill = true;
        PlayBossSound(BossSoundEvent.Summon);
        anim.SetTrigger(AttackTrigger);
        yield return new WaitForSeconds(0.45f);

        EnemyLevelDatabase levelDB = CommonReferent.Instance != null ? CommonReferent.Instance.enemyLevelDatabase : null;

        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / Mathf.Max(1, count);
            Vector2 spawnPos = (Vector2)transform.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            if (spawnPrefab == null)
            {
                Debug.LogWarning($"BossAI: {skillProfile} chua duoc gan prefab linh.");
                continue;
            }

            GameObject minion = ObjectPooler.Instance != null
                ? ObjectPooler.Instance.Get(spawnPrefab.name, spawnPrefab, spawnPos, Quaternion.identity, initSize: 30, expandable: true)
                : Instantiate(spawnPrefab, spawnPos, Quaternion.identity);

            if (minion == null)
                continue;

            EnemyAI ai = minion.GetComponent<EnemyAI>();
            if (ai == null)
            {
                Debug.LogWarning("BossAI: prefab linh khong co EnemyAI.");
                continue;
            }

            if (levelDB != null)
                ai.ApplyLevelData(levelDB.GetDataByLevel(1));

            ai.ResetEnemy();
            spawnedMinions.Add(ai);
        }

        yield return new WaitForSeconds(0.5f);
        isPerformingSkill = false;
    }

    private IEnumerator Skill_LightningStorm()
    {
        isPerformingSkill = true;
        PlayBossSound(BossSoundEvent.Shoot);
        anim.SetTrigger(AttackTrigger);
        yield return new WaitForSeconds(0.35f);

        Vector2 center = target != null ? target.position : transform.position;
        SpawnAreaStrike(center, lightningPrefab, BossAreaStrikeVisual.Lightning, areaSkillRadius, attackDamage);

        for (int i = 0; i < 3; i++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(0.9f, 2.4f);
            SpawnAreaStrike(center + offset, lightningPrefab, BossAreaStrikeVisual.Lightning, areaSkillRadius * 0.85f, Mathf.RoundToInt(attackDamage * 0.75f));
        }

        yield return new WaitForSeconds(0.6f);
        isPerformingSkill = false;
    }

    private IEnumerator Skill_CriticalBite()
    {
        isPerformingSkill = true;
        PlayBossSound(BossSoundEvent.Attack);
        anim.SetTrigger(AttackTrigger);
        yield return new WaitForSeconds(0.25f);

        Transform biteTarget = target;
        if (biteTarget != null)
        {
            Vector2 dir = ((Vector2)biteTarget.position - (Vector2)transform.position).normalized;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = dir * dashSpeed * 0.65f;

            yield return new WaitForSeconds(Mathf.Min(0.18f, dashDuration));

            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            if (Vector2.Distance(transform.position, biteTarget.position) <= attackRange + 0.9f
                && biteTarget.TryGetComponent(out IDamageable damageable))
            {
                int biteDamage = Mathf.RoundToInt(attackDamage * 2.75f);
                damageable.TakeDamage(biteDamage, true);
                PlayBossSound(BossSoundEvent.HitPlayer);
            }
        }

        yield return new WaitForSeconds(0.35f);
        isPerformingSkill = false;
    }

    private IEnumerator Skill_StoneSpikes()
    {
        isPerformingSkill = true;
        PlayBossSound(BossSoundEvent.Attack);
        anim.SetTrigger(AttackTrigger);
        yield return new WaitForSeconds(0.35f);

        Vector2 origin = target != null ? target.position : transform.position;
        SpawnAreaStrike(origin, stoneSpikePrefab, BossAreaStrikeVisual.StoneSpike, areaSkillRadius, Mathf.RoundToInt(attackDamage * 1.25f));

        Vector2 dir = target != null ? ((Vector2)target.position - (Vector2)transform.position).normalized : Vector2.right;
        for (int i = 1; i <= 3; i++)
            SpawnAreaStrike((Vector2)transform.position + dir * (i * 1.15f), stoneSpikePrefab, BossAreaStrikeVisual.StoneSpike, areaSkillRadius * 0.7f, attackDamage);

        yield return new WaitForSeconds(0.75f);
        isPerformingSkill = false;
    }

    private IEnumerator Skill_PhaseVanish()
    {
        isPerformingSkill = true;
        isPhased = true;
        PlayBossSound(BossSoundEvent.Dash);
        anim.SetTrigger(AttackTrigger);
        SetPhaseVisual(true);
        yield return new WaitForSeconds(vanishDuration);
        SetPhaseVisual(false);
        isPhased = false;
        isPerformingSkill = false;
    }

    private void SpawnAreaStrike(Vector2 position, GameObject prefab, BossAreaStrikeVisual visual, float radius, int damage)
    {
        GameObject strike = prefab != null
            ? Instantiate(prefab, position, Quaternion.identity)
            : new GameObject($"{visual}Strike");

        strike.transform.position = position;
        BossAreaStrikeEffect effect = strike.GetComponent<BossAreaStrikeEffect>();
        if (effect == null)
            effect = strike.AddComponent<BossAreaStrikeEffect>();

        effect.Initialize(radius, 0.65f, 0.35f, damage, visual);
    }

    private void SetPhaseVisual(bool phased)
    {
        if (bossCollider != null)
            bossCollider.enabled = !phased;

        renderers ??= GetComponentsInChildren<SpriteRenderer>(true);
        normalRendererColors ??= CaptureRendererColors(renderers);

        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer renderer = renderers[i];
            if (renderer == null)
                continue;

            Color color = phased ? renderer.color : normalRendererColors[Mathf.Min(i, normalRendererColors.Length - 1)];
            if (phased)
                color.a *= 0.18f;

            renderer.color = color;
        }
    }

    private static Color[] CaptureRendererColors(SpriteRenderer[] spriteRenderers)
    {
        Color[] colors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
            colors[i] = spriteRenderers[i] != null ? spriteRenderers[i].color : Color.white;

        return colors;
    }

    public override void TakeDamage(int damage, bool isCrit = false)
    {
        if (isPhased)
        {
            FloatingTextSpawner.Instance?.SpawnText("MISS", transform.position + Vector3.up * 1.2f, Color.cyan);
            return;
        }

        base.TakeDamage(damage, isCrit);
        bossHealthUI?.UpdateHealth(currentHealth);

        if (!isDead)
            PlayBossSound(BossSoundEvent.Hurt);
    }

    protected override void AttackTarget()
    {
        if (useTargetedProjectileAttack && target != null)
        {
            float targetDistance = Vector2.Distance(transform.position, target.position);
            if (targetDistance > attackRange + 0.3f && targetDistance <= projectileStandOffRange)
            {
                if (isTakingDamage || isDead || anim == null || Time.time - lastAttackTime < attackCooldown)
                    return;

                RotateEnemy(target.position.x - transform.position.x);
                attackSnapshot = target;
                isProjectileAttack = true;
                anim.SetBool(MoveBool, false);
                anim.SetTrigger(AttackTrigger);
                lastAttackTime = Time.time;
                PlayBossSound(BossSoundEvent.Shoot);
                SpawnProjectileTelegraph(attackSnapshot.position);
                StartCoroutine(LaunchTargetedProjectile(attackSnapshot));
                return;
            }
        }

        isProjectileAttack = false;
        bool canAttack = target != null
                         && !isTakingDamage
                         && !isDead
                         && anim != null
                         && Time.time - lastAttackTime >= attackCooldown;

        base.AttackTarget();

        if (canAttack)
            PlayBossSound(BossSoundEvent.Attack);
    }

    protected override void MoveToAttackPosition()
    {
        if (useTargetedProjectileAttack && target != null)
        {
            float targetDistance = Vector2.Distance(transform.position, target.position);
            if (targetDistance <= projectileStandOffRange)
            {
                StopMotion();
                if (Time.time - lastAttackTime >= attackCooldown)
                    AttackTarget();
                return;
            }
        }

        base.MoveToAttackPosition();
    }

    public override void DealDamageToTarget()
    {
        // The ranged animation still invokes this event. Damage comes from the projectile on impact.
        if (isProjectileAttack)
            return;

        Transform attackTarget = attackSnapshot != null ? attackSnapshot : target;
        bool hasValidHit = attackTarget != null
                           && attackTarget.gameObject.activeInHierarchy
                           && attackTarget.TryGetComponent(out IDamageable _);

        if (playMeleeSlashVfx)
            SpawnMeleeSlashVfx(attackTarget);

        base.DealDamageToTarget();

        if (hasValidHit)
            PlayBossSound(BossSoundEvent.HitPlayer);
    }

    private IEnumerator LaunchTargetedProjectile(Transform shotTarget)
    {
        yield return new WaitForSeconds(projectileReleaseDelay);

        if (isDead || shotTarget == null || !shotTarget.gameObject.activeInHierarchy || bulletPrefab == null)
            yield break;

        Vector2 direction = ((Vector2)shotTarget.position - (Vector2)transform.position).normalized;
        if (direction.sqrMagnitude <= 0.001f)
            direction = Vector2.right;

        Vector3 spawnPosition = transform.position + (Vector3)(direction * 0.65f) + Vector3.up * 0.18f;
        GameObject projectile = Instantiate(bulletPrefab, spawnPosition, Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));

        if (projectile.TryGetComponent(out EnemyProjectile legacyProjectile))
        {
            legacyProjectile.enabled = false;
            Destroy(legacyProjectile);
        }

        if (projectile.TryGetComponent(out Rigidbody2D body))
        {
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
        }

        BossTargetedProjectile targetedProjectile = projectile.GetComponent<BossTargetedProjectile>();
        if (targetedProjectile == null)
            targetedProjectile = projectile.AddComponent<BossTargetedProjectile>();

        targetedProjectile.Initialize(shotTarget, gameObject, attackDamage, projectileSpeed);
    }

    private void SpawnProjectileTelegraph(Vector3 targetPosition)
    {
        GameObject marker = new GameObject("Goblin King Target Marker");
        marker.transform.position = targetPosition + Vector3.up * 0.02f;

        LineRenderer ring = marker.AddComponent<LineRenderer>();
        ring.useWorldSpace = false;
        ring.loop = true;
        ring.positionCount = 28;
        ring.startWidth = 0.055f;
        ring.endWidth = 0.055f;
        ring.material = new Material(Shader.Find("Sprites/Default"));
        ring.startColor = new Color(1f, 0.16f, 0.03f, 0.9f);
        ring.endColor = new Color(1f, 0.8f, 0.12f, 0.9f);
        ring.sortingOrder = 88;

        const float radius = 0.48f;
        for (int i = 0; i < ring.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / ring.positionCount;
            ring.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
        }

        Destroy(marker, projectileReleaseDelay);
    }

    private void SpawnMeleeSlashVfx(Transform attackTarget)
    {
        Vector2 direction = attackTarget != null
            ? ((Vector2)attackTarget.position - (Vector2)transform.position).normalized
            : new Vector2(-Mathf.Sign(transform.localScale.x), 0f);

        if (direction.sqrMagnitude <= 0.001f)
            direction = Vector2.right;

        Vector3 center = transform.position + (Vector3)(direction * 0.75f) + Vector3.up * 0.15f;
        GameObject slashRoot = new GameObject("Goblin King Slash VFX");
        slashRoot.transform.position = center;
        slashRoot.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        for (int i = 0; i < 3; i++)
        {
            GameObject lineObject = new GameObject($"SlashArc_{i + 1}");
            lineObject.transform.SetParent(slashRoot.transform, false);

            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 3;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = meleeSlashColor;
            line.endColor = new Color(meleeSlashColor.r, meleeSlashColor.g, meleeSlashColor.b, 0f);
            line.startWidth = 0.09f - i * 0.018f;
            line.endWidth = 0.015f;
            line.sortingOrder = 85;

            float yOffset = (i - 1) * 0.18f;
            float length = 0.95f - i * 0.12f;
            line.SetPosition(0, new Vector3(-0.25f, -0.35f + yOffset, 0f));
            line.SetPosition(1, new Vector3(0.18f, 0.02f + yOffset, 0f));
            line.SetPosition(2, new Vector3(length, 0.35f + yOffset, 0f));
        }

        Destroy(slashRoot, 0.18f);
    }

    protected override void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // Dung coroutine dang cast de khong de them linh sau khi boss chet
        StopAllCoroutines();

        // Boss chet thi tat het linh da trieu hoi (khong tinh quest/gold)
        for (int i = 0; i < spawnedMinions.Count; i++)
        {
            if (spawnedMinions[i] != null)
                spawnedMinions[i].gameObject.SetActive(false);
        }
        spawnedMinions.Clear();

        PlayBossSound(BossSoundEvent.Death);
        anim.SetTrigger(DieTrigger);
        GetComponent<Collider2D>().enabled = false;
        enabled = false;

        bossHealthUI?.Hide();

        RaiseDeathEvent();
        QuestManager.Instance?.ReportProgressByObjectiveName(EnemyName, 1);
        QuestManager.Instance?.ReportProgressByObjectiveName("Boss", 1);
        GoldDropHelper.SpawnGoldBurst(
            transform.position,
            Random.Range(10, 20),
            CommonReferent.Instance.goldPrefab
        );

        StartCoroutine(DisableBossAfterDelay());
    }

    private IEnumerator DisableBossAfterDelay()
    {
        yield return null;
        while (anim.IsInTransition(0))
            yield return null;

        float deathLength = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(deathLength);

        Destroy(gameObject);
    }

    private void OnDisable() => bossHealthUI?.Hide();
    private void OnDestroy() => bossHealthUI?.Hide();

    private void PlayBossSound(BossSoundEvent soundEvent)
    {
        BossSoundDatabase.Play(bossSoundId, soundEvent);
    }
}