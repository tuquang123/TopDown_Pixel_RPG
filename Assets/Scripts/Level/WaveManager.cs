using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public struct EnemySpawnEntry
{
    public GameObject prefab;
    [Min(1)]    public int   minStage;
    [Min(0.1f)] public float weight;
}

/// <summary>
/// WaveManager — spawn quái theo phong cách Soul Knight:
/// Chỉ spawn trên các ô được vẽ trong Tilemap tên "SpawnZone",
/// không bao giờ ra ngoài tường hay ngoài map.
///
/// HƯỚNG DẪN MAP:
///   Mỗi map prefab cần có đúng 2 Tilemap con:
///     1. "Ground"    — vẽ nền, chỉ để hiển thị
///     2. "SpawnZone" — vẽ những ô TRONG NỘI THẤT mà quái được spawn
///        (đặt Alpha = 0 trên Tile hoặc ẩn TilemapRenderer để invisible ingame)
///
///   Mỗi StageData PHẢI có mapPrefab — khi stage đổi, map cũ bị Destroy
///   và map mới được Instantiate thay thế.
/// </summary>
public class WaveManager : Singleton<WaveManager>
{
    // ═══════════════════════════════════════════════════════════════
    //  INSPECTOR
    // ═══════════════════════════════════════════════════════════════

    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Stage Database")]
    [SerializeField] private StageDataSO stageDatabase;

    [Header("SpawnZone Tilemap Name")]
    [Tooltip("Tên chính xác của Tilemap con dùng làm vùng spawn trong mỗi map prefab.\n" +
             "Mặc định: 'SpawnZone' — đổi nếu bạn đặt tên khác trong Hierarchy.")]
    [SerializeField] private string spawnZoneTilemapName = "SpawnZone";

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

    [Header("Spawn Distance")]
    [Tooltip("Quái không spawn trong bán kính này quanh player (tính theo tile).")]
    [SerializeField, Min(0f)] private float minSpawnDistanceFromPlayer = 4f;

    [Header("Out-of-Bounds Teleport")]
    [Tooltip("Tần suất kiểm tra quái ra ngoài map (giây).")]
    [SerializeField, Min(0.1f)] private float oobCheckInterval = 0.5f;

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

    [Header("Debug (Editor only)")]
    [SerializeField] private bool showSpawnGizmos;

    // ═══════════════════════════════════════════════════════════════
    //  PRIVATE
    // ═══════════════════════════════════════════════════════════════

    private const string KEY_WAVE  = "wave_current";
    private const string KEY_STAGE = "wave_stage";

    public event Action<int, int, bool> OnWaveStarted;
    public event Action<int>            OnWaveCleared;
    public event Action                 OnWavesRestarted;
    public event Action<int>            OnStageChanged;

    private readonly HashSet<EnemyAI>            aliveEnemies  = new();
    private readonly Dictionary<EnemyAI, Action> deathHandlers = new();

    private int            currentWave;
    private int            currentStage = 1;
    private bool           waveActive;
    private WaveProgressUI waveUiInstance;

    // Map hiện tại — Destroy khi stage đổi, Instantiate map mới thay thế
    private GameObject currentMapInstance;
    private GameObject currentMapPrefab;

    // ── SPAWN TILE CACHE ─────────────────────────────────────────
    private readonly List<Vector3> spawnTiles = new();

    // ── SpawnZone Tilemap reference của map hiện tại ─────────────
    private Tilemap activeSpawnZoneTilemap;

    public int CurrentWave       => currentWave;
    public int CurrentStage      => currentStage;
    public int BossWaveFrequency => bossWaveFrequency;
    public StageData CurrentStageData => stageDatabase?.GetOrLast(currentStage);

    // ═══════════════════════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════

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

    private void OnApplicationQuit()                    => SaveProgress();
    private void OnApplicationPause(bool pause) { if (pause) SaveProgress(); }

    private void Start()
    {
        player ??= PlayerController.Instance?.transform;
        SetupEnemyLayerCollision();
        SetupWaveUi();
        LoadProgress();
        ApplyStageData(currentStage);
        StartNextWave();
    }

    // ═══════════════════════════════════════════════════════════════
    //  SAVE / LOAD
    // ═══════════════════════════════════════════════════════════════

    private void SaveProgress()
    {
        PlayerPrefs.SetInt(KEY_WAVE,  currentWave);
        PlayerPrefs.SetInt(KEY_STAGE, currentStage);
        PlayerPrefs.Save();
        Debug.Log($"[WaveManager] Saved -> Stage {currentStage}, Wave {currentWave}");
    }

    private void LoadProgress()
    {
        currentWave  = PlayerPrefs.GetInt(KEY_WAVE,  Mathf.Max(1, startWave) - 1);
        currentStage = Mathf.Max(1, PlayerPrefs.GetInt(KEY_STAGE, 1));
        Debug.Log($"[WaveManager] Loaded -> Stage {currentStage}, Wave {currentWave}");
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(KEY_WAVE);
        PlayerPrefs.DeleteKey(KEY_STAGE);
        PlayerPrefs.Save();
    }

    // ═══════════════════════════════════════════════════════════════
    //  STAGE
    // ═══════════════════════════════════════════════════════════════

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

        Debug.Log($"[WaveManager] Stage {stageNumber} -> {data.stageName}");

        if (data.mapPrefab != null && data.mapPrefab != currentMapPrefab)
        {
            if (ScreenFader.Instance != null)
                ScreenFader.Instance.FadeIn(0.4f, () =>
                {
                    SwapMap(data.mapPrefab);
                    ScreenFader.Instance.FadeOut(0.4f);
                });
            else
                SwapMap(data.mapPrefab);
        }
        else if (data.mapPrefab == null)
        {
            Debug.LogWarning($"[WaveManager] Stage {stageNumber} không có mapPrefab — spawnTiles có thể rỗng.");
        }

        if (stageNumber > 1)
            GiveStageReward(data);
    }

    private void SwapMap(GameObject newPrefab)
    {
        if (currentMapInstance != null)
            Destroy(currentMapInstance);

        currentMapInstance = Instantiate(newPrefab);
        currentMapPrefab   = newPrefab;
        BuildSpawnCache(currentMapInstance);
    }

    // ═══════════════════════════════════════════════════════════════
    //  SPAWN TILE CACHE
    // ═══════════════════════════════════════════════════════════════

    private void BuildSpawnCache(GameObject mapRoot)
    {
        spawnTiles.Clear();
        activeSpawnZoneTilemap = null;

        if (mapRoot == null) return;

        Tilemap target = null;
        foreach (var tm in mapRoot.GetComponentsInChildren<Tilemap>())
        {
            if (tm.name == spawnZoneTilemapName)
            {
                target = tm;
                break;
            }
        }

        if (target == null)
        {
            Debug.LogWarning($"[WaveManager] Không tìm thấy Tilemap tên \"{spawnZoneTilemapName}\" " +
                             $"trong \"{mapRoot.name}\". Dùng tất cả Tilemap làm fallback.");

            foreach (var tm in mapRoot.GetComponentsInChildren<Tilemap>())
                CacheTilemap(tm);
        }
        else
        {
            activeSpawnZoneTilemap = target;
            CacheTilemap(target);
        }

        Debug.Log($"[WaveManager] SpawnCache: {spawnTiles.Count} ô hợp lệ từ \"{mapRoot.name}\".");
    }

    private void CacheTilemap(Tilemap tm)
    {
        tm.CompressBounds();
        foreach (var cell in tm.cellBounds.allPositionsWithin)
        {
            if (!tm.HasTile(cell)) continue;
            Vector3 world = tm.GetCellCenterWorld(cell);
            world.z = 0f;
            spawnTiles.Add(world);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  STAGE REWARD
    // ═══════════════════════════════════════════════════════════════

    private void GiveStageReward(StageData data)
    {
        if (data.bonusGold > 0)
            CurrencyManager.Instance?.AddGold(data.bonusGold);

        if (data.bonusExp > 0)
        {
            var lvl = PlayerStats.Instance?.GetComponent<PlayerLevel>();
            lvl?.levelSystem?.AddExp(data.bonusExp);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ENEMY STATS
    // ═══════════════════════════════════════════════════════════════

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

        if (isBoss) { hp = Mathf.RoundToInt(hp * bossHealthMult); dmg = Mathf.RoundToInt(dmg * bossDamageMult); }

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

    // ═══════════════════════════════════════════════════════════════
    //  WAVE FLOW
    // ═══════════════════════════════════════════════════════════════

    private void StartNextWave()
    {
        currentWave++;
        bool isBossWave = currentWave % Mathf.Max(1, bossWaveFrequency) == 0;
        waveActive = true;

        SaveProgress();
        OnWaveStarted?.Invoke(currentWave, currentStage, isBossWave);

        StartCoroutine(OutOfBoundsChecker());

        if (isBossWave) { SpawnBoss(); return; }

        int   count    = enemiesBaseCount + (currentWave - 1) * enemiesPerWave;
        float interval = Mathf.Max(minSpawnInterval,
            baseSpawnInterval - (currentWave - 1) * spawnIntervalDecayPerWave);

        StartCoroutine(SpawnWaveRoutine(count, interval));
    }

    private IEnumerator SpawnWaveRoutine(int count, float interval)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnEnemyAt(PickSpawnTile(), false);
            yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnBoss() => SpawnEnemyAt(PickSpawnTile(), true);

    // ═══════════════════════════════════════════════════════════════
    //  PICK SPAWN TILE
    // ═══════════════════════════════════════════════════════════════

    private Vector3 PickSpawnTile()
    {
        if (spawnTiles.Count == 0)
        {
            Debug.LogWarning("[WaveManager] SpawnTiles rỗng! Spawn tại vị trí player.");
            return PlayerPosition();
        }

        Vector3 playerPos = PlayerPosition();
        float   minDist   = minSpawnDistanceFromPlayer;

        int attempts = Mathf.Min(60, spawnTiles.Count);
        for (int i = 0; i < attempts; i++)
        {
            int idx = UnityEngine.Random.Range(0, spawnTiles.Count);
            if (Vector2.Distance(spawnTiles[idx], playerPos) >= minDist)
                return spawnTiles[idx];
        }

        Vector3 best    = spawnTiles[0];
        float   bestDst = 0f;
        int     sample  = Mathf.Min(200, spawnTiles.Count);
        for (int i = 0; i < sample; i++)
        {
            int   idx = UnityEngine.Random.Range(0, spawnTiles.Count);
            float d   = Vector2.Distance(spawnTiles[idx], playerPos);
            if (d > bestDst) { bestDst = d; best = spawnTiles[idx]; }
        }
        return best;
    }

    // ═══════════════════════════════════════════════════════════════
    //  OUT-OF-BOUNDS TELEPORT
    // ═══════════════════════════════════════════════════════════════

    private IEnumerator OutOfBoundsChecker()
    {
        var wait = new WaitForSeconds(oobCheckInterval);
        while (waveActive)
        {
            yield return wait;
            var snapshot = new List<EnemyAI>(aliveEnemies);
            foreach (var ai in snapshot)
            {
                if (ai == null) continue;
                if (!IsOnSpawnZone(ai.transform.position))
                    TeleportToNearest(ai);
            }
        }
    }

    private bool IsOnSpawnZone(Vector3 pos)
    {
        if (activeSpawnZoneTilemap == null)
            return spawnTiles.Count == 0 || IsNearAnyTile(pos);

        Vector3Int cell = activeSpawnZoneTilemap.WorldToCell(pos);
        return activeSpawnZoneTilemap.HasTile(cell);
    }

    private bool IsNearAnyTile(Vector3 pos)
    {
        float threshold = 0.6f;
        foreach (var tile in spawnTiles)
            if (Mathf.Abs(pos.x - tile.x) < threshold && Mathf.Abs(pos.y - tile.y) < threshold)
                return true;
        return false;
    }

    private void TeleportToNearest(EnemyAI ai)
    {
        if (spawnTiles.Count == 0) return;

        Vector3 pos     = ai.transform.position;
        Vector3 best    = spawnTiles[0];
        float   bestDst = float.MaxValue;

        int sample = Mathf.Min(300, spawnTiles.Count);
        for (int i = 0; i < sample; i++)
        {
            int   idx = UnityEngine.Random.Range(0, spawnTiles.Count);
            float d   = Vector2.SqrMagnitude((Vector2)(spawnTiles[idx] - pos));
            if (d < bestDst) { bestDst = d; best = spawnTiles[idx]; }
        }

        ai.transform.position = best;
        Debug.Log($"[WaveManager] OOB Teleport: {ai.name} -> {best}");
    }

    // ═══════════════════════════════════════════════════════════════
    //  SPAWN
    // ═══════════════════════════════════════════════════════════════

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

    private void SetupEnemy(EnemyAI ai, bool isBoss)
    {
        ai.ApplyLevelData(ComputeEnemyData(isBoss));
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
        { dead.OnDeath -= handler; deathHandlers.Remove(dead); }

        aliveEnemies.Remove(dead);

        if (aliveEnemies.Count > 0 || !waveActive) return;

        waveActive = false;
        OnWaveCleared?.Invoke(currentWave);
        if (dead.isBoss) AdvanceStage();
        StartNextWave();
    }

    // ═══════════════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════════════

    private Vector3 PlayerPosition() => player != null ? player.position : Vector3.zero;

    private GameObject GetRandomEnemyPrefab()
    {
        var avail = enemySpawnEntries.FindAll(e => e.prefab != null && currentStage >= e.minStage);
        if (avail.Count == 0)
        {
            avail = enemySpawnEntries.FindAll(e => e.prefab != null);
            if (avail.Count == 0) return null;
        }

        float total = 0f;
        var weights = new float[avail.Count];
        for (int i = 0; i < avail.Count; i++)
        {
            int stages = Mathf.Max(0, currentStage - avail[i].minStage);
            weights[i] = Mathf.Max(0.01f, avail[i].weight) * (1f + weightGrowthPerStage * stages);
            total += weights[i];
        }

        float roll = UnityEngine.Random.Range(0f, total), cum = 0f;
        for (int i = 0; i < avail.Count; i++)
        {
            cum += weights[i];
            if (roll <= cum) return avail[i].prefab;
        }
        return avail[avail.Count - 1].prefab;
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
        if (layer == -1) { Debug.LogWarning($"[WaveManager] Layer '{enemyLayerName}' không tồn tại."); return; }
        Physics2D.IgnoreLayerCollision(layer, layer, true);
    }

    private void SetupWaveUi()
    {
        if (waveUiPrefab == null) return;
        Transform root = waveUiRoot ?? (FindFirstObjectByType<Canvas>()?.transform ?? transform);
        waveUiInstance = Instantiate(waveUiPrefab, root, false);
        waveUiInstance.Bind(this);
    }

    // ═══════════════════════════════════════════════════════════════
    //  PUBLIC API
    // ═══════════════════════════════════════════════════════════════

    public void OnPlayerDied()
    {
        StopAllCoroutines();
        waveActive = false;
        SaveProgress();
    }

    public void RestartWaves()
    {
        StopAllCoroutines();
        currentWave      = Mathf.Max(1, startWave) - 1;
        currentStage     = 1;
        waveActive       = false;
        currentMapPrefab = null;
        spawnTiles.Clear();
        aliveEnemies.Clear();
        deathHandlers.Clear();
        ClearSave();

        if (currentMapInstance != null)
        {
            Destroy(currentMapInstance);
            currentMapInstance = null;
        }

        ApplyStageData(currentStage);
        OnWavesRestarted?.Invoke();
        StartNextWave();
    }

    /// <summary>
    /// [CHEAT] Bắt đầu wave tiếp theo ngay lập tức.
    /// Quái wave cũ vẫn còn sống — không clear aliveEnemies.
    /// Wave mới kết thúc bình thường khi toàn bộ quái (cũ + mới) đã chết.
    /// </summary>
    public void ForceNextWave()
    {
        currentWave++;
        bool isBossWave = currentWave % Mathf.Max(1, bossWaveFrequency) == 0;

        SaveProgress();
        OnWaveStarted?.Invoke(currentWave, currentStage, isBossWave);

        if (isBossWave) { SpawnBoss(); return; }

        int   count    = enemiesBaseCount + (currentWave - 1) * enemiesPerWave;
        float interval = Mathf.Max(minSpawnInterval,
            baseSpawnInterval - (currentWave - 1) * spawnIntervalDecayPerWave);

        StartCoroutine(SpawnWaveRoutine(count, interval));
    }

    // ═══════════════════════════════════════════════════════════════
    //  EDITOR GIZMOS
    // ═══════════════════════════════════════════════════════════════
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showSpawnGizmos) return;

        Gizmos.color = new Color(0f, 1f, 0.3f, 0.22f);
        foreach (var pos in spawnTiles)
            Gizmos.DrawCube(pos, Vector3.one * 0.85f);

        if (player != null)
        {
            Gizmos.color = new Color(1f, 0.15f, 0.15f, 0.4f);
            Gizmos.DrawWireSphere(player.position, minSpawnDistanceFromPlayer);
        }
    }
#endif
}