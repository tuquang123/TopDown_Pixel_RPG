using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EnemyDropItem
{
    public ItemData item;
}

public partial class EnemyAI : MonoBehaviour, IDamageable
{
    #region Base Info

    [BoxGroup("Base Info"), LabelText("Enemy Name")] [SerializeField]
    private string enemyName = "Goblin";

    [BoxGroup("Base Info"), LabelText("Enemy Level"), ReadOnly] [SerializeField]
    private int enemyLevel = 1;

    #endregion

    #region Stats

    [BoxGroup("Stats"), LabelText("Max Health")] [SerializeField]
    protected int maxHealth = 100;

    [BoxGroup("Stats"), LabelText("Attack Damage")] [SerializeField]
    protected int attackDamage = 10;

    #endregion

    #region Combat

    [BoxGroup("Combat"), LabelText("Attack Range"), Range(0.1f, 10f)] [SerializeField]
    protected float attackRange = 1.5f;

    [BoxGroup("Combat"), LabelText("Detection Range"), Range(0.1f, 40f)] [SerializeField]
    protected float detectionRange = 5f;

    [BoxGroup("Combat"), LabelText("Attack Cooldown"), Range(0f, 10f)] [SerializeField]
    protected float attackCooldown = 1f;

    [BoxGroup("Combat"), LabelText("Skip Hurt Animation")] [SerializeField]
    protected bool skipHurtAnimation;

    #endregion

    #region Movement

    [BoxGroup("Movement"), LabelText("Move Speed"), Range(0f, 10f)] [SerializeField]
    protected float moveSpeed = 3f;

    [BoxGroup("Movement"), LabelText("Separation Radius"), Range(0f, 2f)] [SerializeField]
    private float separationRadius = 0.6f;

    [BoxGroup("Movement"), LabelText("Separation Weight"), Range(0f, 3f)] [SerializeField]
    private float separationWeight = 1.2f;

    [BoxGroup("Movement"), LabelText("Max Neighbors"), Range(1, 24)] [SerializeField]
    private int maxSeparationNeighbors = 8;

    // ── FIX: Smoothing tốc độ tăng/giảm vận tốc, tránh giật cục ──────────────
    [BoxGroup("Movement"), LabelText("Acceleration"), Range(1f, 50f)] [SerializeField]
    private float acceleration = 20f;

    // ── FIX: Separation lerp speed — làm mượt lực đẩy, tránh dao động ────────
    [BoxGroup("Movement"), LabelText("Separation Smooth Speed"), Range(1f, 20f)] [SerializeField]
    private float separationSmoothSpeed = 8f;

    #endregion

    #region Knockback

    [BoxGroup("Knockback"), LabelText("Force")] [SerializeField]
    private float knockForce = 3f;

    [BoxGroup("Knockback"), LabelText("Duration")] [SerializeField]
    private float knockDuration = 0.3f;

    #endregion

    #region Damage

    [BoxGroup("Damage"), LabelText("Stun Time After Hit"), Range(0f, 2f)] [SerializeField]
    private float damagedStunTime = 0.3f;

    #endregion

    #region VFX

    [BoxGroup("VFX"), LabelText("Blood VFX Offset")] [SerializeField]
    private Vector3 bloodVFXOffset = new Vector3(0f, 0.5f, 0f);

    [BoxGroup("VFX"), LabelText("Die Delay")] [SerializeField]
    private float timeDieDelay = 0.65f;

    #endregion

    #region Runtime Readonly

    [FoldoutGroup("Runtime Debug"), ReadOnly]
    protected Transform target;

    [FoldoutGroup("Runtime Debug"), ReadOnly]
    protected Animator anim;

    [FoldoutGroup("Runtime Debug"), ReadOnly]
    protected float lastAttackTime;

    [FoldoutGroup("Runtime Debug"), ReadOnly]
    protected int currentHealth;

    [FoldoutGroup("Runtime Debug"), ReadOnly]
    protected bool isDead;

    [FoldoutGroup("Runtime Debug"), ReadOnly]
    protected bool isTakingDamage;

    private bool isKnockbacked;

    // ── FIX: Tách _desiredVelocity (mục tiêu) và _smoothedVelocity (thực tế) ──
    // _desiredVelocity: vận tốc AI muốn đạt được (tính trong FixedUpdate)
    // _smoothedVelocity: vận tốc thực tế được lerp dần đến _desiredVelocity
    private Vector2 _desiredVelocity;
    private Vector2 _smoothedVelocity;

    // ── FIX: Cache separation riêng, lerp độc lập tránh dao động ──────────────
    private Vector2 _smoothedSeparation;

    private const int MaxSeparationBuffer = 24;
    private readonly Collider2D[] _separationBuffer = new Collider2D[MaxSeparationBuffer];

    #endregion

    [Header("Anti Ranged Pressure")]
    [SerializeField] protected int rangedHitThreshold = 3;
    [SerializeField] protected float rangedHitWindow = 2f;

    protected int rangedHitCount;
    protected float lastRangedHitTime = -999f;
    protected bool isUnderRangedPressure;

    [Header("Aggro")]
    [SerializeField] protected bool requireHitToAggro = true;
    protected bool isAggro;

    [Header("HP Display Type")]
    [Tooltip("true = always show HP, false = show only when hit")]
    [SerializeField] private bool alwaysShowHP;

    public static event Action<float> OnEnemyDefeated;

    // ── Public Properties ──────────────────────────────────────────────────────
    public int CurrentHealth  => currentHealth;
    public bool IsDead        => isDead;
    public int MaxHealth      => maxHealth;
    public string EnemyName   => enemyName;
    public int EnemyLevel     => enemyLevel;
    public int AttackDamage   => attackDamage;
    public float MoveSpeed    => moveSpeed;

    protected EnemyHealthUI enemyHealthUI;

    [Header("Enemy Type")]
    public bool isBoss;

    public EnemyHealthUI EnemyHealthUI
    {
        get => enemyHealthUI;
        set => enemyHealthUI = value;
    }

    [BoxGroup("Drops"), LabelText("Gold Min-Max")]
    public Vector2Int goldRange = new Vector2Int(1, 5);

    [BoxGroup("Drops"), LabelText("Item Drops")]
    public List<EnemyDropItem> dropItems = new();

    [Header("Attack Type")]
    public bool isHoldingSpear;

    [Header("Enemy Info On Select")]
    [SerializeField] private float infoYOffset = 1.8f;

    [BoxGroup("Drops"), LabelText("Drop Rate %")]
    [Range(0f, 1f)] public float dropRate = 0.1f;

    #region Patrol

    [Header("Patrol")]
    [SerializeField] protected bool enablePatrol = true;
    [SerializeField] protected float patrolRadius = 3f;
    [SerializeField] protected float patrolWaitTime = 2f;

    protected Vector3 spawnPosition;
    protected Vector3 patrolTarget;
    protected bool isPatrolling;
    protected float patrolWaitTimer;

    public int exp = 3;

    #endregion

    private EnemyInfoPopupUI infoUIInstance;
    private Collider2D cachedCollider;
    protected Rigidbody2D cachedRigidbody;
    private bool isOptimizedActive = true;

    protected static readonly int MoveBool      = Animator.StringToHash("1_Move");
    protected static readonly int AttackTrigger = Animator.StringToHash("2_Attack");
    private static readonly int DamagedTrigger  = Animator.StringToHash("3_Damaged");
    protected static readonly int DieTrigger    = Animator.StringToHash("4_Death");
    private static readonly int LongAttack      = Animator.StringToHash("8_Attack");

    [Header("Selection")]
    [SerializeField] private GameObject selectionCircle;

    [Header("Aggro Icon")]
    [SerializeField] private GameObject aggroIcon;
    [SerializeField] private float aggroLoseTime = 5f;

    private float lastAggroTime;

    public event Action OnDeath;
    [SerializeField] private string killObjectiveID;
    public string KillID => killObjectiveID;

    protected void RaiseDeathEvent()
    {
        OnDeath?.Invoke();
        RewardPopupManager.Instance?.ShowEXP(exp); 
    }

    protected virtual void Awake()
    {
        EnsureCachedComponents();
    }

    private void OnMouseDown()
    {
        if (isDead) return;
        EnemyInfoPopupUI.Instance?.Show(this);
    }

    private void OnEnable()
    {
        EnsureCachedComponents();
        EnemyTracker.Instance?.Register(this);
        isDead = false;
        spawnPosition = transform.position;
        ChooseNewPatrolPoint();
        ResetEnemy();
        QuestManager.Instance?.UpdateArrow();
        EnemyTracker.Instance?.Register(this);

        // FIX: Reset smooth state khi enable lại tránh velocity tồn đọng
        _smoothedVelocity   = Vector2.zero;
        _desiredVelocity    = Vector2.zero;
        _smoothedSeparation = Vector2.zero;
    }

    private void OnDisable()
    {
        EnemyTracker.Instance?.Unregister(this);

        if (!gameObject.scene.isLoaded)
            return;

        EnemyTracker.Instance?.Unregister(this);
        ResetEnemy();
    }

    protected virtual void Start()
    {
        EnsureCachedComponents();
        currentHealth = maxHealth;
        RefreshHealthUI();
    }

    // ── FIX: Update chỉ xử lý logic thuần (target, state, animator) ───────────
    // Không đụng vào Rigidbody ở đây để tránh desync với FixedUpdate
    private void Update()
    {
        TickAILogic(true);
    }

    // ── FIX: Toàn bộ physics (velocity) được apply trong FixedUpdate ───────────
    private void FixedUpdate()
    {
        if (cachedRigidbody == null) return;

        // Lerp từ vận tốc hiện tại → vận tốc mục tiêu bằng acceleration
        // Tránh giật khi đổi hướng đột ngột
        _smoothedVelocity = Vector2.MoveTowards(
            _smoothedVelocity,
            _desiredVelocity,
            acceleration * Time.fixedDeltaTime
        );

        cachedRigidbody.linearVelocity = _smoothedVelocity;
    }

    public void OptimizedUpdate()
    {
        TickAILogic(false);
    }

    // ── FIX: Đổi tên TickAI → TickAILogic, chỉ xử lý logic, không apply vật lý
    private void TickAILogic(bool allowPatrolWhenIdle)
    {
        if ((!allowPatrolWhenIdle && !isOptimizedActive) || isDead || isTakingDamage || isKnockbacked)
        {
            // Nếu bị stun/knock/die → dừng mượt
            SetDesiredVelocity(Vector2.zero);
            return;
        }

        EnsureCachedComponents();

        if (requireHitToAggro && !isAggro)
        {
            if (allowPatrolWhenIdle)
                Patrol();
            else
                SetDesiredVelocity(Vector2.zero);

            return;
        }

        if (isUnderRangedPressure && Time.time - lastRangedHitTime > rangedHitWindow)
        {
            isUnderRangedPressure = false;
            rangedHitCount = 0;
        }

        FindClosestTarget();

        if (!IsValidTarget(target, detectionRange * detectionRange))
        {
            target = null;
            SetDesiredVelocity(Vector2.zero);
            HandleAggroState();
            return;
        }

        float sqrDistanceToTarget = ((Vector2)target.position - (Vector2)transform.position).sqrMagnitude;
        if (sqrDistanceToTarget <= detectionRange * detectionRange)
        {
            MoveToAttackPosition();
            RotateEnemy(target.position.x - transform.position.x);
        }
        else
        {
            SetDesiredVelocity(Vector2.zero);
        }

        HandleAggroState();
    }

    // ─── PATROL ────────────────────────────────────────────────────────────────

    protected void Patrol()
    {
        if (!enablePatrol || isDead)
            return;

        if (patrolWaitTimer > 0f)
        {
            patrolWaitTimer -= Time.deltaTime;
            SetDesiredVelocity(Vector2.zero);
            return;
        }

        if (Vector2.Distance(transform.position, patrolTarget) <= 0.1f)
        {
            patrolWaitTimer = patrolWaitTime;
            SetDesiredVelocity(Vector2.zero);
            ChooseNewPatrolPoint();
            return;
        }

        Vector2 dir = (patrolTarget - transform.position).normalized;
        MoveInDirection(dir);
        RotateEnemy(dir.x);
    }

    protected void ChooseNewPatrolPoint()
    {
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * patrolRadius;
        patrolTarget = spawnPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);
        isPatrolling = true;
    }

    // ─── LEVEL / OPTIMIZATION ─────────────────────────────────────────────────

    public void ApplyLevelData(EnemyLevelData data)
    {
        if (data == null)
        {
            Debug.LogWarning("Level data is null when applying to EnemyAI");
            return;
        }

        maxHealth     = data.maxHealth;
        attackDamage  = data.attackDamage;
        moveSpeed     = data.moveSpeed;
        attackCooldown = data.attackCooldown;
        enemyLevel    = data.level;
    }

    public void SetActiveForOptimization(bool active)
    {
        isOptimizedActive = active;

        if (!active)
            SetDesiredVelocity(Vector2.zero);

        if (anim != null)
            anim.enabled = active;

        if (cachedCollider != null)
            cachedCollider.enabled = active && !IsDead;
    }

    // ─── TARGET FINDING ────────────────────────────────────────────────────────

    protected void FindClosestTarget()
    {
        float sqrDetectionRange = detectionRange * detectionRange;
        if (IsValidTarget(target, sqrDetectionRange))
            return;

        Transform bestTarget = null;
        float minSqrDist = float.MaxValue;

        PlayerController playerController = PlayerController.Instance;
        if (playerController != null && !playerController.IsPlayerDie())
        {
            float playerSqrDist = ((Vector2)playerController.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (playerSqrDist <= sqrDetectionRange)
            {
                bestTarget = playerController.transform;
                minSqrDist = playerSqrDist;
            }
        }

        if (AllyManager.Instance != null)
        {
            foreach (AllyBaseAI ally in AllyManager.Instance.GetAllies())
            {
                if (ally == null || ally.IsDead)
                    continue;

                float allySqrDist = ((Vector2)ally.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (allySqrDist <= sqrDetectionRange && allySqrDist < minSqrDist)
                {
                    bestTarget = ally.transform;
                    minSqrDist = allySqrDist;
                }
            }
        }

        target = bestTarget;
    }

    // ─── UTILITIES ─────────────────────────────────────────────────────────────

    protected void EnsureCachedComponents()
    {
        anim           ??= GetComponentInChildren<Animator>();
        cachedCollider ??= GetComponent<Collider2D>();

        if (cachedRigidbody == null)
        {
            cachedRigidbody = GetComponent<Rigidbody2D>();

            // FIX: Bật interpolation để Unity nội suy vị trí giữa các physics frame
            // Tránh hiện tượng tele/nhảy cóc khi render rate > physics rate
            if (cachedRigidbody != null)
                cachedRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    protected void MoveInDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            SetDesiredVelocity(Vector2.zero);
            return;
        }

        // ── FIX: Lerp separation force thay vì tính raw mỗi frame ──────────────
        // Tránh separation dao động quá nhanh gây quái khựng/rung
        Vector2 rawSeparation = CalculateSeparationOffset();
        _smoothedSeparation = Vector2.Lerp(
            _smoothedSeparation,
            rawSeparation,
            separationSmoothSpeed * Time.deltaTime
        );

        Vector2 steeringDirection = direction.normalized + _smoothedSeparation;

        // ── FIX: Nếu separation triệt tiêu hướng di chuyển thì ưu tiên hướng gốc
        // Không để quái bị "đứng hình" vì lực đẩy ngược chiều hoàn toàn
        if (steeringDirection.sqrMagnitude <= 0.0001f)
            steeringDirection = direction.normalized;

        SetDesiredVelocity(steeringDirection.normalized * moveSpeed);
    }

    // ── FIX: Tách hàm SetDesiredVelocity để cập nhật cả Animator đúng chỗ ─────
    // Animator chỉ được set 1 chỗ, không bị gọi chồng chéo
    private void SetDesiredVelocity(Vector2 velocity)
    {
        _desiredVelocity = velocity;
        anim?.SetBool(MoveBool, velocity.sqrMagnitude > 0.01f);
    }

    private Vector2 CalculateSeparationOffset()
    {
        if (separationRadius <= 0f || separationWeight <= 0f)
            return Vector2.zero;

        int hits = Physics2D.OverlapCircleNonAlloc(transform.position, separationRadius, _separationBuffer);
        if (hits <= 1)
            return Vector2.zero;

        Vector2 separation   = Vector2.zero;
        int countedNeighbors = 0;
        int allowedNeighbors = Mathf.Min(maxSeparationNeighbors, MaxSeparationBuffer);
        Vector2 selfPosition = transform.position;

        for (int i = 0; i < hits && countedNeighbors < allowedNeighbors; i++)
        {
            Collider2D otherCollider = _separationBuffer[i];
            if (otherCollider == null || otherCollider.attachedRigidbody == cachedRigidbody)
                continue;

            if (!otherCollider.TryGetComponent(out EnemyAI otherEnemy) || otherEnemy.IsDead)
                continue;

            Vector2 away      = selfPosition - (Vector2)otherEnemy.transform.position;
            float sqrDistance = away.sqrMagnitude;
            if (sqrDistance <= 0.0001f)
                continue;

            // ── FIX: Clamp separation tối đa để tránh lực đẩy quá mạnh gây giật
            float strength = Mathf.Clamp(1f / sqrDistance, 0f, 5f);
            separation += away.normalized * strength;
            countedNeighbors++;
        }

        return countedNeighbors > 0 ? separation.normalized * separationWeight : Vector2.zero;
    }

    // ── FIX: StopMotion chỉ set desired = 0, KHÔNG reset rigidbody velocity thẳng
    // FixedUpdate sẽ lerp về 0 mượt mà thay vì cắt đứt đột ngột
    protected void StopMotion(bool disablePhysics = false)
    {
        _desiredVelocity = Vector2.zero;
        anim?.SetBool(MoveBool, false);

        if (cachedRigidbody != null)
        {
            if (disablePhysics)
            {
                // Chỉ hard-stop khi disable physics hẳn (die, knockback)
                _smoothedVelocity               = Vector2.zero;
                cachedRigidbody.linearVelocity  = Vector2.zero;
                cachedRigidbody.angularVelocity = 0f;
                cachedRigidbody.simulated       = false;
            }
            // Nếu không disable physics thì để FixedUpdate lerp về 0 tự nhiên
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    public void SetSelected(bool value)
    {
        if (selectionCircle != null)
            selectionCircle.SetActive(value);
    }

    private void HandleAggroState()
    {
        if (target != null && target.gameObject.activeInHierarchy)
        {
            float sqrDist = ((Vector2)target.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDist <= detectionRange * detectionRange)
            {
                isAggro       = true;
                lastAggroTime = Time.time;
                SetAggroIcon(true);
                return;
            }
        }

        if (Time.time - lastAggroTime > aggroLoseTime)
        {
            isAggro = false;
            target  = null;
            SetAggroIcon(false);
        }
    }

    private void SetAggroIcon(bool value)
    {
        if (aggroIcon != null)
            aggroIcon.SetActive(value);
    }

    // ─── HEALTH UI ─────────────────────────────────────────────────────────────

    public void AssignHealthUI(EnemyHealthUI ui)
    {
        if (enemyHealthUI != null && enemyHealthUI != ui)
        {
            Destroy(enemyHealthUI.gameObject);
            enemyHealthUI = null;
        }

        enemyHealthUI = ui;
        enemyHealthUI?.ForceSetTarget(gameObject);
    }

    public void ShowUIOnHit()
    {
        EnemyInfoPopupUI.Instance?.Show(this);

        if (enemyHealthUI != null && !alwaysShowHP)
            enemyHealthUI.ShowUI();
    }

    private void RefreshHealthUI()
    {
        if (enemyHealthUI == null) return;

        enemyHealthUI.ForceSetTarget(gameObject);

        if (alwaysShowHP)
            enemyHealthUI.ShowUI();
        else
            enemyHealthUI.HideUI();
    }

    // ─── TARGET VALIDATION ─────────────────────────────────────────────────────

    private bool IsValidTarget(Transform candidate, float sqrDetectionRange)
    {
        if (candidate == null || !candidate.gameObject.activeInHierarchy)
            return false;

        float sqrDist = ((Vector2)candidate.position - (Vector2)transform.position).sqrMagnitude;
        if (sqrDist > sqrDetectionRange)
            return false;

        if (candidate.TryGetComponent(out PlayerStats playerStats))
            return !playerStats.isDead;

        if (candidate.TryGetComponent(out AllyBaseAI allyAi))
            return !allyAi.IsDead;

        return true;
    }
}