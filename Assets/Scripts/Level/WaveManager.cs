using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public int BossWaveFrequency => bossWaveFrequency;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private EnemyLevelDatabase enemyLevelDatabase;
    [SerializeField] private StageManager stageManager;

    [Header("Prefabs")]
    [SerializeField] private List<GameObject> enemyPrefabs = new();
    [SerializeField] private GameObject bossPrefab;

    [Header("Wave")]
    [SerializeField, Min(1)] private int startWave = 1;
    [SerializeField, Min(1)] private int enemiesBaseCount = 4;
    [SerializeField, Min(0)] private int enemiesPerWave = 2;
    [SerializeField, Min(0f)] private float baseSpawnInterval = 0.8f;
    [SerializeField, Min(0f)] private float spawnIntervalDecayPerWave = 0.02f;
    [SerializeField, Min(0.05f)] private float minSpawnInterval = 0.15f;
    [SerializeField, Min(1)] private int bossWaveFrequency = 5;

    [Header("Formation")]
    [Tooltip("Khoảng cách spawn tính từ player")]
    [SerializeField, Min(1f)] private float formationRadius = 8f;
    [Tooltip("Spacing giữa các quái trong Diamond")]
    [SerializeField, Min(0.5f)] private float formationSpacing = 1.4f;

    [Header("Spawn Area")]
    [SerializeField, Min(1f)] private float spawnRadiusMin = 6f;
    [SerializeField, Min(1f)] private float spawnRadiusMax = 10f;

    [Header("Physics")]
    [Tooltip("Tên layer của enemy — dùng để tắt va chạm enemy vs enemy")]
    [SerializeField] private string enemyLayerName = "Enemy";

    [Header("UI")]
    [SerializeField] private WaveProgressUI waveUiPrefab;
    [SerializeField] private Transform waveUiRoot;

    // ──────────────────────────────────────────────────────────
    //  Events
    // ──────────────────────────────────────────────────────────
    public event Action<int, bool> OnWaveStarted;
    public event Action<int> OnWaveCleared;
    public event Action OnWavesRestarted;

    // ──────────────────────────────────────────────────────────
    //  Private state
    // ──────────────────────────────────────────────────────────
    private readonly HashSet<EnemyAI> aliveEnemies = new();
    private readonly Dictionary<EnemyAI, Action> deathHandlers = new();

    private int currentWave;
    private bool waveActive;
    private WaveProgressUI waveUiInstance;

    public int CurrentWave => currentWave;

    // ──────────────────────────────────────────────────────────
    //  Unity lifecycle
    // ──────────────────────────────────────────────────────────
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
        player             ??= PlayerController.Instance != null ? PlayerController.Instance.transform : null;
        enemyLevelDatabase ??= CommonReferent.Instance   != null ? CommonReferent.Instance.enemyLevelDatabase : null;
        stageManager       ??= StageManager.Instance;

        SetupEnemyLayerCollision();
        SetupWaveUi();

        currentWave = Mathf.Max(1, startWave) - 1;
        StartNextWave();
    }

    // ──────────────────────────────────────────────────────────
    //  Physics setup
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Tắt va chạm vật lý giữa enemy với nhau.
    /// Separation steering trong EnemyAI vẫn chạy để dàn hàng đẹp,
    /// nhưng collider không chặn đường đi của nhau nữa.
    /// </summary>
    private void SetupEnemyLayerCollision()
    {
        int enemyLayer = LayerMask.NameToLayer(enemyLayerName);

        if (enemyLayer == -1)
        {
            Debug.LogWarning($"[WaveManager] Không tìm thấy layer '{enemyLayerName}'. " +
                             "Vào Edit → Project Settings → Tags and Layers để tạo layer này, " +
                             "sau đó gán cho tất cả enemy prefab.");
            return;
        }

        Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
    }

    // ──────────────────────────────────────────────────────────
    //  UI helpers
    // ──────────────────────────────────────────────────────────
    private void SetupWaveUi()
    {
        if (waveUiPrefab == null) return;
        Transform uiParent = ResolveUiRoot();
        waveUiInstance = Instantiate(waveUiPrefab, uiParent, false);
        waveUiInstance.Bind(this, stageManager);
    }

    private Transform ResolveUiRoot()
    {
        if (waveUiRoot != null) return waveUiRoot;
        Canvas found = FindFirstObjectByType<Canvas>();
        return found != null ? found.transform : transform;
    }

    // ──────────────────────────────────────────────────────────
    //  Wave flow
    // ──────────────────────────────────────────────────────────
    private void StartNextWave()
    {
        currentWave++;
        bool isBossWave = currentWave % Mathf.Max(1, bossWaveFrequency) == 0;
        waveActive = true;

        OnWaveStarted?.Invoke(currentWave, isBossWave);

        if (isBossWave)
        {
            SpawnBoss();
            return;
        }

        int   enemyCount    = enemiesBaseCount + (currentWave - 1) * enemiesPerWave;
        float spawnInterval = Mathf.Max(minSpawnInterval,
            baseSpawnInterval - (currentWave - 1) * spawnIntervalDecayPerWave);

        StartCoroutine(SpawnWaveRoutine(enemyCount, spawnInterval));
    }

    // ──────────────────────────────────────────────────────────
    //  Spawn coroutine
    // ──────────────────────────────────────────────────────────
    private IEnumerator SpawnWaveRoutine(int enemyCount, float spawnInterval)
    {
        Debug.Log($"[Wave {currentWave}] formation=Diamond, count={enemyCount}");
        yield return StartCoroutine(SpawnDiamond(enemyCount));
    }

    // ──────────────────────────────────────────────────────────
    //  Diamond formation
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// 4 điểm hình thoi, mỗi điểm spawn ít nhất 2 con xếp hàng
    /// theo hướng ra ngoài, không chồng lên nhau.
    /// </summary>
    private IEnumerator SpawnDiamond(int enemyCount)
    {
        Vector3 center = PlayerPosition();

        Vector3[] apexes =
        {
            center + new Vector3( 0,               formationRadius,  0),
            center + new Vector3( 0,              -formationRadius,  0),
            center + new Vector3(-formationRadius,  0,               0),
            center + new Vector3( formationRadius,  0,               0),
        };

        int groupSize = Mathf.Max(2, Mathf.CeilToInt((float)enemyCount / 4));

        for (int g = 0; g < 4; g++)
        {
            Vector3 apex   = apexes[g];
            Vector3 outDir = (apex - center).normalized;

            for (int i = 0; i < groupSize; i++)
            {
                Vector3 pos = apex + outDir * (i * formationSpacing);
                SpawnEnemyAt(pos, false);
                yield return new WaitForSeconds(0.08f);
            }

            yield return new WaitForSeconds(0.15f);
        }
    }

    // ──────────────────────────────────────────────────────────
    //  Boss
    // ──────────────────────────────────────────────────────────
    private void SpawnBoss() => SpawnEnemyAt(GetRandomSpawnPosition(), true);

    // ──────────────────────────────────────────────────────────
    //  Core spawn
    // ──────────────────────────────────────────────────────────
    private void SpawnEnemyAt(Vector3 spawnPos, bool isBoss)
    {
        GameObject prefab = isBoss ? bossPrefab : GetRandomEnemyPrefab();
        if (prefab == null) return;

        GameObject enemyObj = ObjectPooler.Instance != null
            ? ObjectPooler.Instance.Get(prefab.name, prefab, spawnPos, Quaternion.identity,
                initSize: 8, expandable: true)
            : Instantiate(prefab, spawnPos, Quaternion.identity);

        if (enemyObj == null || !enemyObj.TryGetComponent(out EnemyAI enemyAI)) return;

        SetupEnemy(enemyAI, isBoss);
        RegisterAliveEnemy(enemyAI);
    }

    // ──────────────────────────────────────────────────────────
    //  Enemy setup & lifecycle
    // ──────────────────────────────────────────────────────────
    private void SetupEnemy(EnemyAI enemyAI, bool isBoss)
    {
        EnemyLevelData baseData   = ResolveBaseLevelData(isBoss);
        EnemyLevelData scaledData = stageManager != null ? stageManager.GetScaledData(baseData) : baseData;
        if (scaledData != null) enemyAI.ApplyLevelData(scaledData);

        enemyAI.isBoss = isBoss;
        enemyAI.ResetEnemy();
        EnsureHealthUI(enemyAI.gameObject, enemyAI);
    }

    private EnemyLevelData ResolveBaseLevelData(bool isBoss)
    {
        if (enemyLevelDatabase == null
            || enemyLevelDatabase.levels == null
            || enemyLevelDatabase.levels.Count == 0) return null;

        int targetLevel = isBoss
            ? Mathf.Clamp(Mathf.CeilToInt(currentWave / 2f), 1, enemyLevelDatabase.levels.Count)
            : Mathf.Clamp(currentWave, 1, enemyLevelDatabase.levels.Count);

        return enemyLevelDatabase.GetDataByLevel(targetLevel)
               ?? enemyLevelDatabase.levels[^1];
    }

    private void RegisterAliveEnemy(EnemyAI enemyAI)
    {
        if (enemyAI == null) return;
        if (deathHandlers.TryGetValue(enemyAI, out Action oldHandler))
            enemyAI.OnDeath -= oldHandler;

        Action handler = () => HandleEnemyDeath(enemyAI);
        deathHandlers[enemyAI] = handler;
        aliveEnemies.Add(enemyAI);
        enemyAI.OnDeath += handler;
    }

    private void HandleEnemyDeath(EnemyAI deadEnemy)
    {
        if (deadEnemy == null) return;

        if (deathHandlers.TryGetValue(deadEnemy, out Action handler))
        {
            deadEnemy.OnDeath -= handler;
            deathHandlers.Remove(deadEnemy);
        }
        aliveEnemies.Remove(deadEnemy);

        if (aliveEnemies.Count > 0 || !waveActive) return;

        waveActive = false;
        OnWaveCleared?.Invoke(currentWave);

        if (deadEnemy.isBoss) stageManager?.AdvanceStage();

        StartNextWave();
    }

    // ──────────────────────────────────────────────────────────
    //  Utility helpers
    // ──────────────────────────────────────────────────────────
    private Vector3 PlayerPosition()
        => player != null ? player.position : Vector3.zero;

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 center = PlayerPosition();
        Vector2 dir    = UnityEngine.Random.insideUnitCircle.normalized;
        if (dir == Vector2.zero) dir = Vector2.right;
        float distance = UnityEngine.Random.Range(spawnRadiusMin, Mathf.Max(spawnRadiusMin, spawnRadiusMax));
        return center + new Vector3(dir.x, dir.y, 0f) * distance;
    }

    private GameObject GetRandomEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0) return null;
        return enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Count)];
    }

    private void EnsureHealthUI(GameObject enemyObj, EnemyAI enemyAI)
    {
        if (enemyAI.EnemyHealthUI != null) return;
        if (CommonReferent.Instance == null
            || CommonReferent.Instance.hpSliderUi == null
            || CommonReferent.Instance.canvasHp   == null) return;

        GameObject uiObj = Instantiate(
            CommonReferent.Instance.hpSliderUi,
            CommonReferent.Instance.canvasHp.transform, false);

        EnemyHealthUI hpUi = uiObj.GetComponent<EnemyHealthUI>();
        if (hpUi == null) return;
        hpUi.SetTarget(enemyObj);
        enemyAI.EnemyHealthUI = hpUi;
    }

    // ──────────────────────────────────────────────────────────
    //  Public API
    // ──────────────────────────────────────────────────────────
    public void OnPlayerDied()
    {
        StopAllCoroutines();
        waveActive = false;
    }

    public void RestartWaves()
    {
        currentWave = Mathf.Max(1, startWave) - 1;
        waveActive  = false;
        aliveEnemies.Clear();
        deathHandlers.Clear();
        OnWavesRestarted?.Invoke();
        StartNextWave();
    }
}