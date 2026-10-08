using UnityEngine;

/// <summary>
/// A visible, single-target boss projectile. It follows its intended target just enough to
/// communicate the attack clearly, then applies damage only on contact.
/// </summary>
public class BossTargetedProjectile : MonoBehaviour
{
    private const float HitDistance = 0.22f;

    private Transform target;
    private GameObject owner;
    private int damage;
    private float speed;
    private float remainingLifetime = 2.5f;
    private bool hasHit;

    public void Initialize(Transform targetTransform, GameObject ownerObject, int hitDamage, float moveSpeed)
    {
        target = targetTransform;
        owner = ownerObject;
        damage = hitDamage;
        speed = moveSpeed;

        ApplyReadableVisuals();
    }

    private void Update()
    {
        remainingLifetime -= Time.deltaTime;
        if (remainingLifetime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = target.position;
        Vector2 toTarget = targetPosition - currentPosition;
        float distance = toTarget.magnitude;

        if (distance <= HitDistance)
        {
            HitTarget();
            return;
        }

        Vector2 direction = toTarget / distance;
        transform.position = currentPosition + direction * Mathf.Min(speed * Time.deltaTime, distance);
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    private void HitTarget()
    {
        if (hasHit)
            return;

        hasHit = true;
        if (target != null && target.TryGetComponent(out IDamageable damageable))
            damageable.TakeDamage(damage);

        Destroy(gameObject);
    }

    private void ApplyReadableVisuals()
    {
        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.color = new Color(1f, 0.28f, 0.06f, 1f);
            renderer.sortingOrder = 90;
        }

        if (TryGetComponent(out TrailRenderer trail))
        {
            trail.time = 0.32f;
            trail.startWidth = 0.18f;
            trail.endWidth = 0.02f;
            trail.sortingOrder = 89;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 0.95f, 0.35f), 0f), new GradientColorKey(new Color(1f, 0.12f, 0.02f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            trail.colorGradient = gradient;
        }
    }
}
