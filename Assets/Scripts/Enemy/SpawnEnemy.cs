using UnityEngine;
using Sirenix.OdinInspector;

public class SpawnEnemy : MonoBehaviour
{
    [Title("Boss Settings")] [Tooltip("Prefab Boss")] [Required] [SerializeField]
    private GameObject bossPrefab;

    public int timeBoss = 15;
    private void Start()
    {
        if (bossPrefab == null) return;
        bossPrefab.SetActive(false);
        Invoke(nameof(SpawnBoss), timeBoss);
    }

    private void SpawnBoss()
    {
        bossPrefab.SetActive(true);
        if (bossPrefab.GetComponent<BossAI>() != null)
        {
            bossPrefab.GetComponent<BossAI>().ResetEnemy();
        }

        bossPrefab.SetActive(true);
    }
}