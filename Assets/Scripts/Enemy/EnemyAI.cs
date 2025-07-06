using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public enum EnemyState
{
    Idle,
    MoveToTarget,
    Attacking,
    TakingDamage,
    Dead
}

public class EnemyAI : MonoBehaviour, IDamageable
{
    [BoxGroup("Base Stats")] [SerializeField] protected string enemyName = "Goblin";
    [BoxGroup("Base Stats")] [SerializeField] protected int enemyLevel = 1;
    [BoxGroup("Base Stats")] [SerializeField] protected int maxHealth = 100;
    [BoxGroup("Base Stats")] [SerializeField] protected int attackDamage = 10;
    [BoxGroup("Base Stats")] [SerializeField] protected float attackRange = 1.5f;
    [BoxGroup("Base Stats")] [SerializeField] protected float detectionRange = 5f;
    [BoxGroup("Base Stats")] [SerializeField] protected float attackCooldown = 1f;
    [BoxGroup("Base Stats")] [SerializeField] protected float moveSpeed = 3f;
    [BoxGroup("Base Stats")] [SerializeField] protected float knockForce = 3f;
    [BoxGroup("Base Stats")] [SerializeField] protected float knockDuration = 0.3f;
    [BoxGroup("Base Stats")] [SerializeField] protected float damagedStunTime = 0.3f;
    [BoxGroup("Base Stats")] [SerializeField] protected float timeDieDelay = 0.65f;
    [BoxGroup("Base Stats")] [SerializeField] protected Vector3 bloodVFXOffset = new(0, 0.5f, 0);
    [BoxGroup("Base Stats")] [SerializeField] protected bool skipHurtAnimation = false;

    [BoxGroup("Runtime Info"), ReadOnly] protected Animator anim;
    [BoxGroup("Runtime Info"), ReadOnly] protected Transform target;
    [BoxGroup("Runtime Info"), ReadOnly] protected int currentHealth;
    [BoxGroup("Runtime Info"), ReadOnly] protected float lastAttackTime;
    [BoxGroup("Runtime Info"), ReadOnly] protected bool isDead = false;
    [BoxGroup("Runtime Info"), ReadOnly] protected bool isTakingDamage = false;
    [BoxGroup("Runtime Info"), ReadOnly] protected bool isKnockbacked = false;
    [BoxGroup("Runtime Info"), ReadOnly] protected EnemyState currentState;

    [BoxGroup("Runtime Info"), ReadOnly] private EnemyHealthUI enemyHealthUI;

    protected static readonly int MoveBool = Animator.StringToHash("1_Move");
    protected static readonly int AttackTrigger = Animator.StringToHash("2_Attack");
    protected static readonly int DamagedTrigger = Animator.StringToHash("3_Damaged");
    protected static readonly int DieTrigger = Animator.StringToHash("4_Death");

    public bool IsDead => isDead;
    public int MaxHealth => maxHealth;
    public string EnemyName => enemyName;
    public int EnemyLevel => enemyLevel;

    public EnemyHealthUI EnemyHealthUI
    {
        get => enemyHealthUI;
        set => enemyHealthUI = value;
    }

    public event Action OnDeath;
    public static event Action<float> OnEnemyDefeated;

    protected virtual void Start()
    {
        anim = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;
        EnemyTracker.Instance.Register(this);
        ChangeState(EnemyState.Idle);
    }

    protected virtual void Update()
    {
        if (isDead || isKnockbacked) return;

        switch (currentState)
        {
            case EnemyState.Idle:
                FindClosestTarget();
                if (target != null) ChangeState(EnemyState.MoveToTarget);
                break;

            case EnemyState.MoveToTarget:
                HandleMovement();
                break;

            case EnemyState.Attacking:
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    AttackTarget();
                    lastAttackTime = Time.time;
                }
                break;

            case EnemyState.TakingDamage:
                break;

            case EnemyState.Dead:
                break;
        }
    }

    protected virtual void ChangeState(EnemyState newState)
    {
        currentState = newState;
        anim.SetBool(MoveBool, newState == EnemyState.MoveToTarget);
    }

    protected virtual void FindClosestTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var p in players)
        {
            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = p.transform;
            }
        }

        target = closest;
    }

    protected virtual void HandleMovement()
    {
        if (target == null)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        float dist = Vector2.Distance(transform.position, target.position);

        if (dist > detectionRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        if (dist <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
            return;
        }

        Vector2 dir = (target.position - transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
        Rotate(dir.x);
    }

    protected virtual void Rotate(float directionX)
    {
        transform.localScale = new Vector3(Mathf.Sign(directionX) * -1, 1, 1);
    }

    protected virtual void AttackTarget()
    {
        if (target == null) return;

        anim.SetTrigger(AttackTrigger);

        if (target.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(attackDamage);
        }
    }

    public virtual void TakeDamage(int damage, bool isCrit = false)
    {
        if (isDead) return;

        currentHealth -= damage;
        isTakingDamage = true;
        ChangeState(EnemyState.TakingDamage);

        if (!skipHurtAnimation) anim.SetTrigger(DamagedTrigger);

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

    public void ResetEnemy()
    {
        currentHealth = maxHealth;
        isDead = false;
        isTakingDamage = false;
        enabled = true;
        GetComponent<Collider2D>().enabled = true;
        enemyHealthUI?.UpdateHealth(currentHealth);
    }

    protected virtual void EndDamageStun()
    {
        isTakingDamage = false;
        if (!isDead)
            ChangeState(target != null ? EnemyState.MoveToTarget : EnemyState.Idle);
    }

    protected virtual void Die()
    {
        isDead = true;
        OnDeath?.Invoke();
        OnEnemyDefeated?.Invoke(50);

        anim.SetTrigger(DieTrigger);
        ChangeState(EnemyState.Dead);
        QuestManager.Instance.ReportProgress(ObjectiveType.KillEnemies, enemyName, 1);
        enemyHealthUI?.HideUI();

        GetComponent<Collider2D>().enabled = false;
        EnemyTracker.Instance.Unregister(this);

        StartCoroutine(DisableAfterDelay(timeDieDelay));
    }

    protected virtual IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    protected virtual void SpawnBloodVFX()
    {
        Vector3 spawnPos = transform.position + bloodVFXOffset;
        GameObject vfx = ObjectPooler.Instance.Get("Blood", RefVFX.Instance.bloodVfxPrefab, spawnPos, Quaternion.identity);
        Vector3 scale = vfx.transform.localScale;
        scale.x = Mathf.Sign(transform.localScale.x) * Mathf.Abs(scale.x);
        vfx.transform.localScale = scale;
    }

    public virtual void ApplyLevelData(EnemyLevelData data)
    {
        maxHealth = data.maxHealth;
        attackDamage = data.attackDamage;
        moveSpeed = data.moveSpeed;
        attackRange = data.attackRange;
        detectionRange = data.detectionRange;
        attackCooldown = data.attackCooldown;
        enemyLevel = data.level;
    }

    public virtual void DealDamageToTarget()
    {
        if (target == null) return;
        if (target.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(attackDamage);
        }
    }

#if UNITY_EDITOR
    [Button("Auto Setup Rigidbody, Collider, Layer & UnitRoot")]
    protected virtual void AutoAddRigidbodyAndCollider()
    {
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            Debug.Log("Rigidbody2D added and configured.");
        }

        if (GetComponent<BoxCollider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
            Debug.Log("BoxCollider2D added.");
        }

        gameObject.tag = "Enemy";
        Debug.Log("Tag set to 'Enemy'.");

        if (!IsTagDefined("Enemy"))
        {
            Debug.LogWarning("Tag 'Enemy' is not defined in Tag Manager. Please define it manually.");
        }

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
        {
            gameObject.layer = enemyLayer;
            Debug.Log("Layer set to 'Enemy'.");
        }
        else
        {
            Debug.LogWarning("Layer 'Enemy' is not defined. Please add it manually in the Tag Manager.");
        }

        Transform unitRoot = transform.Find("UnitRoot");
        if (unitRoot != null)
        {
            if (unitRoot.GetComponent<CommonAnimationEvents>() == null)
            {
                unitRoot.gameObject.AddComponent<CommonAnimationEvents>();
                Debug.Log("CommonAnimationEvents component added to UnitRoot.");
            }
            else
            {
                Debug.Log("UnitRoot already has CommonAnimationEvents.");
            }
        }
        else
        {
            Debug.LogWarning("Child named 'UnitRoot' not found under this enemy.");
        }
    }

    protected virtual bool IsTagDefined(string tag)
    {
        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
        {
            if (UnityEditorInternal.InternalEditorUtility.tags[i] == tag)
                return true;
        }

        return false;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        float healthPercent = Application.isPlaying && maxHealth > 0 ? (float)currentHealth / maxHealth : 1f;
        Vector3 barStart = transform.position + Vector3.up * 1.2f;
        Vector3 barEnd = barStart + Vector3.right * healthPercent;
        Gizmos.DrawLine(barStart, barEnd);

        Gizmos.color = Color.cyan;
        Vector3 forwardDir = transform.right * Mathf.Sign(transform.localScale.x);
        Gizmos.DrawRay(transform.position, forwardDir * 0.8f);
    }
#endif
}
