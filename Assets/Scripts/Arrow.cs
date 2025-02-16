using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Transform target;
    private Vector2 startPosition;
    private Vector2 controlPoint; // Điểm giữa tạo đường cong
    private float timeElapsed;
    private float flightDuration = 0.5f; // Thời gian bay
    private int damage;

    [SerializeField] private TrailRenderer arrowTrail; // Tham chiếu tới TrailRenderer

    // Thiết lập mũi tên và chỉ định mục tiêu
    public void SetTarget(Transform enemy, int dmg)
    {
        target = enemy;
        damage = dmg;
        startPosition = transform.position;

        // Xác định điểm giữa cao hơn để tạo vòng cung
        Vector2 midPoint = (startPosition + (Vector2)target.position) / 2;
        controlPoint = midPoint + Vector2.up * 4.5f; // Điều chỉnh độ cong

        timeElapsed = 0;

        // Thiết lập kích thước của TrailRenderer
        if (arrowTrail != null)
        {
            arrowTrail.startWidth = 0.1f;  // Điều chỉnh kích thước đầu
            arrowTrail.endWidth = 0.05f;   // Điều chỉnh kích thước cuối
        }
    }

    // Cập nhật vị trí mũi tên theo đường cong Bezier
    private void Update()
    {
        if (target == null) return;

        timeElapsed += Time.deltaTime;
        float t = timeElapsed / flightDuration;
        if (t > 1) t = 1;

        // Tính toán vị trí mới theo đường cong Bezier
        Vector2 newPos = GetBezierPoint(t);
        transform.position = newPos;

        // Tính toán góc Z thay đổi theo thời gian (t)
        float angleZ = Mathf.Lerp(40, 150, t); // Interpolate between 40 and 135

        // Xoay mũi tên theo hướng chuyển động
        if (t < 1)
        {
            Vector2 direction = (newPos - (Vector2)transform.position).normalized;

            // Tính toán góc giữa vectơ chuyển động và trục X
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Xoay mũi tên theo góc X và Z (tính theo đường bay và thay đổi góc Z)
            transform.rotation = Quaternion.Euler(0, 0, angle + angleZ);
        }
    }

    // Tính toán vị trí mới trên đường Bezier
    private Vector2 GetBezierPoint(float t)
    {
        return (1 - t) * (1 - t) * startPosition + 
               2 * (1 - t) * t * controlPoint + 
               t * t * (Vector2)target.position;
    }

    // Kiểm tra va chạm với kẻ địch và gây sát thương
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyAI>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
