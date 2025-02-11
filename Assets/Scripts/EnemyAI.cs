using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private static readonly int Property = Animator.StringToHash("1_Move");
    private static readonly int Property1 = Animator.StringToHash("2_Attack");
    [SerializeField] private float moveSpeed = 3f;  // Tốc độ di chuyển
    [SerializeField] private float attackRange = 1.5f;  // Tầm đánh
    [SerializeField] private float detectionRange = 5f; // Tầm phát hiện
    [SerializeField] private float attackCooldown = 1f; // Thời gian giữa các đòn tấn công

    private Transform player;
    private Animator anim;
    private float lastAttackTime;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Tìm người chơi theo tag
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange) // Nếu người chơi trong tầm phát hiện
        {
            if (distanceToPlayer > attackRange)
            {
                MoveTowardsPlayer();
            }
            else
            {
                AttackPlayer();
            }
        }
        else
        {
            anim.SetBool(Property, false); // Đứng yên nếu không thấy người chơi
        }
    }

    void MoveTowardsPlayer()
    {
        // Nếu đang tấn công thì không di chuyển
        if (Time.time - lastAttackTime < attackCooldown)
        {
            anim.SetBool(Property, false);
            return;
        }

        anim.SetBool(Property, true);
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

        // Xoay mặt theo hướng người chơi
        if (player.position.x > transform.position.x)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0); // Hướng phải
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 180, 0); // Hướng trái
        }
    }


    void AttackPlayer()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            anim.SetBool(Property, false); // Dừng di chuyển khi đánh
            anim.SetTrigger(Property1);
            lastAttackTime = Time.time;
        }
    }

    // 🔹 Vẽ Gizmos trong Scene View
    void OnDrawGizmosSelected()
    {
        // Màu xanh: Tầm phát hiện
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Màu đỏ: Tầm tấn công
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}