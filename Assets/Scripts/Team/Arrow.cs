using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Transform target;
    private Vector2 startPosition;
    private Vector2 controlPoint;
    private float timeElapsed;
    private float flightDuration = 0.5f;
    private int damage;

    [SerializeField] private TrailRenderer arrowTrail;

    public void SetTarget(Transform enemy, int dmg)
    {
        target = enemy;
        damage = dmg;
        startPosition = transform.position;

        // Xác định điểm giữa cao hơn để tạo vòng cung
        Vector2 midPoint = (startPosition + (Vector2)target.position) / 2;
        controlPoint = midPoint + Vector2.up * 2.5f;

        timeElapsed = 0;

        // Thiết lập kích thước của TrailRenderer
        if (arrowTrail != null)
        {
            arrowTrail.startWidth = 0.1f;
            arrowTrail.endWidth = 0.05f;
        }

        Destroy(gameObject, 2f);
    }

    private void Update()
    {
        // Nếu mục tiêu biến mất, hủy mũi tên ngay
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        timeElapsed += Time.deltaTime;
        float t = timeElapsed / flightDuration;
        if (t > 1) t = 1;

        // Cập nhật vị trí mũi tên
        Vector2 newPos = GetBezierPoint(t);
        transform.position = newPos;

        // Xác định hướng di chuyển
        Vector2 direction = (newPos - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Cập nhật góc xoay của mũi tên
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private Vector2 GetBezierPoint(float t)
    {
        return (1 - t) * (1 - t) * startPosition +
               2 * (1 - t) * t * controlPoint +
               t * t * (Vector2)target.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

}
