using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
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

    [Header("Spawn Area")]
    [SerializeField, Min(1f)] private float spawnRadiusMin = 6f;
    [SerializeField, Min(1f)] private float spawnRadiusMax = 10f;

    [Header("UI")]
    [SerializeField] private WaveProgressUI waveUiPrefab;
    [SerializeField] private Transform waveUiRoot;

    public event Action<int, bool> OnWaveStarted;
    public event Action<int> OnWaveCleared;

    private readonly HashSet<EnemyAI> aliveEnemies = new();
    private readonly Dictionary<EnemyAI, Action> deathHandlers = new();

    private int currentWave;
    private bool waveActive;
    private WaveProgressUI waveUiInstance;

    public int CurrentWave => currentWave;


    private void OnDisable()
    {
        foreach (var pair in deathHandlers)
        {
            if (pair.Key != null)
                pair.Key.OnDeath -= pair.Value;
        }

        deathHandlers.Clear();
        aliveEnemies.Clear();

        if (waveUiInstance != null)
            Destroy(waveUiInstance.gameObject);
    }
    private void Start()
    {
        player ??= PlayerController.Instance != null ? PlayerController.Instance.transform : null;
        enemyLevelDatabase ??= CommonReferent.Instance != null ? CommonReferent.Instance.enemyLevelDatabase : null;
        stageManager ??= StageManager.Instance;

        SetupWaveUi();

        currentWave = Mathf.Max(1, startWave) - 1;
        StartNextWave();
    }

    private void SetupWaveUi()
    {
        if (waveUiPrefab == null)
            return;

        Transform uiParent = ResolveUiRoot();
        waveUiInstance = Instantiate(waveUiPrefab, uiParent, false);
        waveUiInstance.Bind(this, stageManager);
    }

    private Transform ResolveUiRoot()
    {
        if (waveUiRoot != null)
            return waveUiRoot;

        Canvas foundCanvas = FindFirstObjectByType<Canvas>();
        return foundCanvas != null ? foundCanvas.transform : transform;
    }

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

        int enemyCount = enemiesBaseCount + (currentWave - 1) * enemiesPerWave;
        float spawnInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval - (currentWave - 1) * spawnIntervalDecayPerWave);
        StartCoroutine(SpawnWaveRoutine(enemyCount, spawnInterval));
    }
    

    private void SpawnBoss()
    {
        SpawnEnemy(true);
    }

    private void SpawnEnemy(bool isBoss)
    {
        GameObject prefab = isBoss ? bossPrefab : GetRandomEnemyPrefab();
        if (prefab == null)
            return;

        Vector3 spawnPos = GetSpawnPositionAroundPlayer();
        GameObject enemyObj = ObjectPooler.Instance != null
            ? ObjectPooler.Instance.Get(prefab.name, prefab, spawnPos, Quaternion.identity, initSize: 8, expandable: true)
            : Instantiate(prefab, spawnPos, Quaternion.identity);

        if (enemyObj == null || !enemyObj.TryGetComponent(out EnemyAI enemyAI))
            return;

        SetupEnemy(enemyAI, isBoss);
        RegisterAliveEnemy(enemyAI);
    }

    private void SetupEnemy(EnemyAI enemyAI, bool isBoss)
    {
        EnemyLevelData baseData = ResolveBaseLevelData(isBoss);
        EnemyLevelData scaledData = stageManager != null ? stageManager.GetScaledData(baseData) : baseData;

        if (scaledData != null)
            enemyAI.ApplyLevelData(scaledData);

        enemyAI.isBoss = isBoss;
        enemyAI.ResetEnemy();

        EnsureHealthUI(enemyAI.gameObject, enemyAI);
    }

    private EnemyLevelData ResolveBaseLevelData(bool isBoss)
    {
        if (enemyLevelDatabase == null || enemyLevelDatabase.levels == null || enemyLevelDatabase.levels.Count == 0)
            return null;

        int targetLevel = isBoss
            ? Mathf.Clamp(Mathf.CeilToInt(currentWave / 2f), 1, enemyLevelDatabase.levels.Count)
            : Mathf.Clamp(currentWave, 1, enemyLevelDatabase.levels.Count);

        EnemyLevelData exact = enemyLevelDatabase.GetDataByLevel(targetLevel);
        return exact ?? enemyLevelDatabase.levels[enemyLevelDatabase.levels.Count - 1];
    }

    private void RegisterAliveEnemy(EnemyAI enemyAI)
    {
        if (enemyAI == null)
            return;

        if (deathHandlers.TryGetValue(enemyAI, out Action oldHandler))
            enemyAI.OnDeath -= oldHandler;

        Action handler = () => HandleEnemyDeath(enemyAI);
        deathHandlers[enemyAI] = handler;

        aliveEnemies.Add(enemyAI);
        enemyAI.OnDeath += handler;
    }

    private void HandleEnemyDeath(EnemyAI deadEnemy)
    {
        if (deadEnemy == null)
            return;

        if (deathHandlers.TryGetValue(deadEnemy, out Action handler))
        {
            deadEnemy.OnDeath -= handler;
            deathHandlers.Remove(deadEnemy);
        }

        aliveEnemies.Remove(deadEnemy);

        if (aliveEnemies.Count > 0 || !waveActive)
            return;

        waveActive = false;
        OnWaveCleared?.Invoke(currentWave);

        if (deadEnemy.isBoss)
            stageManager?.AdvanceStage();

        StartNextWave();
    }

    private GameObject GetRandomEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
            return null;

        return enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Count)];
    }

    private Vector3 GetSpawnPositionAroundPlayer()
    {
        Vector3 center = player != null ? player.position : Vector3.zero;
        Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
        if (dir == Vector2.zero)
            dir = Vector2.right;

        float distance = UnityEngine.Random.Range(spawnRadiusMin, Mathf.Max(spawnRadiusMin, spawnRadiusMax));
        return center + new Vector3(dir.x, dir.y, 0f) * distance;
    }

    private void EnsureHealthUI(GameObject enemyObj, EnemyAI enemyAI)
    {
        if (enemyAI.EnemyHealthUI != null)
            return;

        if (CommonReferent.Instance == null || CommonReferent.Instance.hpSliderUi == null || CommonReferent.Instance.canvasHp == null)
            return;

        GameObject uiObj = Instantiate(CommonReferent.Instance.hpSliderUi, CommonReferent.Instance.canvasHp.transform, false);
        EnemyHealthUI hpUi = uiObj.GetComponent<EnemyHealthUI>();
        if (hpUi == null)
            return;

        hpUi.SetTarget(enemyObj);
        enemyAI.EnemyHealthUI = hpUi;
    }
    // Gọi method này từ PlayerController khi player chết
    public void OnPlayerDied()
    {
        // Dừng tất cả coroutine đang spawn
        StopAllCoroutines();
        waveActive = false;
    }

// Khi player respawn hoặc game restart
    public void RestartWaves()
    {
        currentWave = Mathf.Max(1, startWave) - 1;
        waveActive = false;
        aliveEnemies.Clear();
        deathHandlers.Clear();
        StartNextWave();
    }
    private IEnumerator SpawnWaveRoutine(int enemyCount, float spawnInterval)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Debug.Log($"[Wave] Spawning enemy {i+1}/{enemyCount}, timeScale={Time.timeScale}");
            SpawnEnemy(false);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

   
}
