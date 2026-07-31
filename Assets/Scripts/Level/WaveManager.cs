using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaveManager : Singleton<WaveManager>
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Stage Database")]
    [SerializeField] private StageDataSO stageDatabase;

    [Header("Wave Config")]
    [Tooltip("ScriptableObject chứa toàn bộ data cân bằng wave/enemy/boss/spawn. Không chỉnh data balance trực tiếp trên WaveManager.")]
    [SerializeField] private WaveManagerConfigSO waveConfig;

    [Header("Boss Warning UI")]
    [Tooltip("Kéo object BossWarning trong scene vào đây.")]
    [SerializeField] private BossWarningUI bossWarningUI;

    [Header("Wave Announcement UI")]
    [Tooltip("Kéo object WaveAnnouncement trong scene vào đây.")]
    [SerializeField] private WaveAnnouncementUI waveAnnouncementUI;

    [Header("UI")]
    [SerializeField] private WaveProgressUI waveUiPrefab;
    [SerializeField] private Transform      waveUiRoot;

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
    private bool           isBossWaveActive;

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

        return waveConfig.CreateDefaultStageWaveConfig();
    }

    // ═══════════════════════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════

    protected override void Awake()
    {
        base.Awake();
        EnsureWaveConfig();
    }

    private void EnsureWaveConfig()
    {
        if (waveConfig != null) return;

        waveConfig = ScriptableObject.CreateInstance<WaveManagerConfigSO>();
        Debug.LogWarning("[WaveManager] Chưa gán WaveManagerConfigSO, đang dùng runtime default config. Hãy tạo asset Data/Wave Manager Config và gán vào WaveManager để designer cân bằng.");
    }

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

    // ═══════════════════════════════════════════════════════════════
    //  SCENE MAP FINDER
    // ═══════════════════════════════════════════════════════════════

    private void FindAndRegisterSceneMap()
    {
        foreach (var tm in FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
        {
            if (tm.name != waveConfig.spawnZoneTilemapName) continue;

            GameObject root = tm.transform.root.gameObject;
            if (root == gameObject) continue;

            currentMapInstance       = root;
            currentMapIsInstantiated = false;
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
        if (PlayerPrefs.HasKey(KEY_WAVE))
        {
            currentWave = PlayerPrefs.GetInt(KEY_WAVE) - 1;
            Debug.Log($"[WaveManager] Loaded save -> Resume wave {currentWave + 1}");
        }
        else
        {
            currentWave = Mathf.Max(1, waveConfig.startWave) - 1;
            Debug.Log($"[WaveManager] Không có save -> Start từ wave {waveConfig.startWave}");
        }

        currentStage = Mathf.Max(1, PlayerPrefs.GetInt(KEY_STAGE, 1));
        Debug.Log($"[WaveManager] Stage {currentStage}, currentWave internal={currentWave}");
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

    private bool AdvanceStage(Action onRewardClaimed = null)
    {
        int completedStage = currentStage;
        StageData completedStageData = stageDatabase?.Get(completedStage);

        currentStage++;
        ApplyStageData(currentStage);
        QuestManager.Instance?.ReportStageCompleted(completedStage);
        OnStageChanged?.Invoke(currentStage);

        return ShowStageReward(completedStageData, completedStage, onRewardClaimed);
    }

    private bool ShowStageReward(StageData completedStageData, int completedStage, Action onRewardClaimed)
    {
        QuestReward reward = completedStageData?.clearReward;
        if (reward == null || !RewardGrantUtility.HasAnyReward(reward))
            return false;

        string stageText = LanguageManager.Instance != null
            ? LanguageManager.Instance.GetTranslationOrFallback("stage", "Stage")
            : "Stage";

        void ClaimReward()
        {
            RewardGrantUtility.Grant(reward, transform);
            onRewardClaimed?.Invoke();
        }

        var popup = UIManager.Instance?.ShowQuestRewardPopup(reward, $"{stageText} {completedStage}", ClaimReward);
        if (popup == null)
        {
            RewardGrantUtility.Grant(reward, transform);
            return false;
        }

        return true;
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
        if (currentMapInstance != null && currentMapIsInstantiated)
        {
            Destroy(currentMapInstance);
            currentMapInstance = null;
        }

        currentMapInstance       = Instantiate(newPrefab);
        currentMapPrefab         = newPrefab;
        currentMapIsInstantiated = true;
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

        bool boundsInit = false;
        foreach (var tm in mapRoot.GetComponentsInChildren<Tilemap>())
        {
            tm.CompressBounds();
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

        Tilemap target = null;
        foreach (var tm in mapRoot.GetComponentsInChildren<Tilemap>())
        {
            if (tm.name == waveConfig.spawnZoneTilemapName) { target = tm; break; }
        }

        if (target == null)
        {
            Debug.LogWarning($"[WaveManager] Không tìm thấy Tilemap \"{waveConfig.spawnZoneTilemapName}\" " +
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

        float wf = Mathf.Pow(1f + waveConfig.waveHealthGrowth, waveIndex);
        float df = Mathf.Pow(1f + waveConfig.waveDamageGrowth, waveIndex);
        float sf = 1f + waveConfig.waveSpeedGrowth * waveIndex;

        int   s        = currentStage - 1;
        float stageHp  = 1f + s * waveConfig.healthScalePerStage;
        float stageDmg = 1f + s * waveConfig.damageScalePerStage;
        float stageSpd = 1f + s * waveConfig.moveSpeedScalePerStage;

        int   hp  = Mathf.Max(1, Mathf.RoundToInt(waveConfig.baseHealth * wf * stageHp));
        int   dmg = Mathf.Max(1, Mathf.RoundToInt(waveConfig.baseDamage * df * stageDmg));
        float spd = Mathf.Max(0.1f, waveConfig.baseMoveSpeed * sf * stageSpd);
        float cd  = Mathf.Max(waveConfig.minAttackCooldown,
                        waveConfig.baseAttackCooldown * Mathf.Pow(1f - waveConfig.cooldownReductionPerStage, s));

        if (!isBoss && currentStage == 1)
        {
            hp  = Mathf.Max(1, Mathf.RoundToInt(hp * 0.5f));
            dmg = Mathf.Max(1, Mathf.RoundToInt(dmg * 0.5f));
        }

        if (isBoss)
        {
            hp  = Mathf.RoundToInt(hp  * waveConfig.bossHealthMult);
            dmg = Mathf.RoundToInt(dmg * waveConfig.bossDamageMult);
        }

        int level = (currentStage - 1) * GetStageWaveConfig().bossWaveFrequency + currentWave;
        if (isBoss) level += waveConfig.bossLevelBonus;

        return new EnemyLevelData
        {
            level          = level,
            maxHealth      = hp,
            attackDamage   = dmg,
            moveSpeed      = spd,
            attackRange    = waveConfig.baseAttackRange,
            detectionRange = waveConfig.baseDetectRange,
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

        if (waveAnnouncementUI != null)
            StartCoroutine(DelayedAnnounce(currentWave, currentStage, isBossWave));

        StartCoroutine(OutOfBoundsChecker());

        if (isBossWave)
        {
            StartCoroutine(BossWaveRoutine());
            return;
        }

        isBossWaveActive = false;

        int   count    = GetStageWaveConfig().enemiesBaseCount + (currentWave - 1) * GetStageWaveConfig().enemiesPerWave;
        float interval = Mathf.Max(GetStageWaveConfig().minSpawnInterval,
            GetStageWaveConfig().baseSpawnInterval - (currentWave - 1) * GetStageWaveConfig().spawnIntervalDecayPerWave);

        StartCoroutine(SpawnWaveRoutine(count, interval));
    }
    private IEnumerator DelayedAnnounce(int wave, int stage, bool isBossWave)
    {
        yield return new WaitForSeconds(waveConfig.waveAnnounceDelay);
        waveAnnouncementUI.Show(wave, stage, isBossWave);
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

    // ═══════════════════════════════════════════════════════════════
    //  BOSS WAVE ROUTINE
    // ═══════════════════════════════════════════════════════════════

    private IEnumerator BossWaveRoutine()
    {
        isBossWaveActive = true;

        if (bossWarningUI != null)
            bossWarningUI.SetVisible(true);

        Debug.Log($"[WaveManager] Boss wave! Đang chờ {waveConfig.bossSpawnDelay}s...");
        yield return new WaitForSeconds(waveConfig.bossSpawnDelay);

        if (bossWarningUI != null)
            bossWarningUI.SetVisible(false);

        GameObject prefab = GetBossPrefabForCurrentStage();

        if (prefab == null)
        {
            Debug.LogWarning($"[WaveManager] Không có boss prefab cho stage {currentStage} — fallback sang wave thường.");
            isBossWaveActive = false;

            int   count    = GetStageWaveConfig().enemiesBaseCount
                           + (currentWave - 1) * GetStageWaveConfig().enemiesPerWave;
            float interval = Mathf.Max(GetStageWaveConfig().minSpawnInterval,
                GetStageWaveConfig().baseSpawnInterval
                - (currentWave - 1) * GetStageWaveConfig().spawnIntervalDecayPerWave);

            StartCoroutine(SpawnWaveRoutine(count, interval));
            yield break;
        }

        SpawnEnemyAt(PickSpawnTile(), true, prefab);
        Debug.Log("[WaveManager] Boss đã spawn.");
    }

    private GameObject GetBossPrefabForCurrentStage()
    {
        GameObject result    = null;
        int        bestStage = -1;

        foreach (var entry in waveConfig.bossStageOverrides)
        {
            if (entry.bossPrefab == null) continue;
            if (entry.stage <= currentStage && entry.stage > bestStage)
            {
                bestStage = entry.stage;
                result    = entry.bossPrefab;
            }
        }

        if (result == null)
            Debug.LogWarning($"[WaveManager] Không tìm thấy BossStageEntry nào khớp stage {currentStage}!");
        else
            Debug.Log($"[WaveManager] Boss stage {bestStage} → \"{result.name}\" (stage hiện tại: {currentStage})");

        return result;
    }

    // ═══════════════════════════════════════════════════════════════
    //  PICK SPAWN TILE
    // ═══════════════════════════════════════════════════════════════

    private Vector3 PickSpawnTile()
    {
        if (spawnTiles.Count == 0)
        {
            Debug.LogWarning("[WaveManager] SpawnTiles rỗng! Bỏ qua spawn.");
            return Vector3.positiveInfinity;
        }

        Vector3 playerPos = PlayerPosition();

        float minDist = justRespawned
            ? Mathf.Max(waveConfig.minSpawnDistanceFromPlayer, waveConfig.postRespawnMinSpawnDistance)
            : waveConfig.minSpawnDistanceFromPlayer;

        int attempts = Mathf.Min(60, spawnTiles.Count);
        for (int i = 0; i < attempts; i++)
        {
            int idx = UnityEngine.Random.Range(0, spawnTiles.Count);
            if (Vector2.Distance(spawnTiles[idx], playerPos) >= minDist)
                return spawnTiles[idx];
        }

        float   hardMin = minDist * 0.5f;
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
        var wait = new WaitForSeconds(waveConfig.oobCheckInterval);
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
            return spawnTiles.Count == 0 || IsNearAnyTile(pos);

        Bounds padded = mapBounds;
        padded.Expand(waveConfig.oobBoundsPadding * 2f);
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

    private void SpawnEnemyAt(Vector3 pos, bool isBoss, GameObject overridePrefab = null)
    {
        if (spawnBlocked)
        {
            Debug.Log("[WaveManager] SpawnEnemyAt bị chặn (grace period).");
            return;
        }

        if (isBossWaveActive && !isBoss)
        {
            Debug.Log("[WaveManager] SpawnEnemyAt bị chặn (boss wave đang diễn ra).");
            return;
        }

        if (float.IsPositiveInfinity(pos.x))
        {
            Debug.LogWarning("[WaveManager] Bỏ qua spawn vì không tìm được vị trí hợp lệ.");
            return;
        }

        GameObject prefab = overridePrefab != null
            ? overridePrefab
            : GetRandomEnemyPrefab();

        if (prefab == null) return;

        string poolKey = $"{prefab.name}_{prefab.GetInstanceID()}";

        GameObject obj = ObjectPooler.Instance != null
            ? ObjectPooler.Instance.Get(poolKey, prefab, pos, Quaternion.identity, initSize: 8, expandable: true)
            : Instantiate(prefab, pos, Quaternion.identity);

        if (obj == null)
        {
            Debug.LogError($"[WaveManager] SpawnEnemyAt: ObjectPooler trả về null cho prefab \"{prefab.name}\" (key={poolKey}).");
            return;
        }

        if (!obj.TryGetComponent(out EnemyAI ai))
        {
            Debug.LogError($"[WaveManager] SpawnEnemyAt: \"{obj.name}\" không có component EnemyAI!");
            return;
        }

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

        if (dead.isBoss)
        {
            isBossWaveActive = false;
            StartCoroutine(DelayedNextWave(waveConfig.bossWaveClearDelay));
        }
        else
        {
            StartNextWave();
        }
    }

    private IEnumerator DelayedNextWave(float delay)
    {
        Debug.Log($"[WaveManager] Boss đã chết! Nghỉ {delay}s trước wave tiếp...");
        yield return new WaitForSeconds(delay);

        bool waitingForReward = AdvanceStage(StartNextWave);
        if (!waitingForReward)
            StartNextWave();
    }

    // ═══════════════════════════════════════════════════════════════
    //  XÓA TOÀN BỘ QUÁI ĐANG SỐNG
    // ═══════════════════════════════════════════════════════════════

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

        yield return new WaitForSeconds(waveConfig.respawnDelay);

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

        if (waveConfig.graceAfterRespawn > 0f)
        {
            Debug.Log($"[WaveManager] Grace period {waveConfig.graceAfterRespawn}s — quái chưa spawn...");
            yield return new WaitForSeconds(waveConfig.graceAfterRespawn);
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
        var avail = waveConfig.enemySpawnEntries.FindAll(e =>
            e.prefab != null && 
            currentStage >= e.minStage &&
            (e.maxStage == 0 || currentStage <= e.maxStage));

        if (avail.Count == 0)
        {
            avail = waveConfig.enemySpawnEntries.FindAll(e => e.prefab != null);
            if (avail.Count == 0) return null;
        }

        float total = 0f;
        var weights = new float[avail.Count];
        for (int i = 0; i < avail.Count; i++)
        {
            int stages = Mathf.Max(0, currentStage - avail[i].minStage);
            weights[i] = Mathf.Max(0.01f, avail[i].weight) * (1f + waveConfig.weightGrowthPerStage * stages);
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
        int layer = LayerMask.NameToLayer(waveConfig.enemyLayerName);
        if (layer == -1) { Debug.LogWarning($"[WaveManager] Layer '{waveConfig.enemyLayerName}' không tồn tại."); return; }
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
        waveActive       = false;
        spawnBlocked     = true;
        isBossWaveActive = false;

        if (bossWarningUI != null) bossWarningUI.SetVisible(false);

        ClearAllEnemies();

        isRespawning     = false;
        justRespawned    = false;
        currentWave      = Mathf.Max(1, waveConfig.startWave) - 1;
        currentStage     = 1;
        currentMapPrefab = null;
        pendingMapPrefab = null;
        spawnTiles.Clear();
        mapBoundsValid   = false;
        ClearSave();

        if (currentMapInstance != null && currentMapIsInstantiated)
        {
            Destroy(currentMapInstance);
            currentMapInstance       = null;
            currentMapIsInstantiated = false;
        }

        FindAndRegisterSceneMap();
        ApplyStageData(currentStage);
        OnWavesRestarted?.Invoke();
        StartCoroutine(RespawnPlayerRoutine());
    }

    public void RestartWaves()
    {
        StopAllCoroutines();
        waveActive       = false;
        isBossWaveActive = false;

        if (bossWarningUI != null) bossWarningUI.SetVisible(false);

        ClearAllEnemies();

        isRespawning     = false;
        justRespawned    = false;
        spawnBlocked     = false;
        currentWave      = Mathf.Max(1, waveConfig.startWave) - 1;
        currentStage     = 1;
        currentMapPrefab = null;
        pendingMapPrefab = null;
        spawnTiles.Clear();
        mapBoundsValid   = false;
        ClearSave();

        if (currentMapInstance != null && currentMapIsInstantiated)
        {
            Destroy(currentMapInstance);
            currentMapInstance       = null;
            currentMapIsInstantiated = false;
        }

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

        if (isBossWave)
        {
            StartCoroutine(BossWaveRoutine());
            return;
        }

        isBossWaveActive = false;

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
        if (!showSpawnGizmos || waveConfig == null) return;

        Gizmos.color = new Color(0f, 1f, 0.3f, 0.22f);
        foreach (var pos in spawnTiles)
            Gizmos.DrawCube(pos, Vector3.one * 0.85f);

        if (player != null)
        {
            Gizmos.color = new Color(1f, 0.15f, 0.15f, 0.4f);
            Gizmos.DrawWireSphere(player.position, waveConfig.minSpawnDistanceFromPlayer);
        }

        if (mapBoundsValid)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawWireCube(mapBounds.center, mapBounds.size + Vector3.one * waveConfig.oobBoundsPadding * 2f);
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
