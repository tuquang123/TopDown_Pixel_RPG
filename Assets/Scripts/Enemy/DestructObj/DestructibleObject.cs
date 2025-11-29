using UnityEngine;
using System.Collections;

public class DestructibleObject : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHits = 3;
    private int currentHits;

    [Header("Drop Settings")]
    [SerializeField] private int minGold = 1;
    [SerializeField] private int maxGold = 3;

    [SerializeField, Range(0f, 1f)] private float gemDropChance = 0.5f;
    [SerializeField] private int minGem = 1;
    [SerializeField] private int maxGem = 2;

    [Header("Enemy Spawn On Break")]
    [SerializeField] private bool spawnEnemyOnBreak = false;
    [SerializeField, Range(0f, 1f)] private float enemySpawnChance = 1f;   // 1 = luôn spawn
    [SerializeField] private bool useSpawnPoint = false;

    [SerializeField] private SpawnPoint spawnPoint;      // nếu muốn dùng SpawnPoint
    [SerializeField] private GameObject[] enemyPrefabs;  // spawn trực tiếp

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeMagnitude = 0.05f;

    [Header("Respawn")]
    [SerializeField] private float respawnTime = 10f;

    private Vector3 originalPos;
    private Coroutine flashCoroutine;
    private string nameOBJ = "barrier";

    private void Awake()
    {
        originalPos = transform.localPosition;
    }

    private void OnEnable()
    {
        currentHits = 0;
        spriteRenderer.color = Color.white;
        transform.localPosition = originalPos;
        DestructibleTracker.Instance?.Register(this);
    }

    private void OnDisable()
    {
        DestructibleTracker.Instance?.Unregister(this);
    }

    public void Hit()
    {
        currentHits++;

        PlayHitFlash();
        StartCoroutine(Shake());

        if (currentHits >= maxHits)
            HandleDestruction();
    }

    private void PlayHitFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    private IEnumerator Shake()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            Vector3 randomOffset = Random.insideUnitCircle * shakeMagnitude;
            transform.localPosition = originalPos + new Vector3(randomOffset.x, randomOffset.y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }

    // =====================================================
    // PHÁ HỦY
    // =====================================================
    private void HandleDestruction()
    {
        // VFX phá
        if (CommonReferent.Instance.destructionVFXPrefab != null)
        {
            ObjectPooler.Instance.Get(
                "BreakVFX",
                CommonReferent.Instance.destructionVFXPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        // DROP VÀNG
        int totalGold = Random.Range(minGold, maxGold + 1);
        for (int i = 0; i < totalGold; i++)
        {
            Vector3 offset = Random.insideUnitCircle * 0.5f;
            ObjectPooler.Instance.Get(
                "Gold",
                CommonReferent.Instance.goldPrefab,
                transform.position + offset,
                Quaternion.identity
            );
        }

        // DROP GEM
        if (Random.value < gemDropChance)
        {
            int totalGem = Random.Range(minGem, maxGem + 1);
            for (int i = 0; i < totalGem; i++)
            {
                Vector3 offset = Random.insideUnitCircle * 0.5f;
                ObjectPooler.Instance.Get(
                    "Gem",
                    CommonReferent.Instance.gemPrefab,
                    transform.position + offset,
                    Quaternion.identity
                );
            }
        }

        // =====================================================
        // SPAWN QUÁI (THEO TÙY CHỌN)
        // =====================================================
        if (spawnEnemyOnBreak && Random.value <= enemySpawnChance)
        {
            SpawnEnemyLogic();
        }

        // BÁO NHIỆM VỤ
        QuestManager.Instance.ReportProgress("NV2", nameOBJ, 1);

        // ẨN VẬT THỂ
        gameObject.SetActive(false);

        // HỒI SINH
        DestructibleTracker.Instance.StartCoroutine(RespawnAfterDelay());
    }

    private void SpawnEnemyLogic()
    { 
        // Cách 2: Spawn trực tiếp prefab
        if (enemyPrefabs.Length > 0)
        {
            var pick = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            ObjectPooler.Instance.Get(
                pick.name,
                pick,
                transform.position,
                Quaternion.identity,
                initSize: 1,
                expandable: true
            );
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnTime);

        currentHits = 0;
        spriteRenderer.color = Color.white;
        transform.localPosition = originalPos;

        gameObject.SetActive(true);
        DestructibleTracker.Instance?.Register(this);
    }
}
