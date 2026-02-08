using UnityEngine;

public class FireBallProjectile: MonoBehaviour
{
    [Header("Move")]
    public float speed = 12f;
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
            visual.localScale = Vector3.one;

            // lật hình nếu bay sang trái
            if (dir.x < 0f)
            {
                visual.localScale = new Vector3(-1, 1, 1);
            }
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