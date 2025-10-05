using UnityEngine;
using Sirenix.OdinInspector;

public class SpawnEnemy : MonoBehaviour
{
    [Title("Boss Settings")] [Tooltip("Prefab Boss")] [Required] [SerializeField]
    private GameObject bossPrefab;

    public int timeBoss = 15;

    [Title("Spawn Points Settings")] [Tooltip("Các điểm spawn cố định")] [ReadOnly] [ShowInInspector]
    private Camera mainCamera;
    
    //[SerializeField] private SpawnPoint[] spawnPoints;
    
    private void Start()
    {
        if (bossPrefab == null) return;
        Invoke(nameof(SpawnBoss), timeBoss);
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null) return;
        
        GameObject boss = Instantiate(bossPrefab, transform.position, Quaternion.identity);

        if (boss.GetComponent<BossAI>() != null)
        {
            boss.GetComponent<BossAI>().ResetEnemy();
        }

        boss.SetActive(true);

        Debug.Log("Boss spawned!");
    }
}