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

public class WaveManager : Singleton<WaveManager>
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Stage Database")]
    [SerializeField] private StageDataSO stageDatabase;

    [Header("SpawnZone Tilemap Name")]
    [Tooltip("Tên chính xác của Tilemap con dùng làm vùng spawn trong mỗi map prefab.")]
    [SerializeField] private string spawnZoneTilemapName = "SpawnZone";

    [Header("Prefabs")]
    [SerializeField] private List<EnemySpawnEntry> enemySpawnEntries = new();
    [SerializeField] private GameObject bossPrefab;

    [Header("Wave")]
    [SerializeField, Min(1)]    private int   startWave                        = 1;
    [SerializeField, Min(1)]    private int   defaultEnemiesBaseCount          = 4;
    [SerializeField, Min(0)]    private int   defaultEnemiesPerWave            = 2;
    [SerializeField, Min(0f)]   private float defaultBaseSpawnInterval         = 0.8f;
    [SerializeField, Min(0f)]   private float defaultSpawnIntervalDecayPerWave = 0.02f;
    [SerializeField, Min(0.05f)]private float defaultMinSpawnInterval          = 0.15f;
    [SerializeField, Min(1)]    private int   defaultBossWaveFrequency         = 5;

    [Header("Spawn Distance")]
    [Tooltip("Quái không spawn trong bán kính này quanh player (tính theo tile).")]
    [SerializeField, Min(0f)] private float minSpawnDistanceFromPlayer = 4f;

    [Header("Out-of-Bounds Teleport")]
    [Tooltip("Tần suất kiểm tra quái ra ngoài map (giây).")]
    [SerializeField, Min(0.1f)] private float oobCheckInterval = 0.5f;
    [Tooltip("Padding mở rộng bounds khi check OOB — tăng nếu quái bị tele oan.")]
    [SerializeField, Min(0f)]   private float oobBoundsPadding = 1f;

    [Header("Physics")]
    [SerializeField] private string enemyLayerName = "Enemy";

    [Header("UI")]
    [SerializeField] private WaveProgressUI waveUiPrefab;
    [SerializeField] private Transform      waveUiRoot;

    [Header("Player Respawn")]
    [SerializeField, Min(0f)] private float respawnDelay          = 1.5f;
    [SerializeField, Min(0f)] private float graceAfterRespawn     = 3f;
    [SerializeField, Min(0f)] private float postRespawnMinSpawnDistance = 10f;

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
    //  PRIVATE STATE
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
    private bool           isRespawning;
    private bool           justRespawned;
    private bool           spawnBlocked;
    private WaveProgressUI waveUiInstance;

    private GameObject currentMapInstance;
    private GameObject currentMapPrefab;
    private GameObject pendingMapPrefab;

 
    private bool currentMapIsInstantiated = false;

    private readonly List<Vector3> spawnTiles = new();
    private Tilemap activeSpawnZoneTilemap;

  
    private Bounds mapBounds;
    private bool   mapBoundsValid = false;

    public int CurrentWave       => currentWave;
    public int CurrentStage      => currentStage;
    public int BossWaveFrequency => GetStageWaveConfig().bossWaveFrequency;
    public StageData CurrentStageData => stageDatabase?.GetOrLast(currentStage);

    private StageWaveConfig GetStageWaveConfig()
    {
        StageData data = CurrentStageData;
        if (data != null && data.useWaveConfigOverride)
            return data.waveConfigOverride;

        return new StageWaveConfig
        {
            enemiesBaseCount          = defaultEnemiesBaseCount,
            enemiesPerWave            = defaultEnemiesPerWave,
            baseSpawnInterval         = defaultBaseSpawnInterval,
            spawnIntervalDecayPerWave = defaultSpawnIntervalDecayPerWave,
            minSpawnInterval          = defaultMinSpawnInterval,
            bossWaveFrequency         = defaultBossWaveFrequency
        };
    }

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
        FindAndRegisterSceneMap();
        ApplyStageData(currentStage);
        StartNextWave();
    }

    private void FindAndRegisterSceneMap()
    {
        foreach (var tm in FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
        {
            if (tm.name != spawnZoneTilemapName) continue;

            GameObject root = tm.transform.root.gameObject;
            if (root == gameObject) continue;

            currentMapInstance       = root;
            currentMapIsInstantiated = false; // ✅ Đây là scene object, không Destroy
            BuildSpawnCache(currentMapInstance);
            Debug.Log($"[WaveManager] Tìm thấy scene map: \"{root.name}\"");
            return;
        }

        Debug.LogWarning("[WaveManager] Không tìm thấy scene map có SpawnZone tilemap.");
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
        int savedWave = PlayerPrefs.GetInt(KEY_WAVE, Mathf.Max(1, startWave) - 1);
        currentWave  = savedWave - 1;
        currentStage = Mathf.Max(1, PlayerPrefs.GetInt(KEY_STAGE, 1));
        Debug.Log($"[WaveManager] Loaded -> Stage {currentStage}, Wave {savedWave} (resume)");
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

        bool needSwap = data.mapPrefab != null
                     && data.mapPrefab != currentMapPrefab
                     && data.mapPrefab != pendingMapPrefab;

        if (needSwap)
        {
            pendingMapPrefab = data.mapPrefab;

            if (ScreenFader.Instance != null)
                ScreenFader.Instance.FadeIn(0.4f, () =>
                {
                    SwapMap(data.mapPrefab);
                    pendingMapPrefab = null;
                    ScreenFader.Instance.FadeOut(0.4f);
                });
            else
            {
                SwapMap(data.mapPrefab);
                pendingMapPrefab = null;
            }
        }
        else if (data.mapPrefab == null && spawnTiles.Count == 0)
        {
            if (currentMapInstance != null)
                BuildSpawnCache(currentMapInstance);
            else
                Debug.LogWarning($"[WaveManager] Stage {stageNumber} không có mapPrefab " +
                                 "và không tìm thấy scene map — spawnTiles sẽ rỗng.");
        }
    }

    private void SwapMap(GameObject newPrefab)
    {
        // ✅ Chỉ Destroy nếu là prefab instance, không Destroy scene object
        if (currentMapInstance != null && currentMapIsInstantiated)
        {
            Destroy(currentMapInstance);
            currentMapInstance = null;
        }

        currentMapInstance       = Instantiate(newPrefab);
        currentMapPrefab         = newPrefab;
        currentMapIsInstantiated = true; // ✅ Đây là prefab instance
        BuildSpawnCache(currentMapInstance);
    }

    // ═══════════════════════════════════════════════════════════════
    //  SPAWN TILE CACHE
    // ═══════════════════════════════════════════════════════════════

    private void BuildSpawnCache(GameObject mapRoot)
    {
        spawnTiles.Clear();
        activeSpawnZoneTilemap = null;
        mapBoundsValid         = false;

        if (mapRoot == null) return;

        // ✅ Tính bounds tổng hợp từ TẤT CẢ tilemap trong map
        // Dùng để OOB check — khác với SpawnZone chỉ dùng để pick vị trí spawn
        bool boundsInit = false;
        foreach (var tm in mapRoot.GetComponentsInChildren<Tilemap>())
        {
            tm.CompressBounds();
            // ✅ Thay HasAnyTile (Unity 2021.2+) bằng check size
            if (tm.cellBounds.size == Vector3Int.zero) continue;

            Bounds world = new Bounds(
                tm.transform.TransformPoint(tm.localBounds.center),
                tm.transform.TransformVector(tm.localBounds.size)
            );
            world.size = new Vector3(Mathf.Abs(world.size.x),
                Mathf.Abs(world.size.y),
                Mathf.Abs(world.size.z));

            if (!boundsInit) { mapBounds = world; boundsInit = true; }
            else              mapBounds.Encapsulate(world);
        }

        mapBoundsValid = boundsInit;
        if (mapBoundsValid)
            Debug.Log($"[WaveManager] MapBounds: center={mapBounds.center}, size={mapBounds.size}");

        // Tìm SpawnZone tilemap để cache vị trí spawn
        Tilemap target = null;
        foreach (var tm in mapRoot.GetComponentsInChildren<Tilemap>())
        {
            if (tm.name == spawnZoneTilemapName) { target = tm; break; }
        }

        if (target == null)
        {
            Debug.LogWarning($"[WaveManager] Không tìm thấy Tilemap \"{spawnZoneTilemapName}\" " +
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

        int level = (currentStage - 1) * GetStageWaveConfig().bossWaveFrequency + currentWave;
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
        bool isBossWave = currentWave % Mathf.Max(1, GetStageWaveConfig().bossWaveFrequency) == 0;
        waveActive = true;

        SaveProgress();
        OnWaveStarted?.Invoke(currentWave, currentStage, isBossWave);

        StartCoroutine(OutOfBoundsChecker());

        if (isBossWave) { SpawnBoss(); return; }

        int   count    = GetStageWaveConfig().enemiesBaseCount + (currentWave - 1) * GetStageWaveConfig().enemiesPerWave;
        float interval = Mathf.Max(GetStageWaveConfig().minSpawnInterval,
            GetStageWaveConfig().baseSpawnInterval - (currentWave - 1) * GetStageWaveConfig().spawnIntervalDecayPerWave);

        StartCoroutine(SpawnWaveRoutine(count, interval));
    }

    private IEnumerator SpawnWaveRoutine(int count, float interval)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnEnemyAt(PickSpawnTile(), false);
            yield return new WaitForSeconds(interval);
        }
        justRespawned = false;
    }

    private void SpawnBoss() => SpawnEnemyAt(PickSpawnTile(), true);

    // ═══════════════════════════════════════════════════════════════
    //  PICK SPAWN TILE
    // ═══════════════════════════════════════════════════════════════

    private Vector3 PickSpawnTile()
    {
        if (spawnTiles.Count == 0)
        {
            Debug.LogWarning("[WaveManager] SpawnTiles rỗng! Bỏ qua spawn.");
            // ✅ Trả null signal — xử lý ở SpawnEnemyAt bằng cách skip
            return Vector3.positiveInfinity;
        }

        Vector3 playerPos = PlayerPosition();

        float minDist = justRespawned
            ? Mathf.Max(minSpawnDistanceFromPlayer, postRespawnMinSpawnDistance)
            : minSpawnDistanceFromPlayer;

        int attempts = Mathf.Min(60, spawnTiles.Count);
        for (int i = 0; i < attempts; i++)
        {
            int idx = UnityEngine.Random.Range(0, spawnTiles.Count);
            if (Vector2.Distance(spawnTiles[idx], playerPos) >= minDist)
                return spawnTiles[idx];
        }

        float hardMin = minDist * 0.5f;
        Vector3 best    = spawnTiles[0];
        float   bestDst = -1f;
        int     sample  = Mathf.Min(200, spawnTiles.Count);

        for (int i = 0; i < sample; i++)
        {
            int   idx = UnityEngine.Random.Range(0, spawnTiles.Count);
            float d   = Vector2.Distance(spawnTiles[idx], playerPos);
            if (d > bestDst && d >= hardMin) { bestDst = d; best = spawnTiles[idx]; }
        }

        if (bestDst < 0f)
        {
            foreach (var t in spawnTiles)
            {
                float d = Vector2.Distance(t, playerPos);
                if (d > bestDst) { bestDst = d; best = t; }
            }
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
                if (!IsInsideMapBounds(ai.transform.position))
                    TeleportToNearest(ai);
            }
        }
    }
    private bool IsInsideMapBounds(Vector3 pos)
    {
        if (!mapBoundsValid)
        {
           
            return spawnTiles.Count == 0 || IsNearAnyTile(pos);
        }

      
        Bounds padded = mapBounds;
        padded.Expand(oobBoundsPadding * 2f);
        return padded.Contains(new Vector3(pos.x, pos.y, mapBounds.center.z));
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
        if (spawnBlocked)
        {
            Debug.Log("[WaveManager] SpawnEnemyAt bị chặn (grace period).");
            return;
        }

       
        if (float.IsPositiveInfinity(pos.x))
        {
            Debug.LogWarning("[WaveManager] Bỏ qua spawn vì không tìm được vị trí hợp lệ.");
            return;
        }

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

   
    private void ClearAllEnemies()
    {
        var snapshot = new List<EnemyAI>(aliveEnemies);
        foreach (var ai in snapshot)
            ForceRemoveEnemy(ai);

        aliveEnemies.Clear();
        deathHandlers.Clear();

        foreach (var ai in FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
            ForceRemoveEnemy(ai);

        Debug.Log("[WaveManager] ClearAllEnemies: toàn bộ quái đã bị xóa.");
    }

    private void ForceRemoveEnemy(EnemyAI ai)
    {
        if (ai == null) return;

        if (deathHandlers.TryGetValue(ai, out var handler))
        {
            ai.OnDeath -= handler;
            deathHandlers.Remove(ai);
        }

        ai.gameObject.SetActive(false);

        if (ObjectPooler.Instance != null)
            ObjectPooler.Instance.ReturnToPool(ai.gameObject);
        else
            Destroy(ai.gameObject);
    }

    // ═══════════════════════════════════════════════════════════════
    //  RESPAWN PLAYER
    // ═══════════════════════════════════════════════════════════════

    private Vector3 GetMapCenter()
    {
        if (spawnTiles.Count > 0)
        {
            Vector3 sum = Vector3.zero;
            foreach (var t in spawnTiles) sum += t;
            Vector3 center = sum / spawnTiles.Count;
            center.z = 0f;
            return center;
        }

        if (mapBoundsValid)
            return new Vector3(mapBounds.center.x, mapBounds.center.y, 0f);

        if (currentMapInstance != null)
        {
            var renderers = currentMapInstance.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds b = renderers[0].bounds;
                foreach (var r in renderers) b.Encapsulate(r.bounds);
                return new Vector3(b.center.x, b.center.y, 0f);
            }
        }

        Debug.LogWarning("[WaveManager] GetMapCenter: không có dữ liệu map, trả về Vector3.zero.");
        return Vector3.zero;
    }

    private IEnumerator RespawnPlayerRoutine()
    {
        isRespawning = true;

        yield return new WaitForSeconds(respawnDelay);

        if (player == null)
        {
            Debug.LogWarning("[WaveManager] Không tìm thấy player để hồi sinh.");
            isRespawning = false;
            yield break;
        }

        if (!player.gameObject.activeSelf)
            player.gameObject.SetActive(true);

        Vector3 center = GetMapCenter();
        player.position = center;
        Debug.Log($"[WaveManager] Player hồi sinh tại giữa map: {center}");

        if (graceAfterRespawn > 0f)
        {
            Debug.Log($"[WaveManager] Grace period {graceAfterRespawn}s — quái chưa spawn...");
            yield return new WaitForSeconds(graceAfterRespawn);
        }

        isRespawning  = false;
        justRespawned = true;
        spawnBlocked  = false;
        Debug.Log("[WaveManager] Grace period kết thúc — bắt đầu wave mới.");
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
        waveActive   = false;
        spawnBlocked = true;

        ClearAllEnemies();

        isRespawning  = false;
        justRespawned = false;
        currentWave   = Mathf.Max(1, startWave) - 1;
        currentStage  = 1;
        currentMapPrefab = null;
        pendingMapPrefab = null;
        spawnTiles.Clear();
        mapBoundsValid = false;
        ClearSave();

        // ✅ Chỉ Destroy nếu là prefab instance — scene map giữ nguyên
        if (currentMapInstance != null && currentMapIsInstantiated)
        {
            Destroy(currentMapInstance);
            currentMapInstance       = null;
            currentMapIsInstantiated = false;
        }

        // ✅ Tìm lại scene map sau reset để spawnTiles không bị rỗng
        FindAndRegisterSceneMap();

        ApplyStageData(currentStage);
        OnWavesRestarted?.Invoke();

        StartCoroutine(RespawnPlayerRoutine());
    }

    public void RestartWaves()
    {
        StopAllCoroutines();
        waveActive = false;

        ClearAllEnemies();

        isRespawning     = false;
        justRespawned    = false;
        spawnBlocked     = false;
        currentWave      = Mathf.Max(1, startWave) - 1;
        currentStage     = 1;
        currentMapPrefab = null;
        pendingMapPrefab = null;
        spawnTiles.Clear();
        mapBoundsValid   = false;
        ClearSave();

        // ✅ Chỉ Destroy nếu là prefab instance
        if (currentMapInstance != null && currentMapIsInstantiated)
        {
            Destroy(currentMapInstance);
            currentMapInstance       = null;
            currentMapIsInstantiated = false;
        }

        // ✅ Tìm lại scene map
        FindAndRegisterSceneMap();

        ApplyStageData(currentStage);
        OnWavesRestarted?.Invoke();
        StartNextWave();
    }

    public void ForceNextWave()
    {
        currentWave++;
        bool isBossWave = currentWave % Mathf.Max(1, GetStageWaveConfig().bossWaveFrequency) == 0;

        SaveProgress();
        OnWaveStarted?.Invoke(currentWave, currentStage, isBossWave);

        if (isBossWave) { SpawnBoss(); return; }

        int   count    = GetStageWaveConfig().enemiesBaseCount + (currentWave - 1) * GetStageWaveConfig().enemiesPerWave;
        float interval = Mathf.Max(GetStageWaveConfig().minSpawnInterval,
            GetStageWaveConfig().baseSpawnInterval - (currentWave - 1) * GetStageWaveConfig().spawnIntervalDecayPerWave);

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

        // ✅ Vẽ map bounds để dễ debug OOB
        if (mapBoundsValid)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawWireCube(mapBounds.center, mapBounds.size + Vector3.one * oobBoundsPadding * 2f);
        }

        if (spawnTiles.Count > 0)
        {
            Vector3 sum = Vector3.zero;
            foreach (var t in spawnTiles) sum += t;
            Vector3 center = sum / spawnTiles.Count;
            Gizmos.color = new Color(0.2f, 0.5f, 1f, 0.9f);
            Gizmos.DrawWireSphere(center, 0.45f);
            Gizmos.DrawLine(center + Vector3.up    * 0.6f, center - Vector3.up    * 0.6f);
            Gizmos.DrawLine(center + Vector3.right * 0.6f, center - Vector3.right * 0.6f);
        }
    }
#endif
}