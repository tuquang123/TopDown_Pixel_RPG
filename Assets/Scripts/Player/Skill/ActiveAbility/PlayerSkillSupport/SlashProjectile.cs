using UnityEngine;

public class SlashProjectile : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 20;
    private Vector2 direction;

    public void Initialize(Vector2 targetPosition, int attackDamage)
    {
        direction = (targetPosition - (Vector2)transform.position).normalized;
        damage = attackDamage;
        Destroy(gameObject, 2f); // Hủy sau 2s tránh rác bộ nhớ
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<EnemyAI>()?.TakeDamage(damage);
            Destroy(gameObject); // Hủy slash sau khi va chạm
        }
    }
}