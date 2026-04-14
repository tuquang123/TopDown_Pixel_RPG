using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────
    //  Formation types
    // ──────────────────────────────────────────────────────────
    public enum SpawnFormation
    {
        Random,     // giống cũ – spawn rải rác xung quanh player
        Line,       // hàng ngang / dọc từ một phía
        Arc,        // cung bán nguyệt hướng vào player
        Circle,     // vòng tròn bao vây
        VShape,     // hình chữ V hướng vào player
        Burst,      // tất cả spawn đồng thời tứ phía
    }
// Thêm vào phần Public API (dưới CurrentWave)
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
    [Tooltip("Random = tự động chọn ngẫu nhiên mỗi wave")]
    [SerializeField] private SpawnFormation formation = SpawnFormation.Random;
    [Tooltip("Khoảng cách spawn tính từ player")]
    [SerializeField, Min(1f)] private float formationRadius = 8f;
    [Tooltip("Spacing giữa các quái trong Line / Arc / V-Shape")]
    [SerializeField, Min(0.5f)] private float formationSpacing = 1.4f;
    [Tooltip("Delay giữa từng con khi spawn Burst (0 = đồng thời)")]
    [SerializeField, Min(0f)] private float burstDelay = 0.05f;

    [Header("Spawn Area (dùng cho Random formation)")]
    [SerializeField, Min(1f)] private float spawnRadiusMin = 6f;
    [SerializeField, Min(1f)] private float spawnRadiusMax = 10f;

    [Header("UI")]
    [SerializeField] private WaveProgressUI waveUiPrefab;
    [SerializeField] private Transform waveUiRoot;

    // ──────────────────────────────────────────────────────────
    //  Events
    // ──────────────────────────────────────────────────────────
    public event Action<int, bool> OnWaveStarted;
    public event Action<int> OnWaveCleared;

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
        player        ??= PlayerController.Instance != null ? PlayerController.Instance.transform : null;
        enemyLevelDatabase ??= CommonReferent.Instance != null ? CommonReferent.Instance.enemyLevelDatabase : null;
        stageManager  ??= StageManager.Instance;

        SetupWaveUi();

        currentWave = Mathf.Max(1, startWave) - 1;
        StartNextWave();
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

        SpawnFormation chosenFormation = PickFormation();
        StartCoroutine(SpawnWaveRoutine(enemyCount, spawnInterval, chosenFormation));
    }

    /// <summary>Trả về formation sẽ dùng cho wave này.</summary>
    private SpawnFormation PickFormation()
    {
        if (formation != SpawnFormation.Random)
            return formation;

        // Ngẫu nhiên trong tất cả các loại (trừ Random chính nó)
        var values = (SpawnFormation[])Enum.GetValues(typeof(SpawnFormation));
        return values[UnityEngine.Random.Range(1, values.Length)]; // index 0 = Random
    }

    // ──────────────────────────────────────────────────────────
    //  Spawn coroutine – điều phối formation
    // ──────────────────────────────────────────────────────────
    private IEnumerator SpawnWaveRoutine(int enemyCount, float spawnInterval, SpawnFormation f)
    {
        Debug.Log($"[Wave {currentWave}] formation={f}, count={enemyCount}");

        switch (f)
        {
            case SpawnFormation.Burst:
                yield return StartCoroutine(SpawnBurst(enemyCount));
                break;

            case SpawnFormation.Circle:
                yield return StartCoroutine(SpawnFormationPositions(
                    BuildCirclePositions(enemyCount), spawnInterval));
                break;

            case SpawnFormation.Line:
                yield return StartCoroutine(SpawnFormationPositions(
                    BuildLinePositions(enemyCount), spawnInterval));
                break;

            case SpawnFormation.Arc:
                yield return StartCoroutine(SpawnFormationPositions(
                    BuildArcPositions(enemyCount), spawnInterval));
                break;

            case SpawnFormation.VShape:
                yield return StartCoroutine(SpawnFormationPositions(
                    BuildVShapePositions(enemyCount), spawnInterval));
                break;

            default: // SpawnFormation.Random fallback
                for (int i = 0; i < enemyCount; i++)
                {
                    SpawnEnemyAt(GetRandomSpawnPosition(), false);
                    yield return new WaitForSeconds(spawnInterval);
                }
                break;
        }
    }

    // ──────────────────────────────────────────────────────────
    //  Formation: spawn lần lượt từ danh sách vị trí
    // ──────────────────────────────────────────────────────────
    private IEnumerator SpawnFormationPositions(List<Vector3> positions, float interval)
    {
        foreach (Vector3 pos in positions)
        {
            SpawnEnemyAt(pos, false);
            yield return new WaitForSeconds(interval);
        }
    }

    // ──────────────────────────────────────────────────────────
    //  Formation: BURST – spawn tất cả cùng lúc (hay delay nhỏ)
    // ──────────────────────────────────────────────────────────
    private IEnumerator SpawnBurst(int enemyCount)
    {
        List<Vector3> positions = BuildCirclePositions(enemyCount);
        foreach (Vector3 pos in positions)
        {
            SpawnEnemyAt(pos, false);
            if (burstDelay > 0f)
                yield return new WaitForSeconds(burstDelay);
        }
        yield return null;
    }

    // ──────────────────────────────────────────────────────────
    //  Position builders
    // ──────────────────────────────────────────────────────────

    /// <summary>Vòng tròn đều xung quanh player.</summary>
    private List<Vector3> BuildCirclePositions(int count)
    {
        var list = new List<Vector3>(count);
        Vector3 center = PlayerPosition();
        float angleStep = 360f / count;
        float startAngle = UnityEngine.Random.Range(0f, 360f);

        for (int i = 0; i < count; i++)
        {
            float rad = (startAngle + i * angleStep) * Mathf.Deg2Rad;
            list.Add(center + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * formationRadius);
        }
        return list;
    }

    /// <summary>Hàng thẳng từ một hướng ngẫu nhiên, perpendicular với hướng vào player.</summary>
    private List<Vector3> BuildLinePositions(int count)
    {
        var list = new List<Vector3>(count);
        Vector3 center = PlayerPosition();

        // Hướng từ ngoài vào player
        float incomingAngle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 inDir = new Vector3(Mathf.Cos(incomingAngle), Mathf.Sin(incomingAngle), 0f);
        // Trục vuông góc (perpendicular) để xếp hàng
        Vector3 perpDir = new Vector3(-inDir.y, inDir.x, 0f);

        Vector3 origin = center - inDir * formationRadius;
        float halfWidth = (count - 1) * formationSpacing * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float offset = -halfWidth + i * formationSpacing;
            list.Add(origin + perpDir * offset);
        }
        return list;
    }

    /// <summary>Cung bán nguyệt (180°) hướng vào player.</summary>
    private List<Vector3> BuildArcPositions(int count)
    {
        var list = new List<Vector3>(count);
        Vector3 center = PlayerPosition();

        float facingAngle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        // Cung 180° đối diện với hướng facing
        float arcStart = facingAngle + 90f * Mathf.Deg2Rad;
        float arcEnd   = facingAngle + 270f * Mathf.Deg2Rad;

        for (int i = 0; i < count; i++)
        {
            float t   = count == 1 ? 0.5f : (float)i / (count - 1);
            float rad = Mathf.Lerp(arcStart, arcEnd, t);
            list.Add(center + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * formationRadius);
        }
        return list;
    }

    /// <summary>Hình chữ V, đầu nhọn hướng thẳng vào player.</summary>
    private List<Vector3> BuildVShapePositions(int count)
    {
        var list = new List<Vector3>(count);
        Vector3 center = PlayerPosition();

        float incomingAngle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 inDir  = new Vector3(Mathf.Cos(incomingAngle), Mathf.Sin(incomingAngle), 0f);
        Vector3 perpDir = new Vector3(-inDir.y, inDir.x, 0f);

        // Con đầu tiên là đầu nhọn của chữ V
        list.Add(center - inDir * formationRadius);

        float vSpread = formationSpacing;
        int remaining = count - 1;
        for (int i = 0; i < remaining; i++)
        {
            // Xen kẽ trái – phải, lùi dần ra sau
            int side   = (i % 2 == 0) ? 1 : -1;
            int rank   = (i / 2) + 1;
            float lateralOffset  = side * rank * vSpread;
            float backwardOffset = rank * vSpread;

            Vector3 pos = center
                - inDir   * (formationRadius + backwardOffset)
                + perpDir * lateralOffset;
            list.Add(pos);
        }
        return list;
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
        Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
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
            || CommonReferent.Instance.canvasHp  == null) return;

        GameObject uiObj = Instantiate(
            CommonReferent.Instance.hpSliderUi,
            CommonReferent.Instance.canvasHp.transform, false);

        EnemyHealthUI hpUi = uiObj.GetComponent<EnemyHealthUI>();
        if (hpUi == null) return;
        hpUi.SetTarget(enemyObj);
        enemyAI.EnemyHealthUI = hpUi;
    }

    // ──────────────────────────────────────────────────────────
    //  Public API (player death / restart)
    // ──────────────────────────────────────────────────────────
    public void OnPlayerDied()
    {
        StopAllCoroutines();
        waveActive = false;
    }
    public event Action OnWavesRestarted;
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