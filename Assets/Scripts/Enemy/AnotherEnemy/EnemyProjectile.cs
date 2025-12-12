using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private int damage;
    private GameObject owner;

    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.1f;
    
    public void Init(int dmg, GameObject ownerObj)
    {
        damage = dmg;
        owner = ownerObj;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == owner) return;

        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);

            // === Nếu trúng Player → knockback ===
            if (other.TryGetComponent(out PlayerController player))
            {
                Vector2 dir = (player.transform.position - transform.position).normalized;
                player.ApplyKnockback(dir, knockbackForce, knockbackDuration);
            }

            Destroy(gameObject);
        }
    }

}