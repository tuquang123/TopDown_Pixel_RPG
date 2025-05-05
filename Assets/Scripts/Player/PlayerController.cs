using UnityEngine;

public class PlayerController : MonoBehaviour , IGameEventListener
{
    protected static readonly int AttackTrigger = Animator.StringToHash("2_Attack");
    protected static readonly int MoveBool = Animator.StringToHash("1_Move");

    [SerializeField] protected float attackSpeed = 1f;
    [SerializeField] protected float detectionRange = 3f;
    [SerializeField] protected float attackRange = 1f;
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected float attackRadius = 0.8f;
    [SerializeField] protected GameObject slashVFX;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected Transform targetEnemy;
    protected float lastAttackTime;
    public Vector2 moveInput;
    protected PlayerStats stats;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        stats = GetComponent<PlayerStats>();
    }
    public void OnEventRaised()
    {
        Debug.Log("Người chơi đã dùng ngựa.");
        anim = GetComponentInChildren<Animator>();
    }

    private void OnEnable() => GameEvents.OnUpdateAnimation.RegisterListener(this);
    private void OnDisable() => GameEvents.OnUpdateAnimation.UnregisterListener(this);

    public Vector2 GetMoveInput()
    {
        // Joystick
        float joystickX = UltimateJoystick.GetHorizontalAxis("Move");
        float joystickY = UltimateJoystick.GetVerticalAxis("Move");

        // Bàn phím
        float keyboardX = Input.GetKey(KeyCode.A) ? -1 : Input.GetKey(KeyCode.D) ? 1 : 0;
        float keyboardY = Input.GetKey(KeyCode.S) ? -1 : Input.GetKey(KeyCode.W) ? 1 : 0;

        // Tổng hợp lại
        Vector2 combined = new Vector2(joystickX + keyboardX, joystickY + keyboardY);

        // Giới hạn lại trong khoảng -1 đến 1 nếu cần
        return combined.magnitude > 1 ? combined.normalized : combined;
    }
    
    protected virtual void Update()
    {
        moveInput = GetMoveInput();

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
            else
            {
                rb.linearVelocity = Vector2.zero;
                anim.SetBool(MoveBool, false);
            }
        }
    }

    public bool IsDashing => GetComponent<PlayerDash>()?.IsDashing == true;

    protected virtual void FixedUpdate()
    {
        if (IsDashing) return; // ĐANG DASH => không can thiệp

        if (moveInput.magnitude > 0.01f)
        {
            float moveSpeed = stats.speed.Value;
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
        float moveSpeed = stats.speed.Value;
        rb.linearVelocity = moveInput.normalized * moveSpeed;
        RotateCharacter(moveInput.x);
    }
    protected virtual void MoveToAttackPosition()
    {
        if (targetEnemy == null) return;

        Vector2 playerPos = transform.position;
        Vector2 enemyPos = targetEnemy.position;

        float yDiff = Mathf.Abs(playerPos.y - enemyPos.y);
        float xDiff = Mathf.Abs(playerPos.x - enemyPos.x);
        float moveSpeed = stats.speed.Value;

        // Ngưỡng sai lệch nhỏ để chấp nhận "đúng trục Y"
        float yTolerance = 0.1f;

        if (yDiff > yTolerance)
        {
            // Di chuyển để canh đúng trục Y trước
            Vector2 directionY = new Vector2(0, enemyPos.y - playerPos.y).normalized;
            rb.linearVelocity = directionY * moveSpeed;

            // Không quay trong khi di chuyển trục Y
            anim.SetBool(MoveBool, true);
        }
        else if (xDiff > attackRange * 0.8f)
        {
            // Khi đã gần đúng trục Y, thì canh trục X
            Vector2 directionX = new Vector2(enemyPos.x - playerPos.x, 0).normalized;
            rb.linearVelocity = directionX * moveSpeed;

            RotateCharacter(directionX.x);
            anim.SetBool(MoveBool, true);
        }
        else
        {
            // Đã vào vị trí chuẩn, dừng lại và tấn công
            rb.linearVelocity = Vector2.zero;
            anim.SetBool(MoveBool, false);
            FaceEnemy();

            if (Time.time - lastAttackTime >= 1f / stats.attackSpeed.Value)
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
                int finalDamage = (int)stats.attack.Value;

                // Tính chí mạng
                float roll = Random.Range(0f, 100f);
                bool isCrit = roll < stats.GetCritChance();
                if (isCrit)
                {
                    finalDamage = Mathf.RoundToInt(finalDamage * 1.5f);
                    Debug.Log("Đòn chí mạng!");
                }

                // Gây sát thương cho enemy
                enemy.GetComponent<EnemyAI>()?.TakeDamage(finalDamage, isCrit);

                // Hút máu theo sát thương gây ra
                stats.HealFromLifeSteal(finalDamage);
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
        if (direction < 0)
            transform.localScale = new Vector3(1, 1, 1); // Quay trái
        else if (direction > 0)
            transform.localScale = new Vector3(-1, 1, 1);  // Quay phải
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
