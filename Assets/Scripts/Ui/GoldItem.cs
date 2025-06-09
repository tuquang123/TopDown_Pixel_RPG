using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GoldItem : MonoBehaviour, IPooledObject
{
    public int value = 1;
    private Rigidbody2D rb;

    public float flyForce = 2f;
    public float torqueForce = 30f;

    public void OnObjectSpawn()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // Reset lại velocity khi spawn
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;

        // Random hướng bay: hơi lên trên và sang ngang
        Vector2 randomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f)).normalized;
        rb.AddForce(randomDir * flyForce, ForceMode2D.Impulse);

        // Xoay nhẹ
        rb.AddTorque(Random.Range(-torqueForce, torqueForce));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CurrencyManager.Instance.AddGold(value);
            FloatingTextSpawner.Instance.SpawnText("+" + value, transform.position, Color.yellow);
            gameObject.SetActive(false); // trả về pool
        }
    }
}