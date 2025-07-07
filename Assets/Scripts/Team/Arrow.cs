using UnityEngine;

public class Arrow : MonoBehaviour, IPooledObject
{
    private Transform target;
    private Vector2 startPosition;
    private Vector2 controlPoint;
    private float timeElapsed;
    private float flightDuration = 0.5f;
    private int damage;
    private bool isFlying = false;

    [SerializeField] private TrailRenderer arrowTrail;

    private float hitDistance = 0.2f;

    public void SetTarget(Transform enemy, int dmg)
    {
        target = enemy;
        damage = dmg;
        startPosition = transform.position;

        Vector2 midPoint = (startPosition + (Vector2)target.position) / 2f;
        controlPoint = midPoint + Vector2.up * 2.5f;

        timeElapsed = 0f;
        isFlying = true;

        if (arrowTrail != null)
        {
            arrowTrail.Clear();
            arrowTrail.startWidth = 0.1f;
            arrowTrail.endWidth = 0.05f;
        }

        Invoke(nameof(DisableSelf), 2f); // timeout fallback
    }

    private void Update()
    {
        if (!isFlying || target == null || !target.gameObject.activeInHierarchy)
        {
            DisableSelf();
            return;
        }

        timeElapsed += Time.deltaTime;
        float t = timeElapsed / flightDuration;
        if (t > 1f) t = 1f;

        Vector2 newPos = GetBezierPoint(t);
        Vector2 moveDir = newPos - (Vector2)transform.position;
        transform.position = newPos;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (Vector2.Distance(transform.position, target.position) <= hitDistance)
        {
            // Gây damage
            if (target.TryGetComponent<IDamageable>(out var dmgTarget))
            {
                dmgTarget.TakeDamage(damage);
            }
            else if (target.TryGetComponent<DestructibleObject>(out var destructible))
            {
                destructible.Hit();
            }

            DisableSelf();
        }
    }


    private Vector2 GetBezierPoint(float t)
    {
        return (1 - t) * (1 - t) * startPosition +
               2 * (1 - t) * t * controlPoint +
               t * t * (Vector2)target.position;
    }

    private void DisableSelf()
    {
        isFlying = false;
        CancelInvoke();
        gameObject.SetActive(false);
    }

    public void OnObjectSpawn()
    {
        isFlying = false;
        timeElapsed = 0;
        transform.rotation = Quaternion.identity;
    }
}
