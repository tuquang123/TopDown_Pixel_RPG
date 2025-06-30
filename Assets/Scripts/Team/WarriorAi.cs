using UnityEngine;

public class WarriorAi : MonoBehaviour
{
    private static readonly int MoveBool = Animator.StringToHash("1_Move");
    private static readonly int AttackTrigger = Animator.StringToHash("8_Attack");

    [SerializeField] private float followDistance = 1.5f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1.5f;

    private Transform player;
    private PlayerController playerController;
    private Transform targetEnemy;
    private Transform targetDestructible;
    private Rigidbody2D rb;
    private Animator anim;
    private float lastAttackTime;
    private float lastMoveChangeTime = 0f;
    private float moveChangeDelay = 0.2f;

    private AllyStats stats;

    public void Setup(Hero hero)
    {
        name = hero.data.name;
        stats = GetComponent<AllyStats>();
        stats.Initialize(hero.currentStats, hero.data.name);
    }

    private void Start()
    {
        stats = GetComponent<AllyStats>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        player = RefVFX.Instance.playerPrefab.transform;
        playerController = player?.GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (player == null || playerController == null) return;

        if (playerController.IsMoving())
        {
            FollowPlayer();
        }
        else
        {
            FindClosestEnemy();

            if (targetEnemy != null)
            {
                MoveToAttackPosition(targetEnemy);
            }
            else
            {
                FindClosestDestructible();

                if (targetDestructible != null)
                {
                    MoveToAttackPosition(targetDestructible);
                }
                else
                {
                    rb.linearVelocity = Vector2.zero;
                    anim.SetBool(MoveBool, false);
                }
            }
        }
    }

    private void FollowPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool isPlayerMoving = playerController.MoveInput.magnitude > 0.1f;

        bool isMoving = false;

        if (distanceToPlayer > followDistance || isPlayerMoving)
        {
            Vector2 direction = (player.position - transform.position).normalized;

            if (distanceToPlayer > followDistance)
            {
                rb.linearVelocity = direction * stats.MoveSpeed;
                RotateCharacter(direction.x);
                isMoving = true;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (Time.time - lastMoveChangeTime >= moveChangeDelay)
        {
            if (anim.GetBool(MoveBool) != isMoving)
            {
                anim.SetBool(MoveBool, isMoving);
                lastMoveChangeTime = Time.time;
            }
        }
    }

    private void MoveToAttackPosition(Transform target)
    {
        Vector2 myPos = transform.position;
        Vector2 targetPos = target.position;

        float yDiff = Mathf.Abs(myPos.y - targetPos.y);
        float xDiff = Mathf.Abs(myPos.x - targetPos.x);
        float yTolerance = 0.1f;

        if (yDiff > yTolerance)
        {
            Vector2 dirY = new Vector2(0, targetPos.y - myPos.y).normalized;
            rb.linearVelocity = dirY * stats.MoveSpeed;
            anim.SetBool(MoveBool, true);
        }
        else if (xDiff > attackRange * 0.8f)
        {
            Vector2 dirX = new Vector2(targetPos.x - myPos.x, 0).normalized;
            rb.linearVelocity = dirX * stats.MoveSpeed;
            RotateCharacter(dirX.x);
            anim.SetBool(MoveBool, true);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool(MoveBool, false);
            RotateCharacter(targetPos.x - myPos.x);

            if (Time.time - lastAttackTime >= 1f / stats.AttackSpeed)
            {
                AttackEnemy();
            }
        }
    }

    private void AttackEnemy()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger(AttackTrigger);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Enemy", "Destructible"));

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out EnemyAI enemy) && !enemy.IsDead)
            {
                enemy.TakeDamage(stats.Attack);
            }
            else if (hit.TryGetComponent(out DestructibleObject destructible))
            {
                destructible.Hit();
            }
        }
    }

    private void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float minDistance = detectionRange;
        targetEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                targetEnemy = enemy.transform;
            }
        }
    }

    private void FindClosestDestructible()
    {
        GameObject[] destructibles = GameObject.FindGameObjectsWithTag("Destructible");
        float minDistance = detectionRange;
        targetDestructible = null;

        foreach (GameObject obj in destructibles)
        {
            float distance = Vector2.Distance(transform.position, obj.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                targetDestructible = obj.transform;
            }
        }
    }

    private void RotateCharacter(float direction)
    {
        transform.rotation = Quaternion.Euler(0, direction < 0 ? 180 : 0, 0);
    }

    public bool IsMoving()
    {
        return rb.linearVelocity.magnitude > 0.1f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
