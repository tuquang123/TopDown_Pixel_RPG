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
        player = CommonReferent.Instance.playerPrefab.transform;
        playerController = player?.GetComponent<PlayerController>();
    }

    protected virtual void Update()
    {
        if (player == null || playerController == null || stats.IsDead)
        {
            currentState = AllyState.Dead;
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
    
    private float[] skillTimers = new float[2];

    protected virtual void HandleSkills()
    {
        TryCastSkill(hero.data.activeSkill1, 0);
        TryCastSkill(hero.data.activeSkill2, 1);
    }
    
    public Transform GetTarget()
    {
        return target;
    }


    private void TryCastSkill(AllySkillData data, int index)
    {
        if (data == null) return;
        if (Time.time < skillTimers[index] + data.cooldown) return;

        float distanceToTarget = target != null ? Vector2.Distance(transform.position, target.position) : Mathf.Infinity;
        if (distanceToTarget > attackRange) return;

        ISkillAlly skill = AllySkillFactory.GetSkillImplementation(data);
        if (skill != null && skill.CanExecute(this, data))
        {
            skill.Execute(this, data);
            skillTimers[index] = Time.time;
        }
    }
    
    protected virtual void EvaluateState()
    {
        if (stats.IsDead)
        {
            currentState = AllyState.Dead;
            return;
        }
        
        if (playerController.IsMoving() && !playerController.IsAttacking && !IsPlayerUnderThreat())
        {
            currentState = AllyState.Follow;
            return;
        }
        
        FindTarget();

        if (target != null)
        {
            float distance = Vector2.Distance(transform.position, target.position);
            if (distance <= attackRange)
            {
                currentState = AllyState.Attack;
            }
            else
            {
                currentState = AllyState.Chase;
            }
        }
        else
        {
            currentState = AllyState.Idle;
        }
    }

    protected virtual void HandleState()
    {
        switch (currentState)
        {
            case AllyState.Idle:
                rb.linearVelocity = Vector2.zero;
                anim.SetBool(MoveBool, false);
                break;

            case AllyState.Follow:
                FollowPlayer();
                break;

            case AllyState.Chase:
                MoveToAttackPosition();
                break;

            case AllyState.Attack:
                rb.linearVelocity = Vector2.zero;
                anim.SetBool(MoveBool, false);
                RotateCharacter(target.position.x - transform.position.x);
                if (Time.time - lastAttackTime >= Mathf.Max(attackCooldown, 1f / stats.AttackSpeed))
                {
                    AttackTarget();
                }
                break;

            case AllyState.Dead:
                rb.linearVelocity = Vector2.zero;
                anim.SetBool(MoveBool, false);
                break;
        }
    }
    
    protected virtual bool IsPlayerUnderThreat()
    {
        return playerController.GetTargetEnemy() != null;
    }


    protected virtual void FollowPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool isPlayerMoving = playerController.MoveInput.magnitude > 0.1f;

        bool shouldMove = distanceToPlayer > followDistance || isPlayerMoving;
        if (!shouldMove)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool(MoveBool, false);
            return;
        }
        
        Vector2 offset = AllyManager.Instance.GetOffsetPosition(this);
        Vector2 targetPos = (Vector2)player.position + offset;
        Vector2 toTarget = targetPos - (Vector2)transform.position;
        float distance = toTarget.magnitude;

        if (distance > 0.15f) 
        {
            Vector2 direction = toTarget.normalized;
            rb.linearVelocity = direction * stats.MoveSpeed;
            RotateCharacter(direction.x);
            anim.SetBool(MoveBool, true);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool(MoveBool, false);
        }

    }
    
    protected virtual void MoveToAttackPosition()
    {
        Vector2 myPos = transform.position;
        Vector2 targetPos = target.position;

        float yDiff = Mathf.Abs(myPos.y - targetPos.y);
        float xDiff = Mathf.Abs(myPos.x - targetPos.x);
        float yTolerance = 0.1f;

        if (yDiff > yTolerance)
        {
            Vector2 dirY = new Vector2(0, targetPos.y - myPos.y).normalized;
            rb.linearVelocity = dirY * stats.MoveSpeed;
            anim.SetBool(MoveBool, true);
        }
        else if (xDiff > attackRange * 0.8f)
        {
            Vector2 dirX = new Vector2(targetPos.x - myPos.x, 0).normalized;
            rb.linearVelocity = dirX * stats.MoveSpeed;
            RotateCharacter(dirX.x);
            anim.SetBool(MoveBool, true);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool(MoveBool, false);
        }
    }

    protected abstract void AttackTarget();

    protected virtual void RotateCharacter(float direction)
    {
        transform.rotation = Quaternion.Euler(0, direction < 0 ? 180 : 0, 0);
    }

    protected virtual void FindTarget()
    {
        if (playerController.GetTargetEnemy() != null)
        {
            target = playerController.GetTargetEnemy();
            return;
        }

        // Ưu tiên enemy sống
        target = FindNearestEnemy();

        if (target == null)
        {
            target = FindNearestDestructible();
        }
    }
    protected Transform FindNearestEnemy()
    {
        float minDist = detectionRange;
        Transform nearest = null;

        foreach (var enemy in EnemyTracker.Instance.GetEnemiesInRange(transform.position, detectionRange))
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }

    protected Transform FindNearestDestructible()
    {
        float minDist = detectionRange;
        Transform nearest = null;

        foreach (var obj in DestructibleTracker.Instance.GetInRange(transform.position, detectionRange))
        {
            float dist = Vector2.Distance(transform.position, obj.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
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
        AllyStats self = this.GetStats();
        float minDistance = Mathf.Infinity;
        AllyStats nearest = null;

        var allies = AllyManager.Instance.GetAllies();
        foreach (var ally in allies)
        {
            if (ally == this || ally.GetStats().IsDead)
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
        if (player == null)
            return null;

        return player.GetComponent<PlayerStats>();
    }
    
    protected virtual void OnEnable()
    {
        AllyManager.Instance.RegisterAlly(this);
    }

    protected virtual void OnDisable()
    {
        AllyManager.Instance.UnregisterAlly(this);
    }


}
