using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Vector3 direction;
    private int damage;
    public float speed = 5f;

    public void Init(Vector3 targetPosition, int damage)
    {
        this.damage = damage;
        direction = (targetPosition - transform.position).normalized;
        Destroy(gameObject, 5f); // tự hủy sau 5s
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out PlayerHealth health))
        {
            health.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}