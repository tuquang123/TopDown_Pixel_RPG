using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct EnemySpawnEntry
{
    public GameObject prefab;
    [Min(1)]    public int   minStage;
    [Min(0.1f)] public float weight;
}

public class WaveManager : Singleton<WaveManager>
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Stage Database")]
    [SerializeField] private StageDataSO stageDatabase;

    [Header("Prefabs")]
    [SerializeField] private List<EnemySpawnEntry> enemySpawnEntries = new();
    [SerializeField] private GameObject bossPrefab;

    [Header("Wave")]
    [SerializeField, Min(1)]    private int   startWave                 = 1;
    [SerializeField, Min(1)]    private int   enemiesBaseCount          = 4;
    [SerializeField, Min(0)]    private int   enemiesPerWave            = 2;
    [SerializeField, Min(0f)]   private float baseSpawnInterval         = 0.8f;
    [SerializeField, Min(0f)]   private float spawnIntervalDecayPerWave = 0.02f;
    [SerializeField, Min(0.05f)]private float minSpawnInterval          = 0.15f;
    [SerializeField, Min(1)]    private int   bossWaveFrequency         = 5;

    [Header("Formation")]
    [SerializeField, Min(1f)]  private float formationRadius  = 8f;
    [SerializeField, Min(0.5f)]private float formationSpacing = 1.4f;

    [Header("Spawn Area")]
    [SerializeField, Min(1f)] private float spawnRadiusMin = 6f;
    [SerializeField, Min(1f)] private float spawnRadiusMax = 10f;

    [Header("Physics")]
    [SerializeField] private string enemyLayerName = "Enemy";

    [Header("UI")]
    [SerializeField] private WaveProgressUI waveUiPrefab;
    [SerializeField] private Transform      waveUiRoot;

    [Header("Enemy Base Stats")]
    [SerializeField] private int   baseHealth         = 50;
    [SerializeField] private int   baseDamage         = 8;
    [SerializeField] private float baseMoveSpeed      = 2f;
    [SerializeField] private float baseAttackRange    = 1.2f;
    [SerializeField] private float baseDetectRange    = 6f;
    [SerializeField] private float baseAttackCooldown = 1.5f;

    [Header("Wave Scaling")]
    [SerializeField, Min(0f)] private float waveHealthGrowth = 0.15f;
    [SerializeField, Min(0f)] private float waveDamageGrowth = 0.10f;
    [SerializeField, Min(0f)] private float waveSpeedGrowth  = 0.02f;

    [Header("Stage Scaling")]
    [SerializeField, Min(0f)]   private float healthScalePerStage       = 0.20f;
    [SerializeField, Min(0f)]   private float damageScalePerStage       = 0.15f;
    [SerializeField, Min(0f)]   private float moveSpeedScalePerStage    = 0.03f;
    [SerializeField, Min(0f)]   private float cooldownReductionPerStage = 0.01f;
    [SerializeField, Min(0.05f)]private float minAttackCooldown         = 0.2f;

    [Header("Boss Multiplier")]
    [SerializeField, Min(1f)] private float bossHealthMult = 5f;
    [SerializeField, Min(1f)] private float bossDamageMult = 2f;

    [Header("Level")]
    [SerializeField, Min(0)] private int bossLevelBonus = 2;

    [Header("Spawn Weight Scaling")]
    [SerializeField, Min(0f)] private float weightGrowthPerStage = 0.1f;

    // ── Events ──
    public event Action<int, int, bool> OnWaveStarted;
    public event Action<int>            OnWaveCleared;
    public event Action                 OnWavesRestarted;
    public event Action<int>            OnStageChanged;

    // ── State ──
    private readonly HashSet<EnemyAI>            aliveEnemies  = new();
    private readonly Dictionary<EnemyAI, Action> deathHandlers = new();

    private int  currentWave;
    private int  currentStage = 1;
    private bool waveActive;
    private WaveProgressUI waveUiInstance;
    private GameObject     currentMapInstance;

    public int CurrentWave       => currentWave;
    public int CurrentStage      => currentStage;
    public int BossWaveFrequency => bossWaveFrequency;
    public StageData CurrentStageData => stageDatabase?.GetOrLast(currentStage);

    // ── Unity lifecycle ──
    private void OnDisable()
    {
        foreach (var pair in deathHandlers)
            if (pair.Key != null)
                pair.Key.OnDeath -= pair.Value;

        deathHandlers.Clear();
        aliveEnemies.Clear();

        if (waveUiInstance != null)
            Destroy(waveUiInstance.gameObject);
    }

    private void Start()
    {
        player ??= PlayerController.Instance?.transform;

        SetupEnemyLayerCollision();
        SetupWaveUi();

        currentWave = Mathf.Max(1, startWave) - 1;
        ApplyStageData(currentStage);
        StartNextWave();
    }

    // ── Stage ──
    private void AdvanceStage()
    {
        currentStage++;
        ApplyStageData(currentStage);
        OnStageChanged?.Invoke(currentStage);
    }

    private void ApplyStageData(int stageNumber)
    {
        if (stageDatabase == null) return;

        StageData data = stageDatabase.GetOrLast(stageNumber);
        if (data == null) return;

        Debug.Log($"[WaveManager] Stage {stageNumber} → {data.stageName}");

        if (data.mapPrefab != null)
        {
            if (ScreenFader.Instance != null)
            {
                ScreenFader.Instance.FadeIn(0.4f, () =>
                {
                    SwapMap(data.mapPrefab);
                    ScreenFader.Instance.FadeOut(0.4f);
                });
            }
            else
            {
                SwapMap(data.mapPrefab);
            }
        }

        if (stageNumber > 1)
            GiveStageReward(data);
    }

    private void SwapMap(GameObject mapPrefab)
    {
        if (currentMapInstance != null)
            Destroy(currentMapInstance);

        currentMapInstance = Instantiate(mapPrefab);
    }

    private void GiveStageReward(StageData data)
    {
        if (data.bonusGold > 0)
            CurrencyManager.Instance?.AddGold(data.bonusGold);

        if (data.bonusExp > 0)
        {
            var playerLevel = PlayerStats.Instance?.GetComponent<PlayerLevel>();
            playerLevel?.levelSystem?.AddExp(data.bonusExp);
        }
    }

    // ── Compute stats ──
    private EnemyLevelData ComputeEnemyData(bool isBoss)
    {
        int waveIndex = isBoss
            ? Mathf.Max(0, Mathf.CeilToInt(currentWave / 2f) - 1)
            : currentWave - 1;

        float wf = Mathf.Pow(1f + waveHealthGrowth, waveIndex);
        float df = Mathf.Pow(1f + waveDamageGrowth, waveIndex);
        float sf = 1f + waveSpeedGrowth * waveIndex;

        int   s        = currentStage - 1;
        float stageHp  = 1f + s * healthScalePerStage;
        float stageDmg = 1f + s * damageScalePerStage;
        float stageSpd = 1f + s * moveSpeedScalePerStage;

        int   hp  = Mathf.Max(1, Mathf.RoundToInt(baseHealth * wf * stageHp));
        int   dmg = Mathf.Max(1, Mathf.RoundToInt(baseDamage * df * stageDmg));
        float spd = Mathf.Max(0.1f, baseMoveSpeed * sf * stageSpd);
        float cd  = Mathf.Max(minAttackCooldown,
                        baseAttackCooldown * Mathf.Pow(1f - cooldownReductionPerStage, s));

        if (isBoss)
        {
            hp  = Mathf.RoundToInt(hp  * bossHealthMult);
            dmg = Mathf.RoundToInt(dmg * bossDamageMult);
        }

        int level = (currentStage - 1) * bossWaveFrequency + currentWave;
        if (isBoss) level += bossLevelBonus;

        return new EnemyLevelData
        {
            level          = level,
            maxHealth      = hp,
            attackDamage   = dmg,
            moveSpeed      = spd,
            attackRange    = baseAttackRange,
            detectionRange = baseDetectRange,
            attackCooldown = cd
        };
    }

    // ── Wave flow ──
    private void StartNextWave()
    {
        currentWave++;
        bool isBossWave = currentWave % Mathf.Max(1, bossWaveFrequency) == 0;
        waveActive = true;

        OnWaveStarted?.Invoke(currentWave, currentStage, isBossWave);

        if (isBossWave) { SpawnBoss(); return; }

        int   count    = enemiesBaseCount + (currentWave - 1) * enemiesPerWave;
        float interval = Mathf.Max(minSpawnInterval,
            baseSpawnInterval - (currentWave - 1) * spawnIntervalDecayPerWave);

        StartCoroutine(SpawnWaveRoutine(count, interval));
    }

    private IEnumerator SpawnWaveRoutine(int enemyCount, float spawnInterval)
    {
        yield return StartCoroutine(SpawnDiamond(enemyCount));
    }

    // ── Spawn ──
    private IEnumerator SpawnDiamond(int enemyCount)
    {
        Vector3 center = PlayerPosition();
        Vector3[] apexes =
        {
            center + new Vector3( 0,               formationRadius, 0),
            center + new Vector3( 0,              -formationRadius, 0),
            center + new Vector3(-formationRadius,  0,              0),
            center + new Vector3( formationRadius,  0,              0),
        };

        int groupSize = Mathf.Max(2, Mathf.CeilToInt((float)enemyCount / 4));

        for (int g = 0; g < 4; g++)
        {
            Vector3 outDir = (apexes[g] - center).normalized;
            for (int i = 0; i < groupSize; i++)
            {
                SpawnEnemyAt(apexes[g] + outDir * (i * formationSpacing), false);
                yield return new WaitForSeconds(0.08f);
            }
            yield return new WaitForSeconds(0.15f);
        }
    }

    private void SpawnBoss() => SpawnEnemyAt(GetRandomSpawnPosition(), true);

    private void SpawnEnemyAt(Vector3 pos, bool isBoss)
    {
        GameObject prefab = isBoss ? bossPrefab : GetRandomEnemyPrefab();
        if (prefab == null) return;

        GameObject obj = ObjectPooler.Instance != null
            ? ObjectPooler.Instance.Get(prefab.name, prefab, pos, Quaternion.identity, initSize: 8, expandable: true)
            : Instantiate(prefab, pos, Quaternion.identity);

        if (obj == null || !obj.TryGetComponent(out EnemyAI ai)) return;

        SetupEnemy(ai, isBoss);
        RegisterAliveEnemy(ai);
    }

    // ── Enemy setup & death ──
    private void SetupEnemy(EnemyAI ai, bool isBoss)
    {
        var data = ComputeEnemyData(isBoss);
        ai.ApplyLevelData(data);
        ai.isBoss = isBoss;
        ai.ResetEnemy();
        EnsureHealthUI(ai.gameObject, ai);
    }

    private void RegisterAliveEnemy(EnemyAI ai)
    {
        if (ai == null) return;
        if (deathHandlers.TryGetValue(ai, out var old)) ai.OnDeath -= old;

        Action handler = () => HandleEnemyDeath(ai);
        deathHandlers[ai] = handler;
        aliveEnemies.Add(ai);
        ai.OnDeath += handler;
    }

    private void HandleEnemyDeath(EnemyAI dead)
    {
        if (dead == null) return;

        if (deathHandlers.TryGetValue(dead, out var handler))
        {
            dead.OnDeath -= handler;
            deathHandlers.Remove(dead);
        }
        aliveEnemies.Remove(dead);

        if (aliveEnemies.Count > 0 || !waveActive) return;

        waveActive = false;
        OnWaveCleared?.Invoke(currentWave);

        if (dead.isBoss) AdvanceStage();

        StartNextWave();
    }

    // ── Helpers ──
    private Vector3 PlayerPosition() => player != null ? player.position : Vector3.zero;

    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
        if (dir == Vector2.zero) dir = Vector2.right;
        float dist = UnityEngine.Random.Range(spawnRadiusMin, Mathf.Max(spawnRadiusMin, spawnRadiusMax));
        return PlayerPosition() + new Vector3(dir.x, dir.y) * dist;
    }

    private GameObject GetRandomEnemyPrefab()
    {
        var available = enemySpawnEntries.FindAll(e => e.prefab != null && currentStage >= e.minStage);

        if (available.Count == 0)
        {
            available = enemySpawnEntries.FindAll(e => e.prefab != null);
            if (available.Count == 0) return null;
        }

        float totalWeight = 0f;
        var effectiveWeights = new float[available.Count];

        for (int i = 0; i < available.Count; i++)
        {
            int stagesUnlocked = Mathf.Max(0, currentStage - available[i].minStage);
            effectiveWeights[i] = Mathf.Max(0.01f, available[i].weight)
                                  * (1f + weightGrowthPerStage * stagesUnlocked);
            totalWeight += effectiveWeights[i];
        }

        float roll       = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < available.Count; i++)
        {
            cumulative += effectiveWeights[i];
            if (roll <= cumulative) return available[i].prefab;
        }

        return available[available.Count - 1].prefab;
    }

    private void EnsureHealthUI(GameObject obj, EnemyAI ai)
    {
        if (ai.EnemyHealthUI != null) return;
        var cr = CommonReferent.Instance;
        if (cr == null || cr.hpSliderUi == null || cr.canvasHp == null) return;

        var uiObj = Instantiate(cr.hpSliderUi, cr.canvasHp.transform, false);
        if (!uiObj.TryGetComponent(out EnemyHealthUI hpUi)) return;
        hpUi.SetTarget(obj);
        ai.EnemyHealthUI = hpUi;
    }

    private void SetupEnemyLayerCollision()
    {
        int layer = LayerMask.NameToLayer(enemyLayerName);
        if (layer == -1)
        {
            Debug.LogWarning($"[WaveManager] Layer '{enemyLayerName}' không tồn tại.");
            return;
        }
        Physics2D.IgnoreLayerCollision(layer, layer, true);
    }

    private void SetupWaveUi()
    {
        if (waveUiPrefab == null) return;
        Transform root = waveUiRoot ?? (FindFirstObjectByType<Canvas>()?.transform ?? transform);
        waveUiInstance = Instantiate(waveUiPrefab, root, false);
        waveUiInstance.Bind(this);
    }

    // ── Public API ──
    public void OnPlayerDied()
    {
        StopAllCoroutines();
        waveActive = false;
    }

    public void RestartWaves()
    {
        currentWave  = Mathf.Max(1, startWave) - 1;
        currentStage = 1;
        waveActive   = false;
        aliveEnemies.Clear();
        deathHandlers.Clear();

        if (currentMapInstance != null)
            Destroy(currentMapInstance);

        ApplyStageData(currentStage);
        OnWavesRestarted?.Invoke();
        StartNextWave();
    }
}