using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private int damage;
    private GameObject owner;

    [SerializeField] private float lifetime = 3f;

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
            Destroy(gameObject);
        }
    }
}