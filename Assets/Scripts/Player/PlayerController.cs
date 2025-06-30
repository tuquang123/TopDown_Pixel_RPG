using UnityEngine;

public class PlayerController : MonoBehaviour, IGameEventListener
{
    protected static readonly int AttackTrigger = Animator.StringToHash("2_Attack");
    protected static readonly int MoveBool = Animator.StringToHash("1_Move");

    [SerializeField] protected float detectionRange = 3f;
    [SerializeField] protected float attackRange = 1f;
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected float attackRadius = 0.8f;
    [SerializeField] protected GameObject vfxDust;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected Transform targetEnemy;
    protected Transform targetDestructible;
    protected PlayerStats stats;
    protected Vector2 moveInput;
    protected float lastAttackTime;

    public Vector2 MoveInput => moveInput;
    public Transform GetTargetEnemy() => targetEnemy;
    public bool IsPlayerDie() => stats.isDead;
    public bool IsMoving() => moveInput.magnitude > 0.01f;
    public bool IsDashing => GetComponent<PlayerDash>()?.IsDashing == true;
    public bool IsAttacking => anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack");


    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        stats = GetComponent<PlayerStats>();
    }

    protected virtual void Update()
    {
        if (stats.isDead) return;

        moveInput = GetMoveInput();
        bool isMoving = IsMoving();
        anim.SetBool(MoveBool, isMoving);

        if (isMoving)
        {
            MovePlayer();
            targetEnemy = null;
            targetDestructible = null;
        }
        else
        {
            FindClosestEnemy();

            if (targetEnemy != null)
                MoveToTarget(targetEnemy, FaceEnemy);
            else
            {
                FindClosestDestructible();

                if (targetDestructible != null)
                    MoveToTarget(targetDestructible);
                else
                {
                    rb.linearVelocity = Vector2.zero;
                    anim.SetBool(MoveBool, false);
                }
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        if (stats.isDead || IsDashing) return;

        if (vfxDust != null)
        {
            bool shouldPlay = IsMoving();
            if (vfxDust.activeSelf != shouldPlay)
                vfxDust.SetActive(shouldPlay);
        }

        if (IsMoving())
        {
            float moveSpeed = stats.speed.Value;
            rb.linearVelocity = moveInput.normalized * moveSpeed;
        }
        else if (targetEnemy == null && targetDestructible == null)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool(MoveBool, false);
        }
    }
    
    public void PlayWalkSFX()
    {
        if (!stats.isDead && !IsDashing)
            AudioManager.Instance.PlaySFX("Walk");
    }
    
    protected virtual void MovePlayer()
    {
        float moveSpeed = stats.speed.Value;
        rb.linearVelocity = moveInput.normalized * moveSpeed;
        RotateCharacter(moveInput.x);
    }

    protected virtual void MoveToTarget(Transform target, System.Action onFacing = null)
    {
        if (target == null) return;

        Vector2 playerPos = transform.position;
        Vector2 targetPos = target.position;

        float yDiff = Mathf.Abs(playerPos.y - targetPos.y);
        float xDiff = Mathf.Abs(playerPos.x - targetPos.x);
        float moveSpeed = stats.speed.Value;
        float yTolerance = 0.1f;

        if (yDiff > yTolerance)
        {
            Vector2 dirY = new Vector2(0, targetPos.y - playerPos.y).normalized;
            rb.linearVelocity = dirY * moveSpeed;
            anim.SetBool(MoveBool, true);
        }
        else if (xDiff > attackRange * 0.8f)
        {
            Vector2 dirX = new Vector2(targetPos.x - playerPos.x, 0).normalized;
            rb.linearVelocity = dirX * moveSpeed;
            RotateCharacter(dirX.x);
            anim.SetBool(MoveBool, true);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool(MoveBool, false);
            if (!IsDashing)
                RotateCharacter(targetPos.x - playerPos.x);
            onFacing?.Invoke();
            
            if (Time.time - lastAttackTime >= 1f / stats.attackSpeed.Value)
            {
                if (stats.isUsingSkill) return;
                if (targetEnemy != null)
                {
                    if (targetEnemy.TryGetComponent(out EnemyAI enemy))
                    {
                        if (enemy != null && !enemy.IsDead)
                        {
                            lastAttackTime = Time.time;
                            anim.SetTrigger(AttackTrigger);
                        }
                    }
                }
                else
                {
                    lastAttackTime = Time.time;
                    anim.SetTrigger(AttackTrigger); 
                }
            }


        }
    }
    
    // Gọi trong Animation Event, KHÔNG gọi trong Update
    public void ApplyAttackDamage()
    {
        AudioManager.Instance.PlaySFX("Attack");

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                int damage = (int)stats.attack.Value;
                
                bool isCrit = Random.Range(0f, 100f) < stats.GetCritChance();

                if (isCrit)
                    damage = Mathf.RoundToInt(damage * 1.5f);

                if (hit == null) return;

                if (hit.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(damage , isCrit);
                }

                int healedAmount = stats.HealFromLifeSteal(damage);
                if (healedAmount > 0)
                {
                    FloatingTextSpawner.Instance.SpawnText(
                        $"+{healedAmount}",
                        transform.position + Vector3.up,
                        new Color(0.3f, 1f, 0.3f)); // Xanh lá non
                }
            }
            else if (hit.CompareTag("Destructible"))
            {
                hit.GetComponent<DestructibleObject>()?.Hit();
            }
        }
    }

    
    protected virtual void AttackEnemy()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger(AttackTrigger);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                int damage = (int)stats.attack.Value;
                bool isCrit = Random.Range(0f, 100f) < stats.GetCritChance();

                if (isCrit)
                    damage = Mathf.RoundToInt(damage * 1.5f);

                hit.GetComponent<EnemyAI>()?.TakeDamage(damage, isCrit);
                stats.HealFromLifeSteal(damage);
            }
            else if (hit.CompareTag("Destructible"))
            {
                hit.GetComponent<DestructibleObject>()?.Hit();
            }
        }
    }

    protected void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float minDist = detectionRange;
        targetEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                targetEnemy = enemy.transform;
            }
        }
    }

    protected void FindClosestDestructible()
    {
        GameObject[] destructibles = GameObject.FindGameObjectsWithTag("Destructible");
        float minDist = detectionRange;
        targetDestructible = null;

        foreach (GameObject obj in destructibles)
        {
            float dist = Vector2.Distance(transform.position, obj.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                targetDestructible = obj.transform;
            }
        }
    }

    public Vector2 GetMoveInput()
    {
        float joyX = UltimateJoystick.GetHorizontalAxis("Move");
        float joyY = UltimateJoystick.GetVerticalAxis("Move");

        float keyX = Input.GetKey(KeyCode.A) ? -1 : Input.GetKey(KeyCode.D) ? 1 : 0;
        float keyY = Input.GetKey(KeyCode.S) ? -1 : Input.GetKey(KeyCode.W) ? 1 : 0;

        Vector2 combined = new Vector2(joyX + keyX, joyY + keyY);
        return combined.magnitude > 1 ? combined.normalized : combined;
    }

    public void RotateCharacter(float direction)
    {
        if (IsDashing) return;
        if (direction < 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction > 0)
            transform.localScale = new Vector3(-1, 1, 1); 
    }

    protected void FaceEnemy()
    {
        if (stats.isDead || IsDashing) return;
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

    public void OnEventRaised()
    {
        anim = GetComponentInChildren<Animator>();
    }

    private void OnEnable() => GameEvents.OnUpdateAnimation.RegisterListener(this);
    private void OnDisable() => GameEvents.OnUpdateAnimation.UnregisterListener(this);
    
}
