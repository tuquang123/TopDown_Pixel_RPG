using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
    protected static readonly int MoveBool = Animator.StringToHash("1_Move");
    protected static readonly int AttackTrigger = Animator.StringToHash("2_Attack");
    private static readonly int DamagedTrigger = Animator.StringToHash("3_Damaged");
    private static readonly int DieTrigger = Animator.StringToHash("4_Death");
    
    [BoxGroup("Movement Settings"), LabelText("Move Speed"), Range(0f, 10f)]
    [SerializeField]
    protected float moveSpeed = 3f;

    [BoxGroup("Combat Settings"), LabelText("Attack Range"), Range(0.1f, 10f)]
    [SerializeField] protected float attackRange = 1.5f;

    [BoxGroup("Combat Settings"), LabelText("Detection Range"), Range(0.1f, 20f)]
    [SerializeField] protected float detectionRange = 5f;

    [BoxGroup("Combat Settings"), LabelText("Attack Cooldown"), Range(0f, 10f)]
    [SerializeField] protected float attackCooldown = 1f;

    [BoxGroup("Stats"), LabelText("Max Health")]
    [SerializeField] protected int maxHealth = 100;

    [BoxGroup("Stats"), LabelText("Attack Damage")]
    [SerializeField] protected int attackDamage = 10;

    [BoxGroup("Damage Settings"), LabelText("Stun Time After Hit"), Range(0f, 2f)]
    [SerializeField] private float damagedStunTime = 0.3f;
    
    [SerializeField] float knockForce = 3f;
    [SerializeField] float knockDuration = 0.3f;

    [FoldoutGroup("Runtime Debug"), ReadOnly] protected Transform player;
    [FoldoutGroup("Runtime Debug"), ReadOnly] protected Animator anim;
    [FoldoutGroup("Runtime Debug"), ReadOnly] protected float lastAttackTime;
    [FoldoutGroup("Runtime Debug"), ReadOnly] protected int currentHealth;
    [FoldoutGroup("Runtime Debug"), ReadOnly] protected bool isDead = false;
    [FoldoutGroup("Runtime Debug"), ReadOnly] protected bool isTakingDamage = false;

    public static event Action<float> OnEnemyDefeated;

    [FoldoutGroup("UI"), LabelText("Health UI"), SerializeField]
    protected EnemyHealthUI enemyHealthUI;

    public EnemyHealthUI EnemyHealthUI
    {
        get => enemyHealthUI;
        set => enemyHealthUI = value;
    }

    public int MaxHealth => maxHealth;

    protected virtual void Start()
    {
        player = RefVFX.Instance.playerPrefab.transform;
        anim = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isDead || isTakingDamage || isKnockbacked) return;

        if (player.TryGetComponent(out PlayerStats playerStats))
        {
            if (playerStats.isDead)
            {
                anim.SetBool(MoveBool, false);
                return;
            }
        }
        if (isDead || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (isTakingDamage) return;

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

    /*void MoveTowardsPlayer()
    {
        if (Time.time - lastAttackTime < attackCooldown)
        {
            anim.SetBool(MoveBool, false);
            return;
        }

        // Tính hướng và di chuyển đến gần player
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);

        // Chỉ xoay trái/phải dựa vào X
        if (player.position.x > transform.position.x)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            transform.rotation = Quaternion.Euler(0, 180, 0);

        anim.SetBool(MoveBool, true);
    }*/
    protected virtual void MoveToAttackPosition()
    {
        Vector2 enemyPos = transform.position;
        Vector2 playerPos = player.position;

        float yDiff = Mathf.Abs(enemyPos.y - playerPos.y);
        float xDiff = Mathf.Abs(enemyPos.x - playerPos.x);

        float yTolerance = 0.1f;

        // Nếu chưa canh được trục Y thì di chuyển theo Y
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
            // Gần đủ vị trí rồi thì tấn công
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                AttackPlayer();
            }
        }
    }
    protected void RotateEnemy(float direction)
    {
        if (direction < 0)
            transform.localScale = new Vector3(1, 1, 1); // Quay trái
        else if (direction > 0)
            transform.localScale = new Vector3(-1, 1, 1); // Quay phải
    }

    
    protected virtual void AttackPlayer()
    {
        if (player.TryGetComponent(out PlayerStats playerStats))
        {
            if(playerStats.isDead) return;
        }
        
        if (isTakingDamage) return;

        RotateEnemy(player.position.x - transform.position.x);
        
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            anim.SetBool(MoveBool, false);
            anim.SetTrigger(AttackTrigger);
            lastAttackTime = Time.time;

            if (player.TryGetComponent(out PlayerStats playerHealth))
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }
    
    public virtual void TakeDamage(int damage, bool isCrit = false)
    {
        if (isDead) return;
        
        enemyHealthUI?.UpdateHealth(currentHealth);

        currentHealth -= damage;
        anim.SetTrigger(DamagedTrigger);
        isTakingDamage = true;

        string damageText = isCrit ? $"CRIT -{damage}" : $"-{damage}";
        Color damageColor = isCrit ? Color.red : Color.white;

        FloatingTextSpawner.Instance.SpawnText(
            damageText,
            transform.position + Vector3.up * .5f,
            damageColor);

        // Knockback (add this line)
        StartCoroutine(ApplyKnockback());
        

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            Invoke(nameof(EndDamageStun), damagedStunTime);
        }
    }

    private bool isKnockbacked = false;

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
    
    void EndDamageStun()
    {
        isTakingDamage = false;
    }
    public void ResetEnemy()
    {
        currentHealth = maxHealth;
        isDead = false;
        isTakingDamage = false;
        this.enabled = true;
        GetComponent<Collider2D>().enabled = true;
        //anim.Play("Idle"); // hoặc reset animation về mặc định
        enemyHealthUI?.UpdateHealth(currentHealth);
    }


    public event System.Action OnDeath;
    protected virtual void Die()
    {
        if (isDead) return;
        OnDeath?.Invoke();
        isDead = true;
        anim.SetTrigger(DieTrigger);
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        
        Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0f, 0);
        ObjectPooler.Instance.Get("Gold", RefVFX.Instance.goldPrefab, spawnPos, Quaternion.identity);

        if (enemyHealthUI != null)
        {
            Destroy(enemyHealthUI.gameObject); // hoặc pool nếu cần
            enemyHealthUI = null;
        }

        // Ẩn enemy thay vì destroy
        StartCoroutine(DisableAfterDelay(0.65f));
        OnEnemyDefeated?.Invoke(50);
    }
    
    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
