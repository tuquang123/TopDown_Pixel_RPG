using UnityEngine;

public class ArcherAI : MonoBehaviour 
{
    private static readonly int MoveBool = Animator.StringToHash("1_Move");
    private static readonly int ShotTrigger = Animator.StringToHash("7_Shoot");

    [SerializeField] private float followDistance = 1.5f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint;

    private Transform player;
    private PlayerController playerController;
    private Transform target;
    private Rigidbody2D rb;
    private Animator anim;
    private AllyStats stats;

    private float lastAttackTime;
    private float lastMoveChangeTime = 0f;
    private float moveChangeDelay = 0.2f;

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
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerController = player?.GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (player == null || playerController == null || stats.IsDead) return;

        if (playerController.IsMoving())
        {
            FollowPlayer();
        }
        else
        {
            FindClosestTarget();

            if (target != null)
            {
                MoveToAttackPosition();
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                anim.SetBool(MoveBool, false);
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

    private void MoveToAttackPosition()
    {
        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        if (distanceToTarget > attackRange * 0.8f)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            rb.linearVelocity = direction * stats.MoveSpeed;
            RotateCharacter(direction.x);
            anim.SetBool(MoveBool, true);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool(MoveBool, false);
            FaceTarget();

            if (Time.time - lastAttackTime >= 1f / stats.AttackSpeed)
            {
                AttackTarget();
            }
        }
    }

    private void AttackTarget()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger(ShotTrigger);

        if (target.TryGetComponent(out EnemyAI enemy) && !enemy.IsDead)
        {
            GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);
            arrow.GetComponent<Arrow>()?.SetTarget(target, stats.Attack);
        }
        else if (target.TryGetComponent(out DestructibleObject destructible))
        {
            destructible.Hit();
        }
    }

    private void FindClosestTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] destructibles = GameObject.FindGameObjectsWithTag("Destructible");

        float minDistance = detectionRange;
        target = null;

        foreach (GameObject enemy in enemies)
        {
            if (enemy.TryGetComponent(out EnemyAI enemyAI) && enemyAI.IsDead) continue;

            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                target = enemy.transform;
            }
        }

        if (target == null)
        {
            foreach (GameObject obj in destructibles)
            {
                float dist = Vector2.Distance(transform.position, obj.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    target = obj.transform;
                }
            }
        }
    }

    private void RotateCharacter(float direction)
    {
        transform.rotation = Quaternion.Euler(0, direction < 0 ? 180 : 0, 0);
    }

    private void FaceTarget()
    {
        if (target == null) return;
        RotateCharacter(target.position.x - transform.position.x);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
