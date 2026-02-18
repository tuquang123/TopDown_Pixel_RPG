// ================= EnemyAI.cs =================

using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

[System.Serializable]
public class EnemyDropItem
{
    public ItemData item; // Item có thể rơi
    //[Range(0f, 1f)] public float dropChance = 0.02f; // 20% rơi
}

public class EnemyAI : MonoBehaviour, IDamageable
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
    protected bool skipHurtAnimation = false;

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
    private Vector3 bloodVFXOffset = new Vector3(0, 0.5f, 0);

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
    protected bool isDead = false;

    [FoldoutGroup("Runtime Debug"), ReadOnly]
    protected bool isTakingDamage = false;

    private bool isKnockbacked = false;

    #endregion
    
    [Header("Anti Ranged Pressure")]
    [SerializeField] protected int rangedHitThreshold = 3;
    [SerializeField] protected float rangedHitWindow = 2f;

    protected int rangedHitCount = 0;
    protected float lastRangedHitTime = -999f;
    protected bool isUnderRangedPressure = false;
    
    [Header("Aggro")]
    [SerializeField] protected bool requireHitToAggro = true;
    protected bool isAggro = false;

    
    public static event Action<float> OnEnemyDefeated;

    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;
    public int MaxHealth => maxHealth;
    public string EnemyName => enemyName;
    public int EnemyLevel => enemyLevel;
    
    protected EnemyHealthUI enemyHealthUI;
    
    [Header("Enemy Type")]
    public bool isBoss = false;
    
    public EnemyHealthUI EnemyHealthUI
    {
        get => enemyHealthUI;
        set => enemyHealthUI = value;
    }

    [BoxGroup("Drops"), LabelText("Gold Min-Max")]
    public Vector2Int goldRange = new Vector2Int(1, 5);

    [BoxGroup("Drops"), LabelText("Item Drops")]
    public List<EnemyDropItem> dropItems = new List<EnemyDropItem>();
    
    [Header("Attack Type")]
    public bool isHoldingSpear = false;
    [Header("Enemy Info On Select")]
    
    [SerializeField] private float infoYOffset = 1.8f;
    
    [BoxGroup("Drops"), LabelText("Drop Rate %")] 
    [Range(0f,1f)] public float dropRate = 0.1f; 
    
    #region Patrol

    [Header("Patrol")]
    [SerializeField] protected bool enablePatrol = true;
    [SerializeField] protected float patrolRadius = 3f;
    [SerializeField] protected float patrolWaitTime = 2f;

    protected Vector3 spawnPosition;
    protected Vector3 patrolTarget;
    protected bool isPatrolling = false;
    protected float patrolWaitTimer = 0f;
	
	public int exp = 3;

    #endregion


    private EnemyInfoPopupUI infoUIInstance;
    private void OnMouseDown()
    {
        if (isDead) return;

        EnemyInfoPopupUI.Instance.Show(this);
    }
    
    protected virtual void Start()
    {
        anim = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;
        
        enemyHealthUI?.UpdateHealth(currentHealth);
    }
    private void OnEnable()
    {
        EnemyTracker.Instance?.Register(this);
        isDead = false; 
        spawnPosition = transform.position;
        ChooseNewPatrolPoint();
        ResetEnemy(); 
    }

    private void OnDisable()
    {
        EnemyTracker.Instance?.Unregister(this);
        ResetEnemy(); 
    }
    
    private void Update()
    {
        if (isDead || isTakingDamage || isKnockbacked) return;
        
        if (requireHitToAggro && !isAggro)
        {
            Patrol();
            return;
        }
        
        if (isUnderRangedPressure && Time.time - lastRangedHitTime > rangedHitWindow)
        {
            isUnderRangedPressure = false;
            rangedHitCount = 0;
        }

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
            MoveToAttackPosition();
            RotateEnemy(target.position.x - transform.position.x);
        }
        else
        {
            anim.SetBool(MoveBool, false);
        }
    }
    
    protected void Patrol()
    {
        if (!enablePatrol || isDead) return;

        if (patrolWaitTimer > 0f)
        {
            patrolWaitTimer -= Time.deltaTime;
            anim.SetBool(MoveBool, false);
            return;
        }

        float dist = Vector2.Distance(transform.position, patrolTarget);

        if (dist <= 0.1f)
        {
            patrolWaitTimer = patrolWaitTime;
            ChooseNewPatrolPoint();
            return;
        }

        Vector2 dir = (patrolTarget - transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);

        RotateEnemy(dir.x);
        anim.SetBool(MoveBool, true);
    }

    
    protected void ChooseNewPatrolPoint()
    {
        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
        patrolTarget = spawnPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
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

    private bool isOptimizedActive = true;

    public void SetActiveForOptimization(bool active)
    {
        isOptimizedActive = active;

        if (anim != null)
            anim.enabled = active;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = active && !IsDead;
    }
    
    public void OptimizedUpdate()
    {
        if (!isOptimizedActive || isDead || isTakingDamage || isKnockbacked)
            return;
        
        if (requireHitToAggro && !isAggro)
        {
            anim.SetBool(MoveBool, false);
            return;
        }

        // --- Logic di chuyển, tấn công ---
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
            MoveToAttackPosition();
            RotateEnemy(target.position.x - transform.position.x);
        }
        else
        {
            anim.SetBool(MoveBool, false);
        }
    }
    protected void FindClosestTarget()
    {
        Transform bestTarget = null;
        float minDist = Mathf.Infinity;

        // Ưu tiên Player nếu trong range
        if (PlayerController.Instance != null && !PlayerController.Instance.IsPlayerDie())
        {
            float dist = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
            if (dist <= detectionRange)
            {
                bestTarget = PlayerController.Instance.transform;
                minDist = dist;
            }
        }

        // Tìm Ally gần nhất
        foreach (var ally in AllyManager.Instance.GetAllies())
        {
            if (ally == null || ally.IsDead) continue;

            float dist = Vector2.Distance(transform.position, ally.transform.position);
            if (dist < minDist && dist <= detectionRange)
            {
                bestTarget = ally.transform;
                minDist = dist;
            }
        }

        target = bestTarget;
    }


    protected virtual void MoveToAttackPosition()
    {
        Vector2 enemyPos = transform.position;
        Vector2 targetPos = target.position;

        float yDiff = Mathf.Abs(enemyPos.y - targetPos.y);
        float xDiff = Mathf.Abs(enemyPos.x - targetPos.x);
        float yTolerance = 0.1f;

        if (yDiff > yTolerance)
        {
            Vector2 directionY = new Vector2(0, targetPos.y - enemyPos.y).normalized;
            transform.position += (Vector3)(directionY * moveSpeed * Time.deltaTime);
            anim.SetBool(MoveBool, true);
        }
        else if (xDiff > attackRange * 0.8f)
        {
            Vector2 directionX = new Vector2(targetPos.x - enemyPos.x, 0).normalized;
            transform.position += (Vector3)(directionX * moveSpeed * Time.deltaTime);
            anim.SetBool(MoveBool, true);
            RotateEnemy(directionX.x);
        }
        else
        {
            anim.SetBool(MoveBool, false);
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                AttackTarget();
            }
        }
    }

    protected void RotateEnemy(float direction)
    {
        transform.localScale = new Vector3(Mathf.Sign(direction) * -1, 1, 1);
    }

    protected virtual void AttackTarget()
    {
        if (target == null || isTakingDamage) return;
        if (isDead) return;

        RotateEnemy(target.position.x - transform.position.x);

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            anim.SetBool(MoveBool, false);

            // Kiểm tra cầm thương
            if (isHoldingSpear)
            {
                anim.SetTrigger(LongAttack); // đây là attack mới
            }
            else
            {
                anim.SetTrigger(AttackTrigger); // attack mặc định
            }

            lastAttackTime = Time.time;
        }
    }


    public void DealDamageToTarget()
    {
        if (target == null) return;
        if (isDead) return;
        
        float distance = Vector2.Distance(transform.position, target.position);
        if (distance > attackRange) return;

        if (target.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(attackDamage);
        }
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

    public virtual void TakeDamage(int damage, bool isCrit = false)
    {
        if (isDead) return;
        
        // 🔥 UPDATE POPUP
        if (EnemyInfoPopupUI.Instance != null)
            EnemyInfoPopupUI.Instance.Refresh();
        
        currentHealth -= damage;
        enemyHealthUI?.UpdateHealth(currentHealth);
        
        RegisterRangedPressure();
        isAggro = true;

        if (!skipHurtAnimation) anim.SetTrigger(DamagedTrigger);

        isTakingDamage = true;

        string damageText = isCrit ? $"CRIT -{damage}" : $"-{damage}";
        Color damageColor = isCrit ? new Color(1f, 0.84f, 0.2f) : Color.white;

        FloatingTextSpawner.Instance.SpawnText(damageText, transform.position + Vector3.up * 1.2f, damageColor);
        SpawnBloodVFX();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            Invoke(nameof(EndDamageStun), damagedStunTime);
        }
    }

    public void SpawnBloodVFX()
    {
        Vector3 basePosition = GetComponent<Collider2D>().bounds.center;
        Vector3 flippedOffset = bloodVFXOffset;
        flippedOffset.x *= Mathf.Sign(transform.localScale.x);
        Vector3 vfxSpawnPos = basePosition + flippedOffset;

        GameObject vfx = ObjectPooler.Instance.Get(
            CommonReferent.Instance.bloodVfxPrefab.name,
            CommonReferent.Instance.bloodVfxPrefab,
            vfxSpawnPos,
            Quaternion.identity
        );

        Vector3 scale = vfx.transform.localScale;
        scale.x = Mathf.Sign(transform.localScale.x) * Mathf.Abs(scale.x);
        vfx.transform.localScale = scale;
    }

    void EndDamageStun() => isTakingDamage = false;

    public void ResetEnemy()
    {
        spawnPosition = transform.position;
        ChooseNewPatrolPoint();
        patrolWaitTimer = 0f;

        currentHealth = maxHealth;
        isDead = false;
        isTakingDamage = false;
        enabled = true;
        isAggro = false;
        GetComponent<Collider2D>().enabled = true;
        enemyHealthUI?.UpdateHealth(currentHealth);
    }

    public event Action OnDeath;

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        SetSelected(false);

        HandleDieAnimation();
        DisableComponents();
        HandleHealthUI();
        NotifySystemsBeforeDrop();
        HandleDrops();
        NotifySystemsAfterDrop();
        Destroy(gameObject);
        if (CommonReferent.Instance.destructionVFXPrefab != null)
        {
            ObjectPooler.Instance.Get(
                "BreakVFX",
                CommonReferent.Instance.destructionVFXPrefab,
                transform.position,
                Quaternion.identity
            );
        }
        //StartCoroutine(DelayDeactivate());
    }

    protected virtual void HandleDieAnimation()
    {
        anim.SetTrigger(DieTrigger);
    }

    protected virtual void DisableComponents()
    {
        GetComponent<Collider2D>().enabled = false;
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
        OnDeath?.Invoke();
        QuestManager.Instance.ReportProgress("NV1", enemyName, 1);
    }

    protected virtual void HandleDrops()
    {
        DropGold();
        DropItems();
    }

    protected virtual void DropGold()
    {
        int goldAmount = Random.Range(goldRange.x, goldRange.y + 1);
        if (goldAmount > 0)
        {
            GoldDropHelper.SpawnGoldBurst(transform.position, goldAmount, CommonReferent.Instance.goldPrefab);
        }
    }
    
    protected virtual void DropItems()
    {
        if (Random.value > dropRate)
        {
            return;
        }
        
        if (dropItems.Count == 0) return;

        int index = Random.Range(0, dropItems.Count);
        EnemyDropItem chosen = dropItems[index];

        if (chosen.item == null) return;

        GameObject prefab = CommonReferent.Instance.itemDropPrefab;

        GameObject dropObj = ObjectPooler.Instance.Get(
            prefab.name,
            prefab,
            transform.position,
            Quaternion.identity
        );

        ItemDrop itemDrop = dropObj.GetComponent<ItemDrop>();
        itemDrop.Setup(new ItemInstance(chosen.item, 0), 1);
    }

    protected virtual void NotifySystemsAfterDrop()
    {
        OnEnemyDefeated?.Invoke(exp);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
    
    protected static readonly int MoveBool = Animator.StringToHash("1_Move");
    protected static readonly int AttackTrigger = Animator.StringToHash("2_Attack");
    private static readonly int DamagedTrigger = Animator.StringToHash("3_Damaged");
    protected static readonly int DieTrigger = Animator.StringToHash("4_Death");
    private static readonly int LongAttack = Animator.StringToHash("8_Attack");
    [Header("Selection")]
    [SerializeField] private GameObject selectionCircle;

    public void SetSelected(bool value)
    {
        if (selectionCircle != null)
            selectionCircle.SetActive(value);
    }

}
