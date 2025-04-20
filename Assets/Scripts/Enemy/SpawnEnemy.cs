using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int enemiesPerWave = 3;
    [SerializeField] private GameObject enemyUI;
    [SerializeField] private GameObject canvasHp;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        InvokeRepeating(nameof(SpawnWave), 1f, spawnInterval);
    }

    private void SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            TrySpawnEnemy();
        }
    }

    private void TrySpawnEnemy()
    {
        // Nếu không tìm được vị trí phù hợp sau nhiều lần, spawn đại
        SpawnAt(GetRandomSpawnPosition());
    }

    private void SpawnAt(Vector2 position)
    {
        int index = Random.Range(0, enemyPrefabs.Length);
        GameObject prefab = enemyPrefabs[index];
        string tag = prefab.name;  // Sử dụng tên prefab làm tag

        GameObject enemy = ObjectPooler.Instance.Get(
            tag, prefab, position, Quaternion.identity, initSize: 10, expandable: true
        );

        if (enemy == null) return;
        
        enemy.GetComponent<EnemyAI>().ResetEnemy();

        if (enemy.GetComponent<EnemyAI>().enemyHealthUI == null)
        {
            SetupHealthUI(enemy);
        }
    }

    private void SetupHealthUI(GameObject enemy)
    {
        GameObject ui = Instantiate(enemyUI, canvasHp.transform, false);
        var uiComponent = ui.GetComponent<EnemyHealthUI>();
        uiComponent.SetTarget(enemy);
        enemy.GetComponent<EnemyAI>().enemyHealthUI = uiComponent;
    }
    
    private Vector2 GetRandomSpawnPosition()
    {
        int edge = Random.Range(0, 4);
        float x = 0, y = 0;

        switch (edge)
        {
            case 0: x = Random.Range(0f, 1f); y = 1.1f; break;
            case 1: x = Random.Range(0f, 1f); y = -0.1f; break;
            case 2: x = -0.1f; y = Random.Range(0f, 1f); break;
            case 3: x = 1.1f; y = Random.Range(0f, 1f); break;
        }

        Vector2 screenPoint = new(x, y);
        return (Vector2)mainCamera.ViewportToWorldPoint(screenPoint) + Random.insideUnitCircle * 0.5f;
    }
}
