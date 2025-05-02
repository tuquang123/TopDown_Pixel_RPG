using UnityEngine;
using Sirenix.OdinInspector;

public class SpawnEnemy : MonoBehaviour
{
    [Title("Enemy Settings")]
    [Tooltip("Danh sách prefab kẻ địch sẽ được spawn")]
    [AssetsOnly]
    [Required]
    [ListDrawerSettings(Expanded = true)]
    [SerializeField] private GameObject[] enemyPrefabs;

    [MinValue(0.1f)]
    [Tooltip("Thời gian giữa các đợt spawn (giây)")]
    [SerializeField] private float spawnInterval = 2f;

    [MinValue(1)]
    [Tooltip("Số lượng kẻ địch spawn mỗi đợt")]
    [SerializeField] private int enemiesPerWave = 3;

    [Title("UI Settings")]
    [Tooltip("Prefab UI máu của kẻ địch")]
    [Required]
    [SerializeField] private GameObject enemyUI;

    [Tooltip("Canvas chứa các UI máu kẻ địch")]
    [SceneObjectsOnly]
    [SerializeField] private GameObject canvasHp;
    
    [Title("Boss Settings")]
    [Tooltip("Prefab Boss")]
    [Required]
    [SerializeField] private GameObject bossPrefab;


    [ReadOnly]
    [ShowInInspector]
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        InvokeRepeating(nameof(SpawnWave), 1f, spawnInterval);
        Invoke(nameof(SpawnBoss), 60f); // 1 phút
    }
    private void SpawnBoss()
    {
        Vector2 bossPos = GetRandomSpawnPosition(); 
        GameObject boss = Instantiate(bossPrefab, bossPos, Quaternion.identity);

        if (boss.GetComponent<BossAI>() != null)
        {
            boss.GetComponent<BossAI>().ResetEnemy(); 
        }
        boss.SetActive(true);
        
        Debug.Log("Boss spawned!");
    }



    [Button("Force Spawn Wave")]
    private void SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            TrySpawnEnemy();
        }
    }

    private void TrySpawnEnemy()
    {
        SpawnAt(GetRandomSpawnPosition());
    }

    private void SpawnAt(Vector2 position)
    {
        int index = Random.Range(0, enemyPrefabs.Length);
        GameObject prefab = enemyPrefabs[index];
        string tag = prefab.name;

        GameObject enemy = ObjectPooler.Instance.Get(
            tag, prefab, position, Quaternion.identity, initSize: 10, expandable: true
        );

        if (enemy == null) return;

        enemy.GetComponent<EnemyAI>().ResetEnemy();

        if (enemy.GetComponent<EnemyAI>().EnemyHealthUI == null)
        {
            SetupHealthUI(enemy);
        }
    }

    private void SetupHealthUI(GameObject enemy)
    {
        GameObject ui = Instantiate(enemyUI, canvasHp.transform, false);
        var uiComponent = ui.GetComponent<EnemyHealthUI>();
        uiComponent.SetTarget(enemy);
        enemy.GetComponent<EnemyAI>().EnemyHealthUI = uiComponent;
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
