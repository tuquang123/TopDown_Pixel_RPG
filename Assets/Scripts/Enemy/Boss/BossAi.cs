using UnityEngine;

public class BossAI : EnemyAI
{
    public enum BossState { Idle, Chase, Attack, CastSkill, Dead }
    private BossState state = BossState.Idle;

    [Header("Boss Stats")]
    [SerializeField] private float maxHP = 4500f;
    private float currentHP;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float basicAttackDamage = 90f;

    [Header("Skills")]
    [SerializeField] private float smashCooldown = 6f;      // Đập búa AOE
    [SerializeField] private float roarCooldown = 12f;      // Gầm buff damage + gọi lính
    [SerializeField] private float leapCooldown = 10f;      // Nhảy vào player
    [SerializeField] private float enrageThreshold = 0.4f;  // Dưới 40% HP thì Enrage

    private float smashTimer;
    private float roarTimer;
    private float leapTimer;
    private bool enraged = false;

    [Header("Minions")]
    [SerializeField] private GameObject orcMinionPrefab;  // Prefab orc nhỏ
    [SerializeField] private int minionCount = 3;         // Số lính gọi ra
    [SerializeField] private float spawnRadius = 3f;      // Vòng spawn quanh boss

    private Transform player;

    protected override void Start()
    {
        base.Start();
        currentHP = maxHP;
        player = GameObject.FindWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (state == BossState.Dead || player == null) return;

        smashTimer -= Time.deltaTime;
        roarTimer -= Time.deltaTime;
        leapTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, player.position);

        // Check Enrage Phase
        if (!enraged && currentHP <= maxHP * enrageThreshold)
        {
            EnterEnrage();
        }

        if (dist <= attackRange)
        {
            if (smashTimer <= 0)
                CastSmash();
            else
                BasicAttack();
        }
        else
        {
            if (roarTimer <= 0)
                CastRoar();
            else if (leapTimer <= 0)
                CastLeap();
            else
                ChasePlayer();
        }
    }

    private void ChasePlayer()
    {
        state = BossState.Chase;
        transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
    }

    private void BasicAttack()
    {
        state = BossState.Attack;
        Debug.Log("Orc chém Player gây " + basicAttackDamage + " damage!");
        // TODO: Gọi hàm trừ máu player
    }

    private void CastSmash()
    {
        state = BossState.CastSkill;
        smashTimer = smashCooldown;
        Debug.Log("Orc giơ búa đập xuống đất -> shockwave AOE gây damage lớn!");
        // TODO: Trigger animation, gây damage AOE
    }

    private void CastRoar()
    {
        state = BossState.CastSkill;
        roarTimer = roarCooldown;
        Debug.Log("Orc gầm thét, tăng damage và gọi đồng minh Orc nhỏ xuất hiện!");

        // Buff damage trong thời gian ngắn
        basicAttackDamage *= 1.2f;

        // Spawn minions
        for (int i = 0; i < minionCount; i++)
        {
            Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;
            Instantiate(orcMinionPrefab, spawnPos, Quaternion.identity);
        }
    }

    private void CastLeap()
    {
        state = BossState.CastSkill;
        leapTimer = leapCooldown;
        Debug.Log("Orc nhảy vọt tới Player, gây sát thương + hất tung khi đáp xuống!");
        // TODO: Trigger animation, dash tới player
    }

    private void EnterEnrage()
    {
        enraged = true;
        basicAttackDamage *= 1.5f;
        moveSpeed *= 1.3f;
        Debug.Log("Orc nổi điên (Enrage)! Tăng tốc độ và damage, hiệu ứng mắt đỏ rực.");
    }

    public override void TakeDamage(int dmg, bool isCrit = false)
    {
        if (state == BossState.Dead) return;

        currentHP -= dmg;
        base.TakeDamage(dmg, isCrit);
        Debug.Log("Orc nhận " + dmg + " damage. HP: " + currentHP);

        if (currentHP <= 0)
            Die();
    }

    protected override void Die()
    {
        if (state == BossState.Dead) return;
        state = BossState.Dead;

        Debug.Log("Orc gục xuống với tiếng gầm cuối cùng.");
        base.Die();
        // TODO: Animation chết, drop loot, báo QuestManager
    }
}
