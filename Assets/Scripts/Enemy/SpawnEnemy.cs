using UnityEngine;
using Sirenix.OdinInspector;

public class SpawnEnemy : MonoBehaviour
{
    [Title("Enemy Level Data")] [SerializeField]
    private EnemyLevelDatabase levelDatabase;

    public EnemyLevelDatabase LevelDatabase => levelDatabase;
    
    [Title("UI Settings")] [Tooltip("Prefab UI máu của kẻ địch")] [Required] [SerializeField]
    private GameObject enemyUI;

    [Tooltip("Canvas chứa các UI máu kẻ địch")] [SceneObjectsOnly] [SerializeField]
    private GameObject canvasHp;

    [Title("Boss Settings")] [Tooltip("Prefab Boss")] [Required] [SerializeField]
    private GameObject bossPrefab;

    public int timeBoss = 15;

    [Title("Spawn Points Settings")] [Tooltip("Các điểm spawn cố định")] [ReadOnly] [ShowInInspector]
    private Camera mainCamera;
    
    [SerializeField] private SpawnPoint[] spawnPoints;
    
    public GameObject EnemyUI => enemyUI;
    
    public GameObject BossPrefab => bossPrefab;
    
    public GameObject CanvasHp => canvasHp;

    private void Awake()
    {
        canvasHp = RefVFX.Instance.canvasHp;
        
        if (canvasHp == null)
        {
            GameObject foundCanvas = GameObject.FindWithTag("EnemyUICanvas");
            
            if (foundCanvas == null)
                foundCanvas = GameObject.Find("HpEnemy");

            if (foundCanvas != null)
            {
                canvasHp = foundCanvas;
                Debug.Log("Đã tự động gán Canvas HP.");
            }
            else
            {
                Debug.LogWarning("Không tìm thấy canvas HP! Bạn nên gán thủ công hoặc đặt tag/tên phù hợp.");
            }
        }
    }

    private void Start()
    {
        foreach (var sp in spawnPoints)
        {
            sp.enemyUI = enemyUI;
            sp.canvasHp = canvasHp;
            //sp.Spawn(levelDatabase);
        }

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