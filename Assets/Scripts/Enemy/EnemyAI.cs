using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
    protected static readonly int MoveBool = Animator.StringToHash("1_Move");
    protected static readonly int AttackTrigger = Animator.StringToHash("2_Attack");
    private static readonly int DamagedTrigger = Animator.StringToHash("3_Damaged");
    private static readonly int DieTrigger = Animator.StringToHash("4_Death");

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] protected float attackCooldown = 1f;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] protected int attackDamage = 10;
    [SerializeField] private float damagedStunTime = 0.3f;
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private int goldDropAmount = 10;

    protected Transform player;
    protected Animator anim;
    protected float lastAttackTime;
    private int currentHealth;
    protected bool isDead = false;
    protected bool isTakingDamage = false;

    public static event Action<float> OnEnemyDefeated;
    public EnemyHealthUI enemyHealthUI;
    public int MaxHealth => maxHealth;

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        anim = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (isTakingDamage) return;

        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer > attackRange)
            {
                MoveTowardsPlayer();
            }
            else
            {
                AttackPlayer();
            }
        }
        else
        {
            anim.SetBool(MoveBool, false);
        }
    }

    void MoveTowardsPlayer()
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
    }


    protected virtual void AttackPlayer()
    {
        if (isTakingDamage) return;

        // Đảm bảo quay mặt
        if (player.position.x > transform.position.x)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            transform.rotation = Quaternion.Euler(0, 180, 0);

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            anim.SetBool(MoveBool, false);
            anim.SetTrigger(AttackTrigger);
            lastAttackTime = Time.time;

            if (player.TryGetComponent(out PlayerHealth playerHealth))
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }


    public void TakeDamage(int damage , bool isCrit = false)
    {
        if (isDead) return;

        currentHealth -= damage;
        anim.SetTrigger(DamagedTrigger);
        isTakingDamage = true;

        string damageText = isCrit ? $"CRIT -{damage}" : $"-{damage}";
        Color damageColor = isCrit ? Color.red : Color.white;

        FloatingTextSpawner.Instance.SpawnText(
            damageText,
            transform.position + Vector3.up * .5f,
            damageColor);
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            Invoke(nameof(EndDamageStun), damagedStunTime);
        }
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


    protected void Die()
    {
        if (isDead) return;
        isDead = true;
        anim.SetTrigger(DieTrigger);
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        
        Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0f, 0);
        ObjectPooler.Instance.Get("Gold", goldPrefab, spawnPos, Quaternion.identity);

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
