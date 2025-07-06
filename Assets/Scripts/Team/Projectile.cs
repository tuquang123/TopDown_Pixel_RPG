using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Projectile : MonoBehaviour, IPooledObject
{
    private Transform target;
    private int damage;
    private float speed = 5f;
    private float maxLifetime = 3f;

    public void SetTarget(Transform newTarget, int dmg)
    {
        target = newTarget;
        damage = dmg;
        CancelInvoke(nameof(Despawn));
        Invoke(nameof(Despawn), maxLifetime); 
    }

    public void OnObjectSpawn()
    {
        // reset trạng thái nếu cần
    }

    private void Update()
    {
        if (target == null)
        {
            Despawn();
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            if (target.TryGetComponent<IDamageable>(out var dmgTarget))
                dmgTarget.TakeDamage(damage);

            Despawn();
        }
    }

    private void Despawn()
    {
        gameObject.SetActive(false);
    }
    
    private void Reset()
    {
        AutoAddRequiredComponents();
    }

    private void OnValidate()
    {
        AutoAddRequiredComponents();
    }

    private void AutoAddRequiredComponents()
    {
        if (!TryGetComponent(out Collider2D collider))
        {
            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.15f;
            Debug.Log($"[{name}] Tự động thêm Collider2D.");
        }

        if (!TryGetComponent(out Rigidbody2D rb))
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.isKinematic = true;
            Debug.Log($"[{name}] Tự động thêm Rigidbody2D.");
        }
    }
}
