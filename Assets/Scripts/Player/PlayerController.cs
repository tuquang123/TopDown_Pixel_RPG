using System.Collections;
using System.Linq;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>, IGameEventListener , IGameEventListener<WeaponCategory> 
{
    protected static readonly int AttackTrigger = Animator.StringToHash("2_Attack");
    protected static readonly int MoveBool = Animator.StringToHash("1_Move");
    private static readonly int AttackRange = Animator.StringToHash("AttackRange");

    [SerializeField] protected float baseDetectionRange = 1f;
   
    [SerializeField] private float meleeRange = 1f;
    [SerializeField] private float rangedRange = 5f;
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected float attackRadius = 0.8f;
    [SerializeField] protected GameObject vfxDust;
    
    [Header("Combat")]
    [SerializeField] private bool allowAutoAttack = true;
    
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
    public bool IsAttacking
    {
        get
        {
            var state = anim.GetCurrentAnimatorStateInfo(0);
            return state.IsTag("Attack") && state.normalizedTime < 1f;
        }
    }
    
    float CurrentAttackRange => typeWeapon == WeaponCategory.Ranged ? rangedRange : meleeRange;
    
    public WeaponCategory typeWeapon;
    
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        stats = GetComponent<PlayerStats>();
    }
    private float lastStepSoundTime;
    private float stepCooldown = 0.4f;


    protected virtual void Update()
    {
        if (stats.isDead) return;
       
        moveInput = GetMoveInput();
        
        bool isAttacking = IsAttacking;

        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool(MoveBool, false);
            return;
        }

        bool isMoving = IsMoving();
        anim.SetBool(MoveBool, isMoving);

        // --- Phát âm thanh đi/chạy ---
        if (isMoving && !IsDashing)
        {
            if (Time.time - lastStepSoundTime >= stepCooldown)
            {
                if (moveInput.magnitude > 0.9f)
                    PlayRunSFX();
                else
                    PlayWalkSFX();

                lastStepSoundTime = Time.time;
            }
        }

        // --- Di chuyển và xử lý mục tiêu ---
        if (isMoving)
        {
            MovePlayer();
            targetEnemy = null;
            targetDestructible = null;
        }
        else
        {
            if (!allowAutoAttack)
            {
                rb.linearVelocity = Vector2.zero;
                anim.SetBool(MoveBool, false);
                return;
            }
            
            FindClosestEnemy();

            if (targetEnemy != null)
            {
                MoveToTarget(targetEnemy, FaceEnemy);
            }
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
    

    public void PlayRunSFX()
    {
        /*if (!stats.isDead && !IsDashing)
            AudioManager.Instance.PlaySFX("Run");*/
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
            float moveSpeed = GetHeavyWeaponMoveSpeed();
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
        float moveSpeed = GetHeavyWeaponMoveSpeed();
        rb.linearVelocity = moveInput.normalized * moveSpeed;
        RotateCharacter(moveInput.x);
    }

    protected virtual void MoveToTarget(Transform target, System.Action onFacing = null)
    {
        if (target == null || stats.isDead) return;

        Vector2 playerPos = transform.position;
        Vector2 targetPos = target.position;

        float distance = Vector2.Distance(playerPos, targetPos);
        float moveSpeed = GetHeavyWeaponMoveSpeed();

        // ===== RANGED WEAPON =====
        if (typeWeapon == WeaponCategory.Ranged)
        {
            if (distance > CurrentAttackRange)
            {
                Vector2 dir = (targetPos - playerPos).normalized;
                rb.linearVelocity = dir * moveSpeed;
                RotateCharacter(dir.x);
                anim.SetBool(MoveBool, true);
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                anim.SetBool(MoveBool, false);
                RotateCharacter(targetPos.x - playerPos.x);
                onFacing?.Invoke();
                TryAttack();
            }

            return;
        }

        // ===== MELEE WEAPON (giữ logic cũ) =====
        float yDiff = Mathf.Abs(playerPos.y - targetPos.y);
        float xDiff = Mathf.Abs(playerPos.x - targetPos.x);
        float yTolerance = 0.1f;

        if (yDiff > yTolerance)
        {
            Vector2 dirY = new Vector2(0, targetPos.y - playerPos.y).normalized;
            rb.linearVelocity = dirY * moveSpeed;
            anim.SetBool(MoveBool, true);
        }
        else if (xDiff > CurrentAttackRange * 0.8f)
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
            RotateCharacter(targetPos.x - playerPos.x);
            onFacing?.Invoke();
            TryAttack();
        }
    }

    
    private void TryAttack()
    {
        if (targetEnemy != null)
        {
            var enemy = targetEnemy.GetComponent<EnemyAI>();
            if (enemy == null || enemy.IsDead || enemy.CurrentHealth <= 0)
                return;

        }
        else if (targetDestructible != null)
        {
            var des = targetDestructible.GetComponent<DestructibleObject>();
            if (des == null)
                return;
        }
        else
        {
            return;
        }

        
        float finalAttackSpeed =
            stats.GetAttackSpeed() * GetWeaponAttackSpeedMultiplier();

        if (Time.time - lastAttackTime < 1f / finalAttackSpeed)
            return;

        if (stats.isUsingSkill) return;

        lastAttackTime = Time.time;
        
        if (typeWeapon == WeaponCategory.Ranged)
            anim.SetTrigger(AttackRange);
        else
            anim.SetTrigger(AttackTrigger);
    }

    
    public void ThrowShuriken()
    {
        if (CommonReferent.Instance.surikenPrefab == null) return;

        Vector2 dir;

        Transform target = null;

        if (targetEnemy != null)
            target = targetEnemy;
        else if (targetDestructible != null)
            target = targetDestructible;

        if (target != null)
        {
            dir = ((Vector2)target.position - (Vector2)attackPoint.position).normalized;
        }
        else
        {
            // fallback nếu ko có target
            dir = transform.localScale.x < 0 ? Vector2.right : Vector2.left;
        }

        var shuriken = Instantiate(
            CommonReferent.Instance.surikenPrefab,
            attackPoint.position,
            Quaternion.identity
        );

        shuriken.GetComponent<ShurikenProjectile>().Init(
            (int)stats.attack.Value,
            dir,
            stats.GetCritChance(),
            stats.lifeSteal.Value
        );
    }

    
    // Gọi trong Animation Event, KHÔNG gọi trong Update
    public void ApplyAttackDamage()
    {
        if (stats.isDead) return; 
        AudioManager.Instance.PlaySFX("Attack");

        int totalHealed = 0;

        foreach (var enemy in EnemyTracker.Instance.GetAllEnemies().ToList())
        {
            if (enemy == null || enemy.IsDead) continue;

            float dist = Vector2.Distance(attackPoint.position, enemy.transform.position);
            if (dist > attackRadius) continue;

            int damage = (int)stats.attack.Value;
            bool isCrit = Random.Range(0f, 100f) < stats.GetCritChance();

            if (isCrit)
                damage = Mathf.RoundToInt(damage * 1.5f);
            
            enemy.TakeDamage(damage, isCrit);
            
            int healedAmount = stats.HealFromLifeSteal(damage);
            if (healedAmount > 0)
                totalHealed += healedAmount;
        }

        // Gây damage cho các object phá hủy (Destructibles)
        var destructibles = DestructibleTracker.Instance.GetInRange(attackPoint.position, attackRadius);
        foreach (var destructible in destructibles)
        {
            destructible.Hit();
        }

        if (totalHealed > 0)
        {
            FloatingTextSpawner.Instance.SpawnText(
                $"+{totalHealed}",
                transform.position + Vector3.up,
                new Color(0.3f, 1f, 0.3f) // xanh lá non
            );
        }
    }
    public void ApplyAttackDamage(bool isSkill, SkillData skill = null)
    {
        AudioManager.Instance.PlaySFX("Attack");

        int totalHealed = 0;

        int currentLevel = isSkill && skill != null ? stats.GetSkillLevel(skill.skillID) : 0;
        SkillLevelStat stat = isSkill && skill != null ? skill.GetLevelStat(currentLevel) : null;

        foreach (var enemy in EnemyTracker.Instance.GetAllEnemies().ToList())
        {
            if (enemy == null || enemy.IsDead) continue;

            float dist = Vector2.Distance(attackPoint.position, enemy.transform.position);
            if (dist > attackRadius) continue;

            int damage = (int)stats.attack.Value;
            bool isCrit = Random.Range(0f, 100f) < stats.GetCritChance();

            if (isCrit)
                damage = Mathf.RoundToInt(damage * 1.5f);

            enemy.TakeDamage(damage, isCrit);

            int healedAmount = stats.HealFromLifeSteal(damage);
            if (healedAmount > 0)
                totalHealed += healedAmount;
        }

        // ✅ Chỉ đánh vào destructibles nếu là đòn đánh thường
        if (!isSkill)
        {
            var destructibles = DestructibleTracker.Instance.GetInRange(attackPoint.position, attackRadius);
            foreach (var destructible in destructibles)
            {
                destructible.Hit();
            }
        }

        if (totalHealed > 0)
        {
            FloatingTextSpawner.Instance.SpawnText(
                $"+{totalHealed}",
                transform.position + Vector3.up,
                new Color(0.3f, 1f, 0.3f)
            );
        }
    }

    
    protected void FindClosestEnemy()
    {
        targetEnemy = null;
        float minDist = Mathf.Infinity;

        foreach (var enemy in EnemyTracker.Instance.GetEnemiesInRange(transform.position, GetDetectionRange()))
        {
            if (enemy.IsDead) continue;

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
        var list = DestructibleTracker.Instance.GetInRange(transform.position, GetDetectionRange());
        float minDist = GetDetectionRange();
        targetDestructible = null;

        foreach (var obj in list)
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
        if (IsDashing || IsAttacking) return;

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
    public void ApplyKnockback(Vector2 direction, float force, float duration = 0.1f)
    {
        StartCoroutine(KnockbackRoutine(direction, force, duration));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float force, float duration)
    {
        float timer = 0f;
        rb.linearVelocity = direction * force;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero; // ngừng knockback
    }
    
    float GetHeavyWeaponMoveSpeed()
    {
        float speed = stats.speed.Value;

        if (typeWeapon == WeaponCategory.HeavyMelee)
            speed *= 0.5f;

        return speed;
    }
    
    float GetDetectionRange()
    {
        float range = baseDetectionRange;

        switch (typeWeapon)
        {
            case WeaponCategory.Ranged:
                range += 3f;
                break;

            case WeaponCategory.HeavyMelee:
                range += 1.5f;
                break;

            case WeaponCategory.Melee:
            default:
                break;
        }

        return range;
    }

    
    float GetWeaponAttackSpeedMultiplier()
    {
        return typeWeapon == WeaponCategory.HeavyMelee ? 0.5f : 1f;
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, GetDetectionRange());

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, CurrentAttackRange);

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
    
    public void OnEventRaised(WeaponCategory type)
    {
        typeWeapon = type;
    }

    private void OnEnable()
    {
        GameEvents.OnUpdateAnimation.RegisterListener(this);
        GameEvents.OnEquipItemRange.RegisterListener(this);
    }

    private void OnDisable()
    {
        GameEvents.OnUpdateAnimation.UnregisterListener(this);
        GameEvents.OnEquipItemRange.UnregisterListener(this);
    }
    
}
