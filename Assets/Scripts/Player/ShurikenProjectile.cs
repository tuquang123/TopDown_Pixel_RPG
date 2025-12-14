using UnityEngine;

public class ShurikenProjectile : MonoBehaviour
{
    [Header("Move")]
    public float speed = 10f;
    public float lifeTime = 3f;

    [Header("Spin")]
    [SerializeField] private Transform visual;
    public float spinSpeed = 1080f;

    private int damage;
    private float critChance;
    private float lifeSteal;
    private Vector2 dir;

    private float spinDir = -1f; // mặc định xoay âm

    public void Init(int dmg, Vector2 direction, float crit, float lifeSteal)
    {
        damage = dmg;
        dir = direction.normalized;
        critChance = crit;
        this.lifeSteal = lifeSteal;

        // 🚫 CẮT SCALE ÂM KẾ THỪA
        transform.localScale = Vector3.one;

        if (visual != null)
        {
            visual.localScale = Vector3.one;

            // 👉 BAY SANG TRÁI
            if (dir.x < 0f)
            {
                visual.localScale = new Vector3(-1, 1, 1);
                spinDir = 1f; // đổi chiều xoay
            }
            else
            {
                spinDir = -1f;
            }
        }

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // MOVE
        transform.position += (Vector3)(dir * speed * Time.deltaTime);

        // SPIN
        if (visual != null)
        {
            visual.Rotate(0f, 0f, spinDir * spinSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out EnemyAI enemy))
        {
            bool isCrit = Random.Range(0f, 100f) < critChance;
            int finalDamage = isCrit ? Mathf.RoundToInt(damage * 1.5f) : damage;

            enemy.TakeDamage(finalDamage, isCrit);
            HandleLifeSteal(finalDamage);
            Destroy(gameObject);
        }
        
        // ===== Destructible =====
        if (other.TryGetComponent(out DestructibleObject destructible))
        {
            destructible.Hit();
            Destroy(gameObject);
        }
    }

    private void HandleLifeSteal(int dmg)
    {
        if (lifeSteal <= 0f) return;

        int heal = Mathf.RoundToInt(dmg * lifeSteal);
        if (heal <= 0) return;

        var stats = PlayerController.Instance.GetComponent<PlayerStats>();
        if (stats == null) return;

        stats.Heal(heal);
    }
}
