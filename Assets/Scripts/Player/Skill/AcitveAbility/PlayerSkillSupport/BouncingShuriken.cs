using UnityEngine;

public class BouncingShuriken : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 10;
    public int maxBounces = 5;
    public float rotationSpeed = 500f;

    private Vector2 direction;
    private int bounceCount = 0;
    private float minX, maxX, minY, maxY;

    void Start()
    {
        // Tính toán rìa màn hình
        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));

        minX = bottomLeft.x;
        maxX = topRight.x;
        minY = bottomLeft.y;
        maxY = topRight.y;
    }

    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        CheckScreenBounds();
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }

    private void CheckScreenBounds()
    {
        Vector3 pos = transform.position;

        if (pos.x <= minX || pos.x >= maxX)
        {
            direction.x = -direction.x;
            bounceCount++;
        }
        if (pos.y <= minY || pos.y >= maxY)
        {
            direction.y = -direction.y;
            bounceCount++;
        }

        if (bounceCount >= maxBounces)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyAI>()?.TakeDamage(damage);
        }
    }
}