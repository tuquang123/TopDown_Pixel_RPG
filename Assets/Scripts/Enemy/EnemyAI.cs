using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum EnemyState
{
    Idle,
    Moving,
    Attacking,
    Damaged,
    Dead
}

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
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private int goldDropAmount = 10;

    private Transform player;
    private Animator anim;
    private float lastAttackTime;
    private int currentHealth;
    private bool isDead = false;
    private bool isTakingDamage = false;

    private EnemyState currentState;

    public static event Action<float> OnEnemyDefeated;
    public EnemyHealthUI enemyHealthUI;
    public int MaxHealth => maxHealth;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        anim = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;

        SetState(EnemyState.Idle);
    }

    void Update()
    {
        if (isDead || player == null) return;

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;
            case EnemyState.Moving:
                HandleMoveState();
                break;
            case EnemyState.Attacking:
                HandleAttackState();
                break;
            case EnemyState.Damaged:
                HandleDamagedState();
                break;
            case EnemyState.Dead:
                HandleDeadState();
                break;
        }
    }

    void HandleIdleState()
    {
        if (Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            SetState(EnemyState.Moving);
        }
    }

    async void HandleMoveState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            SetState(EnemyState.Attacking);
            return;
        }

        if (Time.time - lastAttackTime < attackCooldown)
        {
            anim.SetBool(MoveBool, false);
            return;
        }

        anim.SetBool(MoveBool, true);
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        transform.rotation = player.position.x > transform.position.x ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
    }

    async void HandleAttackState()
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

            // Use UniTask.Delay instead of coroutine here
            await UniTask.Delay(TimeSpan.FromSeconds(attackCooldown));
        }
        SetState(EnemyState.Idle);  // Return to idle state after attacking
    }

    void HandleDamagedState()
    {
        if (currentHealth <= 0)
        {
            SetState(EnemyState.Dead);
        }
        else
        {
            EndDamageStun().Forget();  // End damage stun asynchronously
        }
    }

    async void HandleDeadState()
    {
        if (isDead) return;

        isDead = true;
        anim.SetTrigger(DieTrigger);
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        if (enemyHealthUI != null)
        {
            Destroy(enemyHealthUI.gameObject);
            enemyHealthUI = null;
        }

        Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0f, 0);
        ObjectPooler.Instance.GetToPool("Gold", goldPrefab, spawnPos, Quaternion.identity);

        // Using UniTask.Delay here
        await DisableAfterDelay(0.65f);
        OnEnemyDefeated?.Invoke(50);
    }

    private async UniTask DisableAfterDelay(float delay)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));
        gameObject.SetActive(false);
    }

    // Using async void with UniTask to replace Invoke
    private async UniTask EndDamageStun()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(damagedStunTime));  // Wait for stun time
        isTakingDamage = false;
        if (currentHealth > 0)
        {
            SetState(EnemyState.Idle);
        }
    }

    void SetState(EnemyState newState)
    {
        currentState = newState;
        anim.SetBool(MoveBool, newState == EnemyState.Moving);
        anim.SetTrigger(AttackTrigger);  // Or other triggers based on state
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        anim.SetTrigger(DamagedTrigger);
        isTakingDamage = true;

        enemyHealthUI.UpdateHealth(currentHealth);
        FloatingTextSpawner.Instance.SpawnText(
            "-" + damage,
            transform.position + Vector3.up * .5f,
            Color.white);

        SetState(EnemyState.Damaged);
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
