using System.Collections;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>, IGameEventListener, IGameEventListener<WeaponCategory>
{
    internal static readonly int AttackTrigger = Animator.StringToHash("2_Attack");
    internal static readonly int AttackBow = Animator.StringToHash("7_Shoot");
    internal static readonly int MoveBool = Animator.StringToHash("1_Move");
    internal static readonly int AttackRange = Animator.StringToHash("AttackRange");

    [SerializeField] protected float baseDetectionRange = 1f;
    [SerializeField] private float meleeRange = 1f;
    [SerializeField] private float rangedRange = 5f;
    [SerializeField] private float bowRange = 4f;

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

    private EnemyAI currentSelectedEnemy;
    private DestructibleObject currentSelectedDestructible;
    private PlayerMovement movementLogic;
    private PlayerCombat combatLogic;
    private float lastStepSoundTime;
    private const float StepCooldown = 0.4f;

    public WeaponCategory typeWeapon;

    public Vector2 MoveInput => moveInput;
    public Transform GetTargetEnemy() => targetEnemy;
    public bool IsPlayerDie() => stats != null && stats.isDead;
    public bool IsMoving() => moveInput.magnitude > 0.01f;
    public bool IsDashing => GetComponent<PlayerDash>()?.IsDashing == true;
    public bool IsAttacking
    {
        get
        {
            if (anim == null)
                return false;

            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
            return state.IsTag("Attack") && state.normalizedTime < 1f;
        }
    }

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        stats = GetComponent<PlayerStats>();
        EnsureLogicInitialized();
    }

    protected virtual void Update()
    {
        if (stats == null || stats.isDead)
            return;

        moveInput = GetMoveInput();

        if (IsAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool(MoveBool, false);
            return;
        }

        bool isMoving = IsMoving();
        anim.SetBool(MoveBool, isMoving);

        if (isMoving && !IsDashing && Time.time - lastStepSoundTime >= StepCooldown)
        {
            if (moveInput.magnitude > 0.9f)
                PlayRunSFX();
            else
                PlayWalkSFX();

            lastStepSoundTime = Time.time;
        }

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

        FindClosestEnemy();

        if (targetEnemy != null)
        {
            MoveToTarget(targetEnemy, FaceEnemy);
            return;
        }

        FindClosestDestructible();

        if (targetDestructible != null)
        {
            MoveToTarget(targetDestructible);
            return;
        }

        rb.linearVelocity = Vector2.zero;
        anim.SetBool(MoveBool, false);
    }

    public void PlayRunSFX()
    {
        /*if (!stats.isDead && !IsDashing)
            AudioManager.Instance.PlaySFX("Run");*/
    }

    protected virtual void FixedUpdate()
    {
        if (stats == null || stats.isDead || IsDashing)
            return;

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

    public void PlayWalkSFX()
    {
        if (stats != null && !stats.isDead && !IsDashing)
            AudioManager.Instance?.PlaySFX("Walk");
    }

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

    protected void FaceEnemy()
    {
        if (stats == null || stats.isDead || IsDashing || targetEnemy == null)
            return;

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

        rb.linearVelocity = Vector2.zero;
    }

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
            case WeaponCategory.Ranged:
                range += 3f;
                break;
            case WeaponCategory.HeavyMelee:
                range += 1.5f;
                break;
            case WeaponCategory.Bow:
                range += 4f;
                break;
        }

        return range;
    }

    internal float GetCurrentAttackRange()
    {
        if (typeWeapon == WeaponCategory.Bow)
            return bowRange;

        if (typeWeapon == WeaponCategory.Ranged)
            return rangedRange;

        return meleeRange;
    }

    internal float GetWeaponAttackSpeedMultiplier()
    {
        return typeWeapon == WeaponCategory.HeavyMelee ? 0.5f : 1f;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, GetDetectionRange());

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, GetCurrentAttackRange());

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

    public void FireArrow()
    {
        EnsureLogicInitialized();
        combatLogic.FireArrow();
    }

    internal void RotateToTarget(Transform target)
    {
        EnsureLogicInitialized();
        movementLogic.RotateToTarget(target);
    }

    protected void FindClosestDestructible()
    {
        EnsureLogicInitialized();
        combatLogic.FindClosestDestructible();
    }

    private void EnsureLogicInitialized()
    {
        rb ??= GetComponent<Rigidbody2D>();
        anim ??= GetComponentInChildren<Animator>();
        stats ??= GetComponent<PlayerStats>();
        movementLogic ??= new PlayerMovement(this);
        combatLogic ??= new PlayerCombat(this);
    }

    internal Rigidbody2D Body => rb;
    internal Animator Animator => anim;
    internal PlayerStats Stats => stats;
    internal Transform AttackPoint => attackPoint;
    internal float AttackRadius => attackRadius;
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
