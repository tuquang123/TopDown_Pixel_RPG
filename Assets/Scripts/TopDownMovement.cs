using UnityEngine;

public class TopDownMovement : MonoBehaviour
{
    private static readonly int AttackTrigger = Animator.StringToHash("2_Attack");
    private static readonly int MoveBool = Animator.StringToHash("1_Move");

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private float detectionRange = 3f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.8f;
    [SerializeField] private GameObject slashVFX;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform targetEnemy;
    private float lastAttackTime;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        moveInput.x = UltimateJoystick.GetHorizontalAxis("Move");
        moveInput.y = UltimateJoystick.GetVerticalAxis("Move");
        bool isMoving = moveInput.magnitude > 0.01f;
        anim.SetBool(MoveBool, isMoving);

        if (isMoving)
        {
            MovePlayer();
            targetEnemy = null; 
        }
        else
        {
            FindClosestEnemy();
            if (targetEnemy != null)
            {
                MoveToAttackPosition();
            }
        }
    }

    void FixedUpdate()
    {
        if (moveInput.magnitude > 0.01f)
        {
            rb.linearVelocity = moveInput.normalized * moveSpeed;
        }
        else if (targetEnemy == null)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool(MoveBool, false);
        }
    }

    void MovePlayer()
    {
        rb.linearVelocity = moveInput.normalized * moveSpeed;
        RotateCharacter(moveInput.x);
    }

    void MoveToAttackPosition()
    {
        if (targetEnemy == null) return;

        float distanceToEnemy = Vector2.Distance(transform.position, targetEnemy.position);
        Debug.Log($"Target: {targetEnemy.name}, Distance: {distanceToEnemy}");

        if (distanceToEnemy > attackRange * 0.8f)
        {
            Vector2 direction = (targetEnemy.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
            RotateCharacter(targetEnemy.position.x - transform.position.x);
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

    void FaceEnemy()
    {
        if (targetEnemy == null) return;
        RotateCharacter(targetEnemy.position.x - transform.position.x);
    }

    void AttackEnemy()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger(AttackTrigger);
        slashVFX.SetActive(true);
        Invoke(nameof(DisableVFX), 0.3f);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                enemy.GetComponent<EnemyAI>()?.TakeDamage(attackDamage);
            }
        }
    }

    void DisableVFX()
    {
        slashVFX.SetActive(false);
    }

    void FindClosestEnemy()
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

        if (targetEnemy != null)
        {
            Debug.Log($"Closest Enemy: {targetEnemy.name} at distance {minDistance}");
        }
    }

    void RotateCharacter(float direction)
    {
        transform.rotation = Quaternion.Euler(0, direction < 0 ? 180 : 0, 0);
    }

    void OnDrawGizmos()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
