using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab; // Prefab của enemy
    [SerializeField] private float spawnInterval = 2f; // Khoảng thời gian giữa các lần spawn
    [SerializeField] private int enemiesPerWave = 3; // Số lượng enemy mỗi lần spawn

    [SerializeField] private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        InvokeRepeating(nameof(SpawnWave), 1f, spawnInterval);
    }

    private void SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnerEnemy();
        }
    }

    private void SpawnerEnemy()
    {
        Vector2 spawnPosition = GetRandomSpawnPosition();
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }

    private Vector2 GetRandomSpawnPosition()
    {
        float screenX, screenY;
        int edge = Random.Range(0, 4); // Chọn cạnh spawn ngẫu nhiên (0: Trên, 1: Dưới, 2: Trái, 3: Phải)

        switch (edge)
        {
            case 0: // Trên
                screenX = Random.Range(0f, 1f);
                screenY = 1.1f;
                break;
            case 1: // Dưới
                screenX = Random.Range(0f, 1f);
                screenY = -0.1f;
                break;
            case 2: // Trái
                screenX = -0.1f;
                screenY = Random.Range(0f, 1f);
                break;
            case 3: // Phải
                screenX = 1.1f;
                screenY = Random.Range(0f, 1f);
                break;
            default:
                screenX = 0.5f;
                screenY = 0.5f;
                break;
        }

        return mainCamera.ViewportToWorldPoint(new Vector3(screenX, screenY, 0));
    }
}