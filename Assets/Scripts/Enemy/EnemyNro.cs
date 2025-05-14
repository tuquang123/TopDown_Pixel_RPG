using System;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

public class EnemyNro : MonoBehaviour
{
    // Animator Parameters
    protected static readonly int MoveBool = Animator.StringToHash("1_Move");
    protected static readonly int AttackTrigger = Animator.StringToHash("2_Attack");
    private static readonly int DamagedTrigger = Animator.StringToHash("3_Damaged");
    private static readonly int DieTrigger = Animator.StringToHash("4_Death");

    [BoxGroup("Combat Settings"), SerializeField] protected float attackRange = 1.5f;
    [BoxGroup("Combat Settings"), SerializeField] protected float detectionRange = 5f;
    [BoxGroup("Combat Settings"), SerializeField] protected float attackCooldown = 1f;

    [BoxGroup("Movement Settings"), SerializeField] protected float moveSpeed = 3f;
    [SerializeField] private float maxChaseDistance = 6f;
    [SerializeField] private float recoveryDelayAfterHit = 0.4f;

    [BoxGroup("Stats"), SerializeField] protected int maxHealth = 100;
    [BoxGroup("Stats"), SerializeField] protected int attackDamage = 10;

    [BoxGroup("Damage Settings"), SerializeField] private float damagedStunTime = 0.3f;
    [SerializeField] private float knockForce = 3f;
    [SerializeField] private float knockDuration = 0.3f;

    [FoldoutGroup("Idle Patrol"), SerializeField] private bool patrolWhenIdle = true;
    [FoldoutGroup("Idle Patrol"), SerializeField] private float patrolDistance = 1f;
    [FoldoutGroup("Idle Patrol"), SerializeField] private float patrolSpeed = 1f;

    [FoldoutGroup("UI"), SerializeField] protected EnemyHealthUI enemyHealthUI;

    // Runtime
    protected Transform player;
    protected Animator anim;
    protected int currentHealth;
    protected float lastAttackTime;
    protected bool isDead;
    protected bool isTakingDamage;
    private bool isKnockbacked;
    private float lastDamagedTime;
    private Vector2 patrolOrigin;

    public static event Action<float> OnEnemyDefeated;

    public int MaxHealth => maxHealth;

    protected virtual void Start()
    {
        player = RefVFX.Instance.playerPrefab.transform;
        anim = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;
        patrolOrigin = transform.position;
    }

    protected virtual void Update()
    {
        if (isDead || isTakingDamage || isKnockbacked) return;
        if (player == null) return;

        if (player.TryGetComponent(out PlayerStats playerStats) && playerStats.isDead)
        {
            anim.SetBool(MoveBool, false);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (Time.time - lastDamagedTime < recoveryDelayAfterHit)
        {
            anim.SetBool(MoveBool, false);
            return;
        }

        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer <= maxChaseDistance)
            {
                MoveToAttackPosition();
                RotateToFace(player.position.x - transform.position.x);
            }
            else
            {
                anim.SetBool(MoveBool, false);
            }
        }
        else
        {
            if (patrolWhenIdle)
                IdlePatrol();
            else
                anim.SetBool(MoveBool, false);
        }
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
            Vector2 dirY = new Vector2(0, playerPos.y - enemyPos.y).normalized;
            transform.position += (Vector3)(dirY * moveSpeed * Time.deltaTime);
            anim.SetBool(MoveBool, true);
        }
        else if (xDiff > attackRange * 0.8f)
        {
            Vector2 dirX = new Vector2(playerPos.x - enemyPos.x, 0).normalized;
            transform.position += (Vector3)(dirX * moveSpeed * Time.deltaTime);
            anim.SetBool(MoveBool, true);
        }
        else
        {
            anim.SetBool(MoveBool, false);
            if (Time.time - lastAttackTime >= attackCooldown)
                AttackPlayer();
        }
    }

    protected virtual void AttackPlayer()
    {
        if (player.TryGetComponent(out PlayerStats stats) && stats.isDead) return;
        if (isTakingDamage) return;

        RotateToFace(player.position.x - transform.position.x);

        anim.SetBool(MoveBool, false);
        anim.SetTrigger(AttackTrigger);
        lastAttackTime = Time.time;

        if (player.TryGetComponent(out PlayerStats playerStats))
        {
            playerStats.TakeDamage(attackDamage);
        }
    }

    public virtual void TakeDamage(int damage, bool isCrit = false)
    {
        if (isDead) return;

        currentHealth -= damage;
        isTakingDamage = true;
        anim.SetTrigger(DamagedTrigger);
        lastDamagedTime = Time.time;

        FloatingTextSpawner.Instance.SpawnText(
            isCrit ? $"CRIT -{damage}" : $"-{damage}",
            transform.position + Vector3.up * .5f,
            isCrit ? Color.red : Color.white
        );

        StartCoroutine(ApplyKnockback());

        if (currentHealth <= 0)
            Die();
        else
            Invoke(nameof(EndDamageStun), damagedStunTime);
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
            rb.linearVelocity = Vector2.zero;

        isKnockbacked = false;
    }

    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;
        anim.SetTrigger(DieTrigger);
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0f);
        ObjectPooler.Instance.Get("Gold", RefVFX.Instance.goldPrefab, spawnPos, Quaternion.identity);

        if (enemyHealthUI)
        {
            Destroy(enemyHealthUI.gameObject);
            enemyHealthUI = null;
        }

        StartCoroutine(DisableAfterDelay(0.65f));
        OnEnemyDefeated?.Invoke(50);
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    protected void RotateToFace(float xDir)
    {
        transform.localScale = new Vector3(xDir < 0 ? 1 : -1, 1, 1);
    }

    private void EndDamageStun() => isTakingDamage = false;

    protected void IdlePatrol()
    {
        float move = Mathf.PingPong(Time.time * patrolSpeed, patrolDistance) - patrolDistance / 2f;
        Vector2 target = patrolOrigin + Vector2.right * move;
        transform.position = new Vector3(target.x, transform.position.y, transform.position.z);
        anim.SetBool(MoveBool, true);
        RotateToFace(move);
    }

    public void ResetEnemy()
    {
        currentHealth = maxHealth;
        isDead = false;
        isTakingDamage = false;
        this.enabled = true;
        GetComponent<Collider2D>().enabled = true;
        enemyHealthUI?.UpdateHealth(currentHealth);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
