using UnityEngine;

public enum AllyState
{
    Idle,
    Follow,
    Chase,
    Attack,
    Dead
}

public abstract class AllyBaseAI : MonoBehaviour
{
    protected static readonly int MoveBool = Animator.StringToHash("1_Move");

    protected float followDistance = 1.5f;
    [SerializeField] protected float detectionRange = 5f;
    [SerializeField] protected float attackRange = 1.5f;
    [SerializeField] protected float attackCooldown = 0.5f;

    protected Transform player;
    protected PlayerController playerController;
    protected Transform target;
    protected Rigidbody2D rb;
    protected Animator anim;
    protected AllyStats stats;

    protected float lastAttackTime;
    protected float lastStateCheckTime;
    protected float stateCheckDelay = 0.2f;

    protected AllyState currentState = AllyState.Idle;
    protected Hero hero;
    public bool IsDead { get; set; }

    private readonly float[] skillTimers = new float[2];

    public virtual void Setup(Hero hero)
    {
        this.hero = hero;
        name = hero.data.name;
        stats = GetComponent<AllyStats>();
        stats.Initialize(hero.currentStats, hero.data.name);

        if (hero.data.passiveSkill != null)
        {
            ISkillAlly passive = AllySkillFactory.GetSkillImplementation(hero.data.passiveSkill);
            passive?.Execute(this, hero.data.passiveSkill);
        }
    }

    protected virtual void Start()
    {
        stats = GetComponent<AllyStats>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        RefreshPlayerReference();
    }

    protected virtual void Update()
    {
        RefreshPlayerReference();

        if (player == null || playerController == null || stats == null || stats.IsDead)
        {
            currentState = AllyState.Dead;
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
            anim?.SetBool(MoveBool, false);
            return;
        }

        if (Time.time - lastStateCheckTime >= stateCheckDelay)
        {
            EvaluateState();
            lastStateCheckTime = Time.time;
        }

        HandleState();
        HandleSkills();
    }

    protected virtual void HandleSkills()
    {
        TryCastSkill(hero?.data.activeSkill1, 0);
        TryCastSkill(hero?.data.activeSkill2, 1);
    }

    public Transform GetTarget()
    {
        return target;
    }

    private void TryCastSkill(AllySkillData data, int index)
    {
        if (data == null || target == null)
            return;

        if (Time.time < skillTimers[index] + data.cooldown)
            return;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        if (distanceToTarget > attackRange)
            return;

        ISkillAlly skill = AllySkillFactory.GetSkillImplementation(data);
        if (skill != null && skill.CanExecute(this, data))
        {
            skill.Execute(this, data);
            skillTimers[index] = Time.time;
        }
    }

    protected virtual void EvaluateState()
    {
        if (stats == null || stats.IsDead)
        {
            currentState = AllyState.Dead;
            return;
        }

        FindTarget();

        if (target != null)
        {
            float sqrAttackRange = attackRange * attackRange;
            float targetSqrDist = ((Vector2)target.position - (Vector2)transform.position).sqrMagnitude;
            currentState = targetSqrDist <= sqrAttackRange ? AllyState.Attack : AllyState.Chase;
            return;
        }

        if (ShouldFollowPlayer())
        {
            currentState = AllyState.Follow;
            return;
        }

        currentState = AllyState.Idle;
    }

    protected virtual void HandleState()
    {
        switch (currentState)
        {
            case AllyState.Idle:
                rb.linearVelocity = Vector2.zero;
                anim?.SetBool(MoveBool, false);
                break;

            case AllyState.Follow:
                FollowPlayer();
                break;

            case AllyState.Chase:
                MoveToAttackPosition();
                break;

            case AllyState.Attack:
                if (!IsValidCombatTarget(target))
                {
                    target = null;
                    currentState = AllyState.Follow;
                    rb.linearVelocity = Vector2.zero;
                    anim?.SetBool(MoveBool, false);
                    break;
                }

                rb.linearVelocity = Vector2.zero;
                anim?.SetBool(MoveBool, false);
                RotateCharacter(target.position.x - transform.position.x);

                float cooldown = Mathf.Max(attackCooldown, 1f / Mathf.Max(0.01f, stats.AttackSpeed));
                if (Time.time - lastAttackTime >= cooldown)
                    AttackTarget();
                break;

            case AllyState.Dead:
                rb.linearVelocity = Vector2.zero;
                anim?.SetBool(MoveBool, false);
                break;
        }
    }

    protected virtual bool IsPlayerUnderThreat()
    {
        Transform playerTarget = playerController.GetTargetEnemy();
        if (playerTarget == null)
            return false;

        return IsValidCombatTarget(playerTarget);
    }

    protected virtual void FollowPlayer()
    {
        Vector2 targetPos = GetFormationAnchor();
        Vector2 toTarget = targetPos - (Vector2)transform.position;
        float sqrDistance = toTarget.sqrMagnitude;
        float stopDistance = 0.15f;

        if (sqrDistance <= stopDistance * stopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            anim?.SetBool(MoveBool, false);
            return;
        }

        Vector2 direction = toTarget.normalized;
        rb.linearVelocity = direction * stats.MoveSpeed;
        RotateCharacter(direction.x);
        anim?.SetBool(MoveBool, true);
    }

    protected virtual void MoveToAttackPosition()
    {
        if (!IsValidCombatTarget(target))
        {
            target = null;
            rb.linearVelocity = Vector2.zero;
            anim?.SetBool(MoveBool, false);
            return;
        }

        Vector2 myPos = transform.position;
        Vector2 targetPos = target.position;
        float distance = Vector2.Distance(myPos, targetPos);
        float closeCombatAttackBuffer = 0.2f;

        // Cho phép hero cận chiến đứng lệch chéo nhẹ nhưng vẫn được đánh.
        if (distance <= attackRange + closeCombatAttackBuffer)
        {
            rb.linearVelocity = Vector2.zero;
            anim?.SetBool(MoveBool, false);
            RotateCharacter(targetPos.x - myPos.x);

            float cooldown = Mathf.Max(attackCooldown, 1f / Mathf.Max(0.01f, stats.AttackSpeed));
            if (Time.time - lastAttackTime >= cooldown)
                AttackTarget();
            return;
        }

        float yDiff = Mathf.Abs(myPos.y - targetPos.y);
        float xDiff = Mathf.Abs(myPos.x - targetPos.x);

        if (yDiff > 0.1f)
        {
            Vector2 dirY = new Vector2(0f, targetPos.y - myPos.y).normalized;
            rb.linearVelocity = dirY * stats.MoveSpeed;
            anim?.SetBool(MoveBool, true);
        }
        else if (xDiff > attackRange * 0.8f)
        {
            Vector2 dirX = new Vector2(targetPos.x - myPos.x, 0f).normalized;
            rb.linearVelocity = dirX * stats.MoveSpeed;
            RotateCharacter(dirX.x);
            anim?.SetBool(MoveBool, true);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            anim?.SetBool(MoveBool, false);
        }
    }

    protected abstract void AttackTarget();

    protected virtual void RotateCharacter(float direction)
    {
        if (Mathf.Approximately(direction, 0f))
            return;

        transform.rotation = Quaternion.Euler(0f, direction < 0f ? 180f : 0f, 0f);
    }

    protected virtual void FindTarget()
    {
        Transform playerTarget = playerController.GetTargetEnemy();
        if (IsPreferredPlayerTarget(playerTarget))
        {
            target = playerTarget;
            return;
        }

        target = FindEnemyThreatNearPlayer();
        if (target != null)
            return;

        target = FindNearestEnemy();
        if (target != null)
            return;

        target = FindNearestDestructible();
    }

    protected Transform FindNearestEnemy()
    {
        if (EnemyTracker.Instance == null)
            return null;

        EnemyAI nearest = EnemyTracker.Instance.GetClosestEnemy(transform.position, detectionRange);
        return nearest != null ? nearest.transform : null;
    }

    protected Transform FindNearestDestructible()
    {
        if (DestructibleTracker.Instance == null)
            return null;

        float minSqrDist = detectionRange * detectionRange;
        Transform nearest = null;

        foreach (var obj in DestructibleTracker.Instance.GetInRange(transform.position, detectionRange))
        {
            if (obj == null)
                continue;

            float sqrDist = ((Vector2)obj.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDist < minSqrDist)
            {
                minSqrDist = sqrDist;
                nearest = obj.transform;
            }
        }

        return nearest;
    }

    public AllyStats GetStats()
    {
        return stats;
    }

    public AllyStats FindNearestAlly()
    {
        AllyStats self = GetStats();
        float minDistance = Mathf.Infinity;
        AllyStats nearest = null;

        if (AllyManager.Instance == null)
            return null;

        foreach (AllyBaseAI ally in AllyManager.Instance.GetAllies())
        {
            if (ally == null || ally == this || ally.GetStats().IsDead)
                continue;

            float dist = Vector2.Distance(transform.position, ally.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = ally.GetStats();
            }
        }

        return nearest;
    }

    public PlayerStats FindPlayerStats()
    {
        return player != null ? player.GetComponent<PlayerStats>() : null;
    }

    protected virtual void OnEnable()
    {
        AllyManager.Instance?.RegisterAlly(this);
    }

    protected virtual void OnDisable()
    {
        AllyManager.Instance?.UnregisterAlly(this);
    }

    private void RefreshPlayerReference()
    {
        if (PlayerController.Instance == null)
            return;

        playerController = PlayerController.Instance;
        player = playerController.transform;
    }

    private bool ShouldFollowPlayer()
    {
        if (player == null || playerController == null)
            return false;

        float desiredDistance = followDistance + GetFormationDistanceBias();
        float sqrDistanceToFormation = (GetFormationAnchor() - (Vector2)transform.position).sqrMagnitude;
        bool tooFarFromFormation = sqrDistanceToFormation > desiredDistance * desiredDistance;

        return tooFarFromFormation || playerController.IsMoving() || playerController.IsAttacking || IsPlayerUnderThreat();
    }

    private Vector2 GetFormationAnchor()
    {
        Vector2 baseOffset = AllyManager.Instance != null ? AllyManager.Instance.GetOffsetPosition(this) : Vector2.zero;
        Vector2 movementLead = playerController != null && playerController.MoveInput.sqrMagnitude > 0.01f
            ? playerController.MoveInput.normalized * 0.5f
            : Vector2.zero;

        return (Vector2)player.position + baseOffset + movementLead;
    }

    private float GetFormationDistanceBias()
    {
        Vector2 offset = AllyManager.Instance != null ? AllyManager.Instance.GetOffsetPosition(this) : Vector2.zero;
        return offset.magnitude * 0.25f;
    }

    private Transform FindEnemyThreatNearPlayer()
    {
        if (EnemyTracker.Instance == null || player == null)
            return null;

        float searchRange = Mathf.Max(detectionRange, Vector2.Distance(transform.position, player.position) + attackRange);
        float sqrBestScore = float.MaxValue;
        Transform best = null;

        foreach (EnemyAI enemy in EnemyTracker.Instance.GetEnemiesInRange(player.position, searchRange))
        {
            if (enemy == null || enemy.IsDead)
                continue;

            float playerSqrDist = ((Vector2)enemy.transform.position - (Vector2)player.position).sqrMagnitude;
            float allySqrDist = ((Vector2)enemy.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (allySqrDist > detectionRange * detectionRange)
                continue;

            float score = playerSqrDist * 0.7f + allySqrDist * 0.3f;
            if (score < sqrBestScore)
            {
                sqrBestScore = score;
                best = enemy.transform;
            }
        }

        return best;
    }

    private bool IsPreferredPlayerTarget(Transform playerTarget)
    {
        if (!IsValidCombatTarget(playerTarget))
            return false;

        float playerThreatSqrDist = ((Vector2)playerTarget.position - (Vector2)player.position).sqrMagnitude;
        return playerThreatSqrDist <= detectionRange * detectionRange;
    }

    private bool IsValidCombatTarget(Transform candidate)
    {
        if (candidate == null || !candidate.gameObject.activeInHierarchy)
            return false;

        float sqrDist = ((Vector2)candidate.position - (Vector2)transform.position).sqrMagnitude;
        if (sqrDist > detectionRange * detectionRange)
            return false;

        EnemyAI enemy = candidate.GetComponent<EnemyAI>();
        if (enemy != null)
            return !enemy.IsDead;

        DestructibleObject destructible = candidate.GetComponent<DestructibleObject>();
        return destructible != null;
    }
}
