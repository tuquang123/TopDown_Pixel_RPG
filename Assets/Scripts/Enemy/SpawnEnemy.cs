using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int enemiesPerWave = 3;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject enemyUI;  // Tham chiếu đến UI cho enemy
    [SerializeField] private GameObject canvasHp;

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

        // Dùng GetToPool thay cho SpawnFromPool để tự tạo pool nếu chưa có
        GameObject enemy = ObjectPooler.Instance.GetToPool(
            "Enemy",
            enemyPrefab,                          // Prefab bạn truyền từ Inspector
            spawnPosition,
            Quaternion.identity,
            initSize: 5,                          // Kích thước pool khởi tạo (tùy ý)
            expandable: true                      // Cho phép mở rộng nếu thiếu
        );

        // Reset trạng thái cần thiết
        enemy.GetComponent<EnemyAI>().ResetEnemy();

        // Tạo lại UI nếu chưa có
        if (enemy.GetComponent<EnemyAI>().enemyHealthUI == null)
        {
            CreateEnemyUI(enemy);
        }
    }

    private void CreateEnemyUI(GameObject enemy)
    {
        // Tạo đối tượng UI thanh HP và gán vào vị trí của enemy
        var enemyHealthUI = Instantiate(enemyUI, canvasHp.transform, false);
        enemyHealthUI.GetComponent<EnemyHealthUI>().SetTarget(enemy);  // Gán đối tượng enemy cho UI
        enemy.GetComponent<EnemyAI>().enemyHealthUI = enemyHealthUI.GetComponent<EnemyHealthUI>();
    }

    private Vector2 GetRandomSpawnPosition()
    {
        float screenX, screenY;
        int edge = Random.Range(0, 4); // Chọn cạnh spawn ngẫu nhiên

        switch (edge)
        {
            case 0: screenX = Random.Range(0f, 1f); screenY = 1.1f; break;
            case 1: screenX = Random.Range(0f, 1f); screenY = -0.1f; break;
            case 2: screenX = -0.1f; screenY = Random.Range(0f, 1f); break;
            case 3: screenX = 1.1f; screenY = Random.Range(0f, 1f); break;
            default: screenX = 0.5f; screenY = 0.5f; break;
        }

        return mainCamera.ViewportToWorldPoint(new Vector3(screenX, screenY, 0));
    }
}