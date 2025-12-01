using System.Collections;
using UnityEngine;

public class BossAI : EnemyAI
{
    [Header("Boss Special Settings")]
    //[SerializeField] private float specialAttackCooldown = 5f;
    [SerializeField] private BossHealthUI bossHealthUI;

    [Header("Boss Skills")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float minionCooldown = 10f;
    [SerializeField] private float dashCooldown = 8f;
    [SerializeField] private float shootCooldown = 6f;

    //private float _lastSpecialAttackTime;
    private float _lastMinionTime;
    private float _lastDashTime;
    private float _lastShootTime;

    private bool _isPerformingSkill = false;

    protected override void Start()
    {
        bossHealthUI = CommonReferent.Instance.bossHealthUI;
        isBoss = true;

        if (bossHealthUI == null)
        {
            bossHealthUI = FindFirstObjectByType<BossHealthUI>(); 
            if (bossHealthUI == null)
                Debug.LogWarning("Không tìm thấy BossHealthUI trong scene!");
        }

        base.Start();
        bossHealthUI?.SetMaxHealth(maxHealth);
        skipHurtAnimation = true;
    }

    private void Update()
    {
        if (isDead || isTakingDamage || _isPerformingSkill) return;

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
            if (!_isPerformingSkill && !anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
                MoveToAttackPosition();
        
            RotateEnemy(target.position.x - transform.position.x);
        }
        else
        {
            anim.SetBool(MoveBool, false);
        }


        // Gọi skill theo cooldown
        if (Time.time - _lastMinionTime >= minionCooldown)
        {
            StartCoroutine(Skill_SpawnMinions());
            _lastMinionTime = Time.time;
        }
        else if (Time.time - _lastDashTime >= dashCooldown)
        {
            StartCoroutine(Skill_Dash());
            _lastDashTime = Time.time;
        }
        else if (Time.time - _lastShootTime >= shootCooldown)
        {
            StartCoroutine(Skill_Shoot());
            _lastShootTime = Time.time;
        }
    }

    // ================== SKILLS ==================

    private IEnumerator Skill_SpawnMinions()
    {
        _isPerformingSkill = true;
        anim.SetTrigger(AttackTrigger);
        yield return new WaitForSeconds(0.5f); // delay gồng trước khi triệu hồi

        EnemyLevelDatabase levelDB = CommonReferent.Instance.enemyLevelDatabase;
        Transform hpCanvas = CommonReferent.Instance.canvasHp.transform;

        for (int i = 0; i < 2; i++)
        {
            Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 1.5f;

            // --- Lấy quái từ Object Pool ---
            if (minionPrefab == null)
            {
                Debug.LogError("BossAI: minionPrefab chưa được gán!");
                continue;
            }

            GameObject minion = ObjectPooler.Instance.Get(
                minionPrefab.name,
                minionPrefab,
                spawnPos,
                Quaternion.identity,
                initSize: 30,
                expandable: true
            );

            if (minion == null) continue;

            // --- Thiết lập dữ liệu quái ---
            var ai = minion.GetComponent<EnemyAI>();
            if (ai != null)
            {
                if (levelDB != null)
                {
                    var levelData = levelDB.GetDataByLevel(1); // cấp 1 cho quái con
                    ai.ApplyLevelData(levelData);
                }

                ai.ResetEnemy();

                // Tạo UI HP nếu chưa có
                if (ai.EnemyHealthUI == null && CommonReferent.Instance.hpSliderUi != null)
                {
                    GameObject ui = Instantiate(CommonReferent.Instance.hpSliderUi, hpCanvas, false);
                    var uiComp = ui.GetComponent<EnemyHealthUI>();
                    uiComp.SetTarget(minion);
                    ai.EnemyHealthUI = uiComp;
                }
            }
            else
            {
                Debug.LogWarning("Prefab minion không có EnemyAI!");
            }
        }

        yield return new WaitForSeconds(0.5f);
        _isPerformingSkill = false;
    }


    private IEnumerator Skill_Dash()
    {
        _isPerformingSkill = true;
        anim.SetTrigger(AttackTrigger);

        yield return new WaitForSeconds(0.3f); // gồng nhẹ

        Vector2 dir = (target.position - transform.position).normalized;
        float elapsed = 0f;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        while (elapsed < dashDuration)
        {
            rb.linearVelocity = dir * dashSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        _isPerformingSkill = false;
    }

    private IEnumerator Skill_Shoot()
    {
        _isPerformingSkill = true;
        anim.SetTrigger(AttackTrigger);
        yield return new WaitForSeconds(0.3f);

        if (bulletPrefab != null && target != null)
        {
            Vector2 dir = (target.position - transform.position).normalized;
            GameObject bullet = Instantiate(bulletPrefab, transform.position + (Vector3)(dir * 0.8f), Quaternion.identity);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = dir * 8f;
        }

        yield return new WaitForSeconds(0.5f);
        _isPerformingSkill = false;
    }

    // ================== HEALTH & DEATH ==================

    public override void TakeDamage(int damage, bool isCrit = false)
    {
        base.TakeDamage(damage, isCrit);
        bossHealthUI?.UpdateHealth(currentHealth);
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        anim.SetTrigger(DieTrigger);
        GetComponent<Collider2D>().enabled = false;
        enabled = false;

        bossHealthUI?.Hide();

        QuestManager.Instance.ReportProgress("BossKilled", EnemyName, 1);
        GoldDropHelper.SpawnGoldBurst(
            transform.position,
            UnityEngine.Random.Range(10, 20),
            CommonReferent.Instance.goldPrefab
        );

        StartCoroutine(DisableBossAfterDelay(2f));
    }

    private IEnumerator DisableBossAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    private void OnDisable() => bossHealthUI?.Hide();
    private void OnDestroy() => bossHealthUI?.Hide();
}
