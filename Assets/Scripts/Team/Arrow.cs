using UnityEngine;

public class Arrow : MonoBehaviour, IPooledObject
{
    private Transform target;
    private Vector2 startPosition;
    private Vector2 controlPoint;
    private float timeElapsed;
    private float flightDuration = 0.5f;
    private int damage;

    [SerializeField] private TrailRenderer arrowTrail;

    private bool isFlying = false;

    public void SetTarget(Transform enemy, int dmg)
    {
        target = enemy;
        damage = dmg;
        startPosition = transform.position;

        Vector2 midPoint = (startPosition + (Vector2)target.position) / 2;
        controlPoint = midPoint + Vector2.up * 2.5f;

        timeElapsed = 0;
        isFlying = true;

        if (arrowTrail != null)
        {
            arrowTrail.Clear(); // reset trail
            arrowTrail.startWidth = 0.1f;
            arrowTrail.endWidth = 0.05f;
        }

        // Optional fallback: disable after timeout in case of no hit
        Invoke(nameof(DisableSelf), 2f);
    }

    private void Update()
    {
        if (!isFlying || target == null)
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
    }

    private Vector2 GetBezierPoint(float t)
    {
        return (1 - t) * (1 - t) * startPosition +
               2 * (1 - t) * t * controlPoint +
               t * t * (Vector2)target.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
            DisableSelf();
        }
    }

    private void DisableSelf()
    {
        isFlying = false;
        CancelInvoke(); // cancel any delayed call
        gameObject.SetActive(false); // pooling: hide instead of destroy
    }

    public void OnObjectSpawn()
    {
        isFlying = false;
        timeElapsed = 0;
        transform.rotation = Quaternion.identity;
    }
}
