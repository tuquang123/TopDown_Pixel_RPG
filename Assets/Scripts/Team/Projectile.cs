using UnityEngine;

public class Projectile : MonoBehaviour, IPooledObject
{
    private Transform target;
    private int damage;
    private float speed = 5f;
    private float maxLifetime = 3f;
    private float hitDistance = 0.2f;

    public void SetTarget(Transform newTarget, int dmg)
    {
        target = newTarget;
        damage = dmg;
        CancelInvoke(nameof(Despawn));
        Invoke(nameof(Despawn), maxLifetime); 
    }

    public void OnObjectSpawn()
    {
        // Reset nếu cần
    }

    private void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Despawn();
            return;
        }

        // Move toward target
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        // Hit check bằng khoảng cách
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance < hitDistance)
        {
            // Gây damage trực tiếp
            if (target.TryGetComponent<EnemyAI>(out var enemy))
            {
                if (!enemy.IsDead)
                    enemy.TakeDamage(damage);
            }
            else if (target.TryGetComponent<DestructibleObject>(out var destructible))
            {
                destructible.Hit();
            }

            Despawn();
        }
    }

    private void Despawn()
    {
        gameObject.SetActive(false);
    }
}