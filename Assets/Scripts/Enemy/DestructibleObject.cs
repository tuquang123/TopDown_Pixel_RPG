using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHits = 3; // số hit cần để phá
    private int currentHits;

    [Header("Drops")]
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private int minGold = 1;
    [SerializeField] private int maxGold = 3;

    [Header("VFX")]
    [SerializeField] private GameObject destructionVFX;

    private void OnEnable()
    {
        currentHits = 0;
    }

    public void Hit()
    {
        currentHits++;

        if (currentHits >= maxHits)
        {
            HandleDestruction();
        }
    }

    private void HandleDestruction()
    {
        // Spawn VFX
        if (destructionVFX)
        {
            ObjectPooler.Instance.Get("VFX", destructionVFX, transform.position, Quaternion.identity);
        }

        // Spawn gold
        int dropCount = Random.Range(minGold, maxGold + 1);
        for (int i = 0; i < dropCount; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
            ObjectPooler.Instance.Get("Gold", goldPrefab, transform.position + offset, Quaternion.identity);
        }

        gameObject.SetActive(false);
    }
}