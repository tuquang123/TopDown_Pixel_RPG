using UnityEngine;

public class GoldItem : MonoBehaviour, IPooledObject
{
    public int value = 1;

    public void OnObjectSpawn()
    {
        // Reset state mỗi lần spawn từ pool nếu cần
        // Ví dụ hiệu ứng, animation, random hướng bay...
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CurrencyManager.Instance.AddGold(value);
            FloatingTextSpawner.Instance.SpawnText("+" + value, transform.position, Color.yellow);

            gameObject.SetActive(false); // Trả về pool
        }
    }
}