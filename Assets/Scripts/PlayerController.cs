using UnityEngine;

public class PlayerController : MonoBehaviour
{
    protected static readonly int AttackTrigger = Animator.StringToHash("2_Attack");
    protected static readonly int MoveBool = Animator.StringToHash("1_Move");

    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float attackSpeed = 1f;
    [SerializeField] protected float detectionRange = 3f;
    [SerializeField] protected float attackRange = 1f;
    [SerializeField] protected int attackDamage = 20;
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected float attackRadius = 0.8f;
    [SerializeField] protected GameObject slashVFX;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected Transform targetEnemy;
    protected float lastAttackTime;
    public Vector2 moveInput;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    protected virtual void Update()
    {
        moveInput.x = UltimateJoystick.GetHorizontalAxis("Move");
        moveInput.y = UltimateJoystick.GetVerticalAxis("Move");
        bool isMoving = moveInput.magnitude > 0.01f;

        anim.SetBool(MoveBool, isMoving);

        if (isMoving)
        {
            MovePlayer();  // Nếu player di chuyển, thì di chuyển
            targetEnemy = null;
        }
        else
        {
            FindClosestEnemy();  // Nếu không di chuyển, tìm và tấn công kẻ thù
            if (targetEnemy != null)
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

    protected virtual void FixedUpdate()
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

    protected virtual void MovePlayer()
    {
        rb.linearVelocity = moveInput.normalized * moveSpeed;
        RotateCharacter(moveInput.x);
    }

    protected virtual void MoveToAttackPosition()
    {
        if (targetEnemy == null) return;

        float distanceToEnemy = Vector2.Distance(transform.position, targetEnemy.position);

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

    protected virtual void AttackEnemy()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger(AttackTrigger);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                enemy.GetComponent<EnemyAI>()?.TakeDamage(attackDamage);
            }
        }
    }

    protected void FindClosestEnemy()
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

    public bool IsMoving()
    {
        return moveInput.magnitude > 0.01f;
    }

    protected void RotateCharacter(float direction)
    {
        transform.rotation = Quaternion.Euler(0, direction < 0 ? 180 : 0, 0);
    }

    protected void FaceEnemy()
    {
        if (targetEnemy == null) return;
        RotateCharacter(targetEnemy.position.x - transform.position.x);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}
