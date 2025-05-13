using UnityEngine;

public class ArcherAI : MonoBehaviour
{
    private static readonly int MoveBool = Animator.StringToHash("1_Move");
    private static readonly int ShotTrigger = Animator.StringToHash("7_Shoot");

    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float followDistance = 1.5f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private int attackDamage = 15;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint;

    private Transform player;
    private PlayerController playerController;
    private Transform targetEnemy;
    private Rigidbody2D rb;
    private Animator anim;
    private float lastAttackTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerController = player?.GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (player == null || playerController == null) return;

        if (playerController.IsMoving())
        {
            FollowPlayer();  // Chạy theo player nếu player đang di chuyển
        }
        else
        {
            // Nếu player không di chuyển, tìm và tấn công kẻ thù
            FindClosestEnemy();

            if (targetEnemy != null)
            {
                MoveToAttackPosition();  // Di chuyển tới vị trí tấn công
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                anim.SetBool(MoveBool, false);  // Dừng lại nếu không có kẻ thù
            }
        }
    }

    private float lastMoveChangeTime = 0f;
    private float moveChangeDelay = 0.2f; // Độ trễ nhỏ

    private void FollowPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool isPlayerMoving = playerController.MoveInput.magnitude > 0.1f;

        bool isMoving = false;

        if (distanceToPlayer > followDistance || isPlayerMoving)
        {
            Vector2 direction = (player.position - transform.position).normalized;

            if (distanceToPlayer > followDistance)
            {
                rb.linearVelocity = direction * moveSpeed;
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

        // Đảm bảo Animator chỉ thay đổi sau một khoảng thời gian nhất định
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
        if (targetEnemy == null) return;

        float distanceToEnemy = Vector2.Distance(transform.position, targetEnemy.position);

        if (distanceToEnemy > attackRange * 0.8f)
        {
            Vector2 direction = (targetEnemy.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
            RotateCharacter(direction.x);
            anim.SetBool(MoveBool, true);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool(MoveBool, false);
            FaceEnemy();

            if (Time.time - lastAttackTime >= 1f / attackSpeed)
            {
                AttackEnemy();
            }
        }
    }

    private void AttackEnemy()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger(ShotTrigger);

        GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);
        arrow.GetComponent<Arrow>()?.SetTarget(targetEnemy, attackDamage);
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

    private void RotateCharacter(float direction)
    {
        transform.rotation = Quaternion.Euler(0, direction < 0 ? 180 : 0, 0);
    }

    private void FaceEnemy()
    {
        if (targetEnemy == null) return;
        RotateCharacter(targetEnemy.position.x - transform.position.x);
    }
    private void OnDrawGizmos()
    {
        // Vẽ phạm vi phát hiện
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Vẽ phạm vi tấn công
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
