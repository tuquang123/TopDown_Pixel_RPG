using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemySpawnEntry
{
    [Tooltip("Prefab quái có thể được WaveManager spawn.")]
    public GameObject prefab;

    [Tooltip("Stage nhỏ nhất mà prefab này bắt đầu xuất hiện.")]
    [Min(1)] public int minStage;

    [Tooltip("Stage lớn nhất mà prefab này còn xuất hiện. Đặt 0 để không giới hạn.")]
    [Min(0)] public int maxStage;

    [Tooltip("Tỉ lệ chọn prefab trong danh sách hợp lệ. Số càng cao thì càng dễ được spawn.")]
    [Min(0.1f)] public float weight;
}

[System.Serializable]
public struct BossStageEntry
{
    [Tooltip("Boss này bắt đầu được dùng từ stage này trở đi, cho đến khi có entry stage cao hơn.")]
    [Min(1)] public int stage;

    [Tooltip("Prefab boss dùng cho stage tương ứng.")]
    public GameObject bossPrefab;
}

[CreateAssetMenu(fileName = "WaveManagerConfig", menuName = "Data/Wave Manager Config")]
public class WaveManagerConfigSO : ScriptableObject
{
    [Header("Design Notes")]
    [Tooltip("Ghi chú cân bằng tổng quan để designer hiểu mục tiêu difficulty curve của config này.")]
    [TextArea(3, 8)]
    public string description = "Cấu hình wave mặc định: số lượng quái, nhịp spawn, tăng trưởng chỉ số, boss và respawn.";

    [Header("Spawn Zone")]
    [Tooltip("Tên chính xác của Tilemap con dùng làm vùng spawn trong mỗi map prefab.")]
    public string spawnZoneTilemapName = "SpawnZone";

    [Header("Enemy Prefabs")]
    [Tooltip("Danh sách quái thường. min/max stage giới hạn thời điểm xuất hiện, weight quyết định tỉ lệ được chọn.")]
    public List<EnemySpawnEntry> enemySpawnEntries = new();

    [Tooltip("Boss theo stage. Stage hiện tại chưa có entry sẽ dùng entry có stage cao nhất nhưng vẫn <= stage hiện tại.")]
    public List<BossStageEntry> bossStageOverrides = new();

    [Header("Default Wave")]
    [Tooltip("Wave đầu tiên khi bắt đầu game hoặc restart.")]
    [Min(1)] public int startWave = 1;

    [Tooltip("Số quái cơ bản ở wave 1 khi stage không override wave config.")]
    [Min(1)] public int defaultEnemiesBaseCount = 4;

    [Tooltip("Số quái cộng thêm sau mỗi wave khi stage không override wave config.")]
    [Min(0)] public int defaultEnemiesPerWave = 2;

    [Tooltip("Thời gian giữa mỗi lần spawn ở wave 1 khi stage không override wave config.")]
    [Min(0f)] public float defaultBaseSpawnInterval = 0.8f;

    [Tooltip("Mỗi wave sẽ giảm spawn interval bao nhiêu giây khi stage không override wave config.")]
    [Min(0f)] public float defaultSpawnIntervalDecayPerWave = 0.02f;

    [Tooltip("Spawn interval thấp nhất để tránh spawn quá dày.")]
    [Min(0.05f)] public float defaultMinSpawnInterval = 0.15f;

    [Tooltip("Cứ bao nhiêu wave thì gặp boss nếu stage không override wave config.")]
    [Min(1)] public int defaultBossWaveFrequency = 5;

    [Header("Boss Wave")]
    [Tooltip("Delay trước khi boss xuất hiện. BossWarningUI nhấp nháy trong thời gian này.")]
    [Min(0f)] public float bossSpawnDelay = 3f;

    [Tooltip("Delay sau khi boss chết trước khi bắt đầu wave tiếp theo/stage tiếp theo.")]
    [Min(0f)] public float bossWaveClearDelay = 3f;

    [Header("Wave Announcement")]
    [Tooltip("Delay trước khi hiện thông báo wave sau khi wave bắt đầu.")]
    [Min(0f)] public float waveAnnounceDelay = 1f;

    [Header("Spawn Distance")]
    [Tooltip("Quái không spawn trong bán kính này quanh player, tính theo tile/world unit.")]
    [Min(0f)] public float minSpawnDistanceFromPlayer = 4f;

    [Header("Out-of-Bounds Teleport")]
    [Tooltip("Tần suất kiểm tra quái ra ngoài map, tính bằng giây.")]
    [Min(0.1f)] public float oobCheckInterval = 0.5f;

    [Tooltip("Padding mở rộng bounds khi check OOB. Tăng nếu quái bị teleport oan.")]
    [Min(0f)] public float oobBoundsPadding = 1f;

    [Header("Physics")]
    [Tooltip("Tên layer của enemy để WaveManager tắt va chạm enemy-enemy.")]
    public string enemyLayerName = "Enemy";

    [Header("Player Respawn")]
    [Tooltip("Delay từ lúc player chết đến khi hồi sinh.")]
    [Min(0f)] public float respawnDelay = 1.5f;

    [Tooltip("Thời gian chặn spawn sau khi player hồi sinh.")]
    [Min(0f)] public float graceAfterRespawn = 3f;

    [Tooltip("Khoảng cách spawn tối thiểu riêng cho wave đầu sau hồi sinh.")]
    [Min(0f)] public float postRespawnMinSpawnDistance = 10f;

    [Header("Enemy Base Stats")]
    [Tooltip("Máu cơ bản của quái thường trước khi áp dụng scaling wave/stage/boss.")]
    public int baseHealth = 50;

    [Tooltip("Damage cơ bản của quái thường trước khi áp dụng scaling wave/stage/boss.")]
    public int baseDamage = 8;

    [Tooltip("Tốc chạy cơ bản của quái thường trước khi áp dụng scaling wave/stage.")]
    public float baseMoveSpeed = 2f;

    [Tooltip("Tầm đánh áp dụng cho quái được WaveManager spawn.")]
    public float baseAttackRange = 1.2f;

    [Tooltip("Tầm phát hiện player áp dụng cho quái được WaveManager spawn.")]
    public float baseDetectRange = 6f;

    [Tooltip("Cooldown đánh cơ bản trước khi giảm theo stage.")]
    public float baseAttackCooldown = 1.5f;

    [Header("Wave Scaling")]
    [Tooltip("Tăng máu theo từng wave. 0.15 nghĩa là +15% mỗi wave theo cấp số nhân.")]
    [Min(0f)] public float waveHealthGrowth = 0.15f;

    [Tooltip("Tăng damage theo từng wave. 0.10 nghĩa là +10% mỗi wave theo cấp số nhân.")]
    [Min(0f)] public float waveDamageGrowth = 0.10f;

    [Tooltip("Tăng tốc độ theo từng wave. 0.02 nghĩa là +2% tuyến tính mỗi wave.")]
    [Min(0f)] public float waveSpeedGrowth = 0.02f;

    [Header("Stage Scaling")]
    [Tooltip("Tăng máu theo stage. 0.20 nghĩa là +20% mỗi stage sau stage 1.")]
    [Min(0f)] public float healthScalePerStage = 0.20f;

    [Tooltip("Tăng damage theo stage. 0.15 nghĩa là +15% mỗi stage sau stage 1.")]
    [Min(0f)] public float damageScalePerStage = 0.15f;

    [Tooltip("Tăng tốc chạy theo stage. 0.03 nghĩa là +3% mỗi stage sau stage 1.")]
    [Min(0f)] public float moveSpeedScalePerStage = 0.03f;

    [Tooltip("Giảm attack cooldown theo stage. 0.01 nghĩa là giảm 1% mỗi stage sau stage 1.")]
    [Min(0f)] public float cooldownReductionPerStage = 0.01f;

    [Tooltip("Cooldown đánh thấp nhất sau khi áp dụng giảm theo stage.")]
    [Min(0.05f)] public float minAttackCooldown = 0.2f;

    [Header("Boss Multiplier")]
    [Tooltip("Hệ số nhân máu cho boss sau khi đã tính scaling wave/stage.")]
    [Min(1f)] public float bossHealthMult = 5f;

    [Tooltip("Hệ số nhân damage cho boss sau khi đã tính scaling wave/stage.")]
    [Min(1f)] public float bossDamageMult = 2f;

    [Header("Level")]
    [Tooltip("Level cộng thêm cho boss so với level tính theo stage/wave.")]
    [Min(0)] public int bossLevelBonus = 2;

    [Header("Spawn Weight Scaling")]
    [Tooltip("Tăng weight theo số stage kể từ minStage của quái. Giúp quái cũ/mới đổi tỉ lệ theo stage.")]
    [Min(0f)] public float weightGrowthPerStage = 0.1f;

    public StageWaveConfig CreateDefaultStageWaveConfig()
    {
        return new StageWaveConfig
        {
            enemiesBaseCount = defaultEnemiesBaseCount,
            enemiesPerWave = defaultEnemiesPerWave,
            baseSpawnInterval = defaultBaseSpawnInterval,
            spawnIntervalDecayPerWave = defaultSpawnIntervalDecayPerWave,
            minSpawnInterval = defaultMinSpawnInterval,
            bossWaveFrequency = defaultBossWaveFrequency
        };
    }
}
