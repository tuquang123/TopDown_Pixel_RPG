using UnityEngine;
using Sirenix.OdinInspector;

public class SpawnEnemy : MonoBehaviour
{
    
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

    public int timeBoss = 15;
    
    [Title("Spawn Points Settings")]
    [Tooltip("Các điểm spawn cố định")]
    
    [ReadOnly]
    [ShowInInspector]
    private Camera mainCamera;

    [SerializeField] private SpawnPoint[] spawnPoints;

    private void Start()
    {
        foreach (var sp in spawnPoints)
        {
            sp.enemyUI = enemyUI;
            sp.canvasHp = canvasHp;
            sp.Spawn();
        }

        Invoke(nameof(SpawnBoss), timeBoss);
    }

    private void SpawnBoss()
    {
        GameObject boss = Instantiate(bossPrefab,transform.position, Quaternion.identity);

        if (boss.GetComponent<BossAI>() != null)
        {
            boss.GetComponent<BossAI>().ResetEnemy(); 
        }
        boss.SetActive(true);
        
        Debug.Log("Boss spawned!");
    }
}
