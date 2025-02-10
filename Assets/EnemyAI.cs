using UnityEngine;

public class EnemyAI : MonoBehaviour
{
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
            anim.SetBool("Move", false); // Đứng yên nếu không thấy người chơi
        }
    }

    void MoveTowardsPlayer()
    {
        anim.SetBool("1_Move", true);
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
            anim.SetTrigger("2_Attack");
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