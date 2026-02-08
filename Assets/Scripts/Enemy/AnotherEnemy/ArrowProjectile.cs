using UnityEngine;

public class ArrowProjectile: MonoBehaviour
{
    [Header("Move")]
    public float speed = 6f;
    public float lifeTime = 2.5f;

    [Header("Visual")]
    [SerializeField] private Transform visual;

    private int damage;
    private float critChance;
    private Vector2 dir;

    public void Init(int dmg, Vector2 direction, float crit)
    {
        damage = dmg;
        dir = direction.normalized;
        critChance = crit;

        // đảm bảo không dính scale từ object cha
        transform.localScale = Vector3.one;

        if (visual != null)
        {
            visual.localScale = Vector3.one * 0.24f; // 👈 CHỈNH Ở ĐÂY

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            visual.rotation = Quaternion.Euler(0f, 0f, angle);
        }



        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out EnemyAI enemy))
        {
            bool isCrit = Random.Range(0f, 100f) < critChance;
            int finalDamage = isCrit
                ? Mathf.RoundToInt(damage * 1.5f)
                : damage;

            enemy.TakeDamage(finalDamage, isCrit);
            Destroy(gameObject);
            return;
        }

        if (other.TryGetComponent(out DestructibleObject destructible))
        {
            destructible.Hit();
            Destroy(gameObject);
        }
    }
}