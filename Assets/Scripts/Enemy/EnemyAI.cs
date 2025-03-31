using System;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private static readonly int MoveBool = Animator.StringToHash("1_Move");
    private static readonly int AttackTrigger = Animator.StringToHash("2_Attack");
    private static readonly int DamagedTrigger = Animator.StringToHash("3_Damaged");
    private static readonly int DieTrigger = Animator.StringToHash("4_Death");

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float damagedStunTime = 0.3f;

    private Transform player;
    private Animator anim;
    private float lastAttackTime;
    private int currentHealth;
    private bool isDead = false;
    private bool isTakingDamage = false;
    
    public static event Action<float> OnEnemyDefeated;

    void Start()
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

        anim.SetBool(MoveBool, true);
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        transform.rotation = player.position.x > transform.position.x ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
    }

    void AttackPlayer()
    {
        if (isTakingDamage) return;

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

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        anim.SetTrigger(DamagedTrigger);
        isTakingDamage = true;

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

    void Die()
    {
        if (isDead) return;

        isDead = true;
        anim.SetTrigger(DieTrigger);
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        Destroy(gameObject, .7f);
        
        OnEnemyDefeated?.Invoke(50);

    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
