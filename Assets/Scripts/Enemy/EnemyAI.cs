using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
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
    protected Transform player;

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

    public static event Action<float> OnEnemyDefeated;

    public bool IsDead => isDead;
    public int MaxHealth => maxHealth;
    public string EnemyName => enemyName;
    public int EnemyLevel => enemyLevel;

    protected EnemyHealthUI enemyHealthUI;

    public EnemyHealthUI EnemyHealthUI
    {
        get => enemyHealthUI;
        set => enemyHealthUI = value;
    }

    protected virtual void Start()
    {
        player = RefVFX.Instance.playerPrefab.transform;
        anim = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;
        
        EnemyTracker.Instance.Register(this);
    }

    private void Update()
    {
        if (isDead || isTakingDamage || isKnockbacked) return;

        if (player.TryGetComponent(out PlayerStats playerStats) && playerStats.isDead)
        {
            anim.SetBool(MoveBool, false);
            return;
        }

        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange)
        {
            MoveToAttackPosition();
            RotateEnemy(player.position.x - transform.position.x);
        }
        else
        {
            anim.SetBool(MoveBool, false);
        }
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
        attackRange = data.attackRange;
        detectionRange = data.detectionRange;
        attackCooldown = data.attackCooldown;
        enemyLevel = data.level;
    }

    protected virtual void MoveToAttackPosition()
    {
        Vector2 enemyPos = transform.position;
        Vector2 playerPos = player.position;

        float yDiff = Mathf.Abs(enemyPos.y - playerPos.y);
        float xDiff = Mathf.Abs(enemyPos.x - playerPos.x);
        float yTolerance = 0.1f;

        if (yDiff > yTolerance)
        {
            Vector2 directionY = new Vector2(0, playerPos.y - enemyPos.y).normalized;
            transform.position += (Vector3)(directionY * moveSpeed * Time.deltaTime);
            anim.SetBool(MoveBool, true);
        }
        else if (xDiff > attackRange * 0.8f)
        {
            Vector2 directionX = new Vector2(playerPos.x - enemyPos.x, 0).normalized;
            transform.position += (Vector3)(directionX * moveSpeed * Time.deltaTime);
            anim.SetBool(MoveBool, true);
            RotateEnemy(directionX.x);
        }
        else
        {
            anim.SetBool(MoveBool, false);
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                AttackPlayer();
            }
        }
    }

    protected void RotateEnemy(float direction)
    {
        transform.localScale = new Vector3(Mathf.Sign(direction) * -1, 1, 1);
    }

    public void DealDamageToPlayer()
    {
        if (player == null) return;
        if (player.TryGetComponent(out PlayerStats playerStats) && !playerStats.isDead)
        {
            playerStats.TakeDamage(attackDamage);
        }
    }

    protected virtual void AttackPlayer()
    {
        if (player.TryGetComponent(out PlayerStats playerStats) && playerStats.isDead) return;
        if (isTakingDamage) return;

        RotateEnemy(player.position.x - transform.position.x);

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            anim.SetBool(MoveBool, false);
            anim.SetTrigger(AttackTrigger);
            lastAttackTime = Time.time;
        }
    }

    public virtual void TakeDamage(int damage, bool isCrit = false)
    {
        if (isDead) return;

        currentHealth -= damage;
        enemyHealthUI?.UpdateHealth(currentHealth);

        if (!skipHurtAnimation)
            anim.SetTrigger(DamagedTrigger);

        isTakingDamage = true;

        string damageText = isCrit ? $"CRIT -{damage}" : $"-{damage}";
        Color damageColor = isCrit ? new Color(1f, 0.84f, 0.2f) : Color.white;

        FloatingTextSpawner.Instance.SpawnText(damageText, transform.position + Vector3.up * 0.8f, damageColor);
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
            RefVFX.Instance.bloodVfxPrefab.name,
            RefVFX.Instance.bloodVfxPrefab,
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
        currentHealth = maxHealth;
        isDead = false;
        isTakingDamage = false;
        enabled = true;
        GetComponent<Collider2D>().enabled = true;
        enemyHealthUI?.UpdateHealth(currentHealth);
    }

    public event Action OnDeath;

    protected virtual void Die()
    {
        if (isDead) return;
        OnDeath?.Invoke();
        isDead = true;
        anim.SetTrigger(DieTrigger);
        GetComponent<Collider2D>().enabled = false;
        enabled = false;

        Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0f, 0);
        ObjectPooler.Instance.Get("Gold", RefVFX.Instance.goldPrefab, spawnPos, Quaternion.identity);

        if (enemyHealthUI != null)
        {
            Destroy(enemyHealthUI.gameObject);
            enemyHealthUI = null;
        }
        
        enemyHealthUI?.HideUI();
        EnemyTracker.Instance.Unregister(this);
        
        StartCoroutine(DisableAfterDelay(timeDieDelay));
        OnEnemyDefeated?.Invoke(50);
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    private IEnumerator ApplyKnockback()
    {
        if (player == null) yield break;

        Vector2 knockDir = (transform.position - player.position).normalized;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            isKnockbacked = true;
            rb.linearVelocity = knockDir * knockForce;
        }

        yield return new WaitForSeconds(knockDuration);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        isKnockbacked = false;
    }

    protected static readonly int MoveBool = Animator.StringToHash("1_Move");
    protected static readonly int AttackTrigger = Animator.StringToHash("2_Attack");
    private static readonly int DamagedTrigger = Animator.StringToHash("3_Damaged");
    private static readonly int DieTrigger = Animator.StringToHash("4_Death");

#if UNITY_EDITOR
    [Button("Auto Setup Rigidbody, Collider, Layer & UnitRoot")]
    private void AutoAddRigidbodyAndCollider()
    {
        // Add Rigidbody2D nếu chưa có
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            Debug.Log("Rigidbody2D added and configured.");
        }

        // Add BoxCollider2D nếu chưa có
        if (GetComponent<BoxCollider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
            Debug.Log("BoxCollider2D added.");
        }

        // Gán tag "Enemy"
        gameObject.tag = "Enemy";
        Debug.Log("Tag set to 'Enemy'.");

        if (!IsTagDefined("Enemy"))
        {
            Debug.LogWarning("Tag 'Enemy' is not defined in Tag Manager. Please define it manually.");
        }

        // Gán Layer "Enemy" nếu tồn tại
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

        // Tìm object con tên "UnitRoot" và gắn CommonAnimationEvents nếu cần
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

    /// <summary>
    /// Kiểm tra tag đã tồn tại trong TagManager chưa
    /// </summary>
    private bool IsTagDefined(string tag)
    {
        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
        {
            if (UnityEditorInternal.InternalEditorUtility.tags[i] == tag)
                return true;
        }

        return false;
    }
    private void OnDrawGizmosSelected()
    {
        // Vẽ detection range (vòng xanh)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Vẽ attack range (vòng đỏ)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Vẽ máu còn lại (thanh ngang)
        Gizmos.color = Color.yellow;
        float healthPercent = Application.isPlaying && maxHealth > 0 ? (float)currentHealth / maxHealth : 1f;
        Vector3 barStart = transform.position + Vector3.up * 1.2f;
        Vector3 barEnd = barStart + Vector3.right * healthPercent;
        Gizmos.DrawLine(barStart, barEnd);

        // Vẽ hướng đang quay mặt
        Gizmos.color = Color.cyan;
        Vector3 forwardDir = transform.right * Mathf.Sign(transform.localScale.x);
        Gizmos.DrawRay(transform.position, forwardDir * 0.8f);
    }

#endif
}