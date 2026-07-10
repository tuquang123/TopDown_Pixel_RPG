using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private int damage;
    private GameObject owner;
    private float criticalChance;
    private float criticalDamageMultiplier = 1.5f;

    [SerializeField] private float lifetime = 3f;
    
    public void Init(int dmg, GameObject ownerObj, float critChance = 0f, float critMultiplier = 1.5f)
    {
        damage = dmg;
        owner = ownerObj;
        criticalChance = critChance;
        criticalDamageMultiplier = Mathf.Max(1f, critMultiplier);
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == owner) return;

        if (other.TryGetComponent(out IDamageable damageable))
        {
            bool isCrit = Random.Range(0f, 100f) < criticalChance;
            int finalDamage = isCrit
                ? Mathf.RoundToInt(damage * criticalDamageMultiplier)
                : damage;

            damageable.TakeDamage(finalDamage, isCrit);
            Destroy(gameObject);
        }
    }
}
