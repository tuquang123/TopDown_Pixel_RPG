using System.Collections;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>, IGameEventListener, IGameEventListener<WeaponCategory>
{
    internal static readonly int AttackTrigger = Animator.StringToHash("2_Attack");
    internal static readonly int AttackBow     = Animator.StringToHash("7_Shoot");
    internal static readonly int MoveBool      = Animator.StringToHash("1_Move");
    internal static readonly int AttackRange   = Animator.StringToHash("AttackRange");

    [SerializeField] protected float baseDetectionRange = 1f;
    [SerializeField] private   float meleeRange         = 1f;
    [SerializeField] private   float rangedRange        = 5f;
    [SerializeField] private   float bowRange           = 4f;

    [SerializeField] protected Transform  attackPoint;
    [SerializeField] protected float      attackRadius = 0.8f;
    [SerializeField] protected GameObject vfxDust;

    [Header("Combat")]
    [SerializeField] private bool  allowAutoAttack   = true;
    [SerializeField] private bool  enableManualInput = false;

    /// <summary>
    /// Sau khi bị đánh, player "bám" target và ưu tiên phản đòn trong khoảng thời gian này (giây).
    /// </summary>
    [SerializeField] private float combatStanceDuration = 1.5f;

    protected Rigidbody2D  rb;
    protected Animator     anim;
    protected Transform    targetEnemy;
    protected Transform    targetDestructible;
    protected PlayerStats  stats;
    protected Vector2      moveInput;
    protected float        lastAttackTime;

    private EnemyAI            currentSelectedEnemy;
    private DestructibleObject currentSelectedDestructible;
    private PlayerMovement     movementLogic;
    private PlayerCombat       combatLogic;
    private float              lastStepSoundTime;
    private const float        StepCooldown = 0.4f;

    /// <summary>
    /// Thời điểm player nhận đòn cuối cùng — kích hoạt combat stance.
    /// Gọi NotifyHit() từ PlayerStats mỗi khi player bị tấn công.
    /// </summary>
    private float lastHitTime = -999f;

    public WeaponCategory typeWeapon;

    // ─────────────────────────── Public accessors ────────────────────────────

    public Vector2   MoveInput         => moveInput;
    public Transform GetTargetEnemy()   => targetEnemy;
    public bool      IsPlayerDie()      => stats != null && stats.isDead;
    public bool      IsMoving()         => moveInput.magnitude > 0.01f;
    public bool      IsDashing          => GetComponent<PlayerDash>()?.IsDashing == true;

    /// <summary>True khi player đang trong animation Attack chưa kết thúc.</summary>
    public bool IsAttacking
    {
        get
        {
            if (anim == null) return false;
            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
            return state.IsTag("Attack") && state.normalizedTime < 1f;
        }
    }

    /// <summary>
    /// True khi player vừa bị đánh và còn trong cửa sổ combat stance.
    /// Lúc này player sẽ ưu tiên phản đòn và mở rộng detection range.
    /// </summary>
    public bool IsInCombatStance => Time.time - lastHitTime < combatStanceDuration;

    // ─────────────────────────── Unity lifecycle ─────────────────────────────

    protected virtual void Start()
    {
        rb    = GetComponent<Rigidbody2D>();
        anim  = GetComponentInChildren<Animator>();
        stats = GetComponent<PlayerStats>();
        EnsureLogicInitialized();
    }

    protected virtual void Update()
    {
        if (stats == null || stats.isDead) return;

        moveInput = enableManualInput ? GetMoveInput() : Vector2.zero;

        // Đang tấn công: khoá di chuyển nhưng không return sớm —
        // để frame tiếp theo vẫn gọi TryAttack() ngay khi animation xong.
        if (IsAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool(MoveBool, false);
            return;
        }

        bool isMoving = IsMoving();
        anim.SetBool(MoveBool, isMoving);

        // ── Step sounds ──────────────────────────────────────────────────────
        if (isMoving && !IsDashing && Time.time - lastStepSoundTime >= StepCooldown)
        {
            if (moveInput.magnitude > 0.9f) PlayRunSFX();
            else                             PlayWalkSFX();
            lastStepSoundTime = Time.time;
        }

        // ── Manual movement: bỏ target, di chuyển ───────────────────────────
        if (isMoving)
        {
            MovePlayer();
            combatLogic.ClearTargetsAndSelection();
            return;
        }

        if (!allowAutoAttack)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool(MoveBool, false);
            return;
        }

        // ── Auto-combat ──────────────────────────────────────────────────────
        FindClosestEnemy();

        if (targetEnemy != null)
        {
            UpdateAttackPoint();
            FaceEnemy();

            if (IsTargetInAttackRange(targetEnemy))
            {
                // Đứng yên và phản đòn ngay
                rb.linearVelocity = Vector2.zero;
                anim.SetBool(MoveBool, false);
                combatLogic.TryAttack();
            }
            else
            {
                // Tiến lại gần target
                MoveToTarget(targetEnemy, FaceEnemy);
            }
            return;
        }

        FindClosestDestructible();

        if (targetDestructible != null)
        {
            UpdateAttackPoint();

            if (IsTargetInAttackRange(targetDestructible))
            {
                rb.linearVelocity = Vector2.zero;
                anim.SetBool(MoveBool, false);
                combatLogic.TryAttack();
            }
            else
            {
                MoveToTarget(targetDestructible);
            }
            return;
        }

        rb.linearVelocity = Vector2.zero;
        anim.SetBool(MoveBool, false);
    }

    protected virtual void FixedUpdate()
    {
        if (stats == null || stats.isDead || IsDashing) return;

        if (vfxDust != null)
        {
            bool shouldPlay = IsMoving();
            if (vfxDust.activeSelf != shouldPlay)
                vfxDust.SetActive(shouldPlay);
        }

        if (IsMoving())
        {
            movementLogic.ApplyMoveVelocity();
        }
        else if (targetEnemy == null && targetDestructible == null)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool(MoveBool, false);
        }
    }

    // ─────────────────────────── Combat Stance ───────────────────────────────

    /// <summary>
    /// Gọi hàm này từ PlayerStats (hoặc nơi xử lý nhận damage) mỗi khi player bị đánh.
    /// Ví dụ: PlayerController.Instance?.NotifyHit();
    /// </summary>
    public void NotifyHit()
    {
        lastHitTime = Time.time;

        // Nếu chưa có target, tìm ngay để phản đòn kịp thời
        if (targetEnemy == null)
            FindClosestEnemy();
    }

    // ─────────────────────────── Attack range helper ─────────────────────────

    /// <summary>
    /// Kiểm tra target có nằm trong attack range không.
    /// Dùng GetCurrentAttackRange() + attackRadius để khớp vùng damage thực tế.
    /// </summary>
    private bool IsTargetInAttackRange(Transform target)
    {
        if (target == null) return false;
        float range   = GetCurrentAttackRange() + attackRadius;
        float sqrDist = ((Vector2)target.position - (Vector2)transform.position).sqrMagnitude;
        return sqrDist <= range * range;
    }

    // ─────────────────────────── SFX ─────────────────────────────────────────

    public void PlayRunSFX() { /* AudioManager.Instance?.PlaySFX("Run"); */ }

    public void PlayWalkSFX()
    {
        if (stats != null && !stats.isDead && !IsDashing)
            AudioManager.Instance?.PlaySFX("Walk");
    }

    // ─────────────────────────── Movement delegation ─────────────────────────

    protected virtual void MovePlayer()
    {
        EnsureLogicInitialized();
        movementLogic.MovePlayer();
    }

    protected virtual void MoveToTarget(Transform target, System.Action onFacing = null)
    {
        EnsureLogicInitialized();
        movementLogic.MoveToTarget(target, onFacing);
    }

    // ─────────────────────────── Combat delegation ───────────────────────────

    internal void TryAttack()
    {
        EnsureLogicInitialized();
        combatLogic.TryAttack();
    }

    public void ThrowShuriken()
    {
        EnsureLogicInitialized();
        combatLogic.ThrowShuriken();
    }

    public void FireArrow()
    {
        EnsureLogicInitialized();
        combatLogic.FireArrow();
    }

    public void ApplyAttackDamage()
    {
        EnsureLogicInitialized();
        combatLogic.ApplyAttackDamage(false);
    }

    public void ApplyAttackDamage(bool isSkill, SkillData skill = null)
    {
        EnsureLogicInitialized();
        combatLogic.ApplyAttackDamage(isSkill, skill);
    }

    protected void FindClosestEnemy()
    {
        EnsureLogicInitialized();
        combatLogic.FindClosestEnemy();
    }

    protected void FindClosestDestructible()
    {
        EnsureLogicInitialized();
        combatLogic.FindClosestDestructible();
    }

    // ─────────────────────────── Rotation / facing ───────────────────────────

    public Vector2 GetMoveInput()
    {
        EnsureLogicInitialized();
        return movementLogic.GetMoveInput();
    }

    public void RotateCharacter(float direction)
    {
        EnsureLogicInitialized();
        movementLogic.RotateCharacter(direction);
    }

    internal void RotateToTarget(Transform target)
    {
        EnsureLogicInitialized();
        movementLogic.RotateToTarget(target);
    }

    protected void FaceEnemy()
    {
        if (stats == null || stats.isDead || IsDashing || targetEnemy == null) return;
        RotateCharacter(targetEnemy.position.x - transform.position.x);
        UpdateAttackPoint();
    }

    /// <summary>
    /// Xoay AttackPoint về phía target mỗi frame.
    /// AttackPoint là Transform con cố định — không tự xoay, nên phải update thủ công.
    /// </summary>
    private void UpdateAttackPoint()
    {
        if (attackPoint == null) return;

        Transform t = targetEnemy ?? targetDestructible;
        if (t == null) return;

        float radius = attackPoint.localPosition.magnitude;
        if (radius < 0.01f) radius = attackRadius;

        Vector2 dir = ((Vector2)t.position - (Vector2)transform.position).normalized;
        attackPoint.localPosition = dir * radius;
    }

    // ─────────────────────────── Knockback ───────────────────────────────────

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

        rb.linearVelocity = Vector2.zero;
    }

    // ─────────────────────────── Stat helpers ────────────────────────────────

    internal float GetHeavyWeaponMoveSpeed()
    {
        float speed = stats.speed.Value;
        if (typeWeapon == WeaponCategory.HeavyMelee)
            speed *= 0.5f;
        return speed;
    }

    internal float GetDetectionRange()
    {
        float range = baseDetectionRange;

        switch (typeWeapon)
        {
            case WeaponCategory.Ranged:     range += 3f;   break;
            case WeaponCategory.HeavyMelee: range += 1.5f; break;
            case WeaponCategory.Bow:        range += 4f;   break;
        }

        // Khi vừa bị đánh, mở rộng detection để tìm kẻ tấn công kể cả khi bị đẩy lùi
        if (IsInCombatStance)
            range += 1f;

        return range;
    }

    internal float GetCurrentAttackRange()
    {
        if (typeWeapon == WeaponCategory.Bow)    return bowRange;
        if (typeWeapon == WeaponCategory.Ranged) return rangedRange;
        return meleeRange;
    }

    internal float GetWeaponAttackSpeedMultiplier()
    {
        return typeWeapon == WeaponCategory.HeavyMelee ? 0.5f : 1f;
    }

    // ─────────────────────────── Gizmos ──────────────────────────────────────

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, GetDetectionRange());

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, GetCurrentAttackRange() + attackRadius);

        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        // Hiển thị combat stance indicator trong Scene view
        if (Application.isPlaying && IsInCombatStance)
        {
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, GetCurrentAttackRange() + attackRadius + 0.15f);
        }
    }

    // ─────────────────────────── Events ──────────────────────────────────────

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

    // ─────────────────────────── Lazy init ───────────────────────────────────

    private void EnsureLogicInitialized()
    {
        rb            ??= GetComponent<Rigidbody2D>();
        anim          ??= GetComponentInChildren<Animator>();
        stats         ??= GetComponent<PlayerStats>();
        movementLogic ??= new PlayerMovement(this);
        combatLogic   ??= new PlayerCombat(this);
    }

    // ─────────────────────────── Internal accessors ──────────────────────────

    internal Rigidbody2D Body        => rb;
    internal Animator    Animator    => anim;
    internal PlayerStats Stats       => stats;
    internal Transform   AttackPoint => attackPoint;
    internal float       AttackRadius => attackRadius;

    internal EnemyAI CurrentSelectedEnemy
    {
        get => currentSelectedEnemy;
        set => currentSelectedEnemy = value;
    }
    internal DestructibleObject CurrentSelectedDestructible
    {
        get => currentSelectedDestructible;
        set => currentSelectedDestructible = value;
    }
    internal Transform TargetEnemy
    {
        get => targetEnemy;
        set => targetEnemy = value;
    }
    internal Transform TargetDestructible
    {
        get => targetDestructible;
        set => targetDestructible = value;
    }
    internal float LastAttackTime
    {
        get => lastAttackTime;
        set => lastAttackTime = value;
    }
}