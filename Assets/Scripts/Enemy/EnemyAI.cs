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

    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;
    public int MaxHealth => maxHealth;
    public string EnemyName => enemyName;
    public int EnemyLevel => enemyLevel;

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

    protected static readonly int MoveBool = Animator.StringToHash("1_Move");
    protected static readonly int AttackTrigger = Animator.StringToHash("2_Attack");
    private static readonly int DamagedTrigger = Animator.StringToHash("3_Damaged");
    protected static readonly int DieTrigger = Animator.StringToHash("4_Death");
    private static readonly int LongAttack = Animator.StringToHash("8_Attack");

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
    }
    protected virtual void Awake()
    {
        EnsureCachedComponents();
    }

    private void OnMouseDown()
    {
        if (isDead)
            return;

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

    private void Update()
    {
        TickAI(true);
    }

    public void OptimizedUpdate()
    {
        TickAI(false);
    }

    private void TickAI(bool allowPatrolWhenIdle)
    {
        if ((!allowPatrolWhenIdle && !isOptimizedActive) || isDead || isTakingDamage || isKnockbacked)
            return;

        EnsureCachedComponents();

        if (requireHitToAggro && !isAggro)
        {
            if (allowPatrolWhenIdle)
                Patrol();
            else
                StopMotion();

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
            StopMotion();
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
            StopMotion();
        }

        HandleAggroState();
    }

    protected void Patrol()
    {
        if (!enablePatrol || isDead)
            return;

        if (patrolWaitTimer > 0f)
        {
            patrolWaitTimer -= Time.deltaTime;
            StopMotion();
            return;
        }

        if (Vector2.Distance(transform.position, patrolTarget) <= 0.1f)
        {
            patrolWaitTimer = patrolWaitTime;
            StopMotion();
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

    public void ApplyLevelData(EnemyLevelData data)
    {
        if (data == null)
        {
            Debug.LogWarning("Level data is null when applying to EnemyAI");
            return;
        }

        maxHealth = data.maxHealth;
        attackDamage = data.attackDamage;
        moveSpeed = data.moveSpeed;
        attackCooldown = data.attackCooldown;
        enemyLevel = data.level;
    }

    public void SetActiveForOptimization(bool active)
    {
        isOptimizedActive = active;

        if (!active)
            StopMotion();

        if (anim != null)
            anim.enabled = active;

        if (cachedCollider != null)
            cachedCollider.enabled = active && !IsDead;
    }

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

    protected void EnsureCachedComponents()
    {
        anim ??= GetComponentInChildren<Animator>();
        cachedCollider ??= GetComponent<Collider2D>();
        cachedRigidbody ??= GetComponent<Rigidbody2D>();
    }

    protected void MoveInDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            StopMotion();
            return;
        }

        transform.position += (Vector3)(direction.normalized * moveSpeed * Time.deltaTime);
        anim?.SetBool(MoveBool, true);
    }

    protected void StopMotion(bool disablePhysics = false)
    {
        if (cachedRigidbody != null)
        {
            cachedRigidbody.linearVelocity = Vector2.zero;
            cachedRigidbody.angularVelocity = 0f;
            cachedRigidbody.simulated = !disablePhysics;
        }

        anim?.SetBool(MoveBool, false);
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
                isAggro = true;
                lastAggroTime = Time.time;
                SetAggroIcon(true);
                return;
            }
        }

        if (Time.time - lastAggroTime > aggroLoseTime)
        {
            isAggro = false;
            target = null;
            SetAggroIcon(false);
        }
    }

    private void SetAggroIcon(bool value)
    {
        if (aggroIcon != null)
            aggroIcon.SetActive(value);
    }

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
        if (enemyHealthUI != null && !alwaysShowHP)
            enemyHealthUI.ShowUI();
    }

    private void RefreshHealthUI()
    {
        if (enemyHealthUI == null)
            return;

        enemyHealthUI.ForceSetTarget(gameObject);

        if (alwaysShowHP)
            enemyHealthUI.ShowUI();
        else
            enemyHealthUI.HideUI();
    }

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
