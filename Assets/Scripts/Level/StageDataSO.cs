using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StageGameplayConfig
{
    [Header("Wave")]
    [Min(1)] public int enemiesBaseCount;
    [Min(0)] public int enemiesPerWave;
    [Min(0f)] public float baseSpawnInterval;
    [Min(0f)] public float spawnIntervalDecayPerWave;
    [Min(0.05f)] public float minSpawnInterval;
    [Min(1)] public int bossWaveFrequency;

    [Header("Spawn")]
    [Min(0f)] public float minSpawnDistanceFromPlayer;
    [Min(0.1f)] public float oobCheckInterval;
    [Min(0f)] public float postRespawnMinSpawnDistance;

    [Header("Enemy Base Stats")]
    public int baseHealth;
    public int baseDamage;
    public float baseMoveSpeed;
    public float baseAttackRange;
    public float baseDetectRange;
    public float baseAttackCooldown;

    [Header("Wave Scaling")]
    [Min(0f)] public float waveHealthGrowth;
    [Min(0f)] public float waveDamageGrowth;
    [Min(0f)] public float waveSpeedGrowth;

    [Header("Stage Scaling")]
    [Min(0f)] public float healthScalePerStage;
    [Min(0f)] public float damageScalePerStage;
    [Min(0f)] public float moveSpeedScalePerStage;
    [Min(0f)] public float cooldownReductionPerStage;
    [Min(0.05f)] public float minAttackCooldown;

    [Header("Boss")]
    [Min(1f)] public float bossHealthMult;
    [Min(1f)] public float bossDamageMult;
    [Min(0)] public int bossLevelBonus;

    [Header("Spawn Weight")]
    [Min(0f)] public float weightGrowthPerStage;

    [Header("Respawn")]
    [Min(0f)] public float respawnDelay;
    [Min(0f)] public float graceAfterRespawn;

    public static StageGameplayConfig Default => new()
    {
        enemiesBaseCount = 4,
        enemiesPerWave = 2,
        baseSpawnInterval = 0.8f,
        spawnIntervalDecayPerWave = 0.02f,
        minSpawnInterval = 0.15f,
        bossWaveFrequency = 5,
        minSpawnDistanceFromPlayer = 4f,
        oobCheckInterval = 0.5f,
        postRespawnMinSpawnDistance = 10f,
        baseHealth = 50,
        baseDamage = 8,
        baseMoveSpeed = 2f,
        baseAttackRange = 1.2f,
        baseDetectRange = 6f,
        baseAttackCooldown = 1.5f,
        waveHealthGrowth = 0.15f,
        waveDamageGrowth = 0.10f,
        waveSpeedGrowth = 0.02f,
        healthScalePerStage = 0.20f,
        damageScalePerStage = 0.15f,
        moveSpeedScalePerStage = 0.03f,
        cooldownReductionPerStage = 0.01f,
        minAttackCooldown = 0.2f,
        bossHealthMult = 5f,
        bossDamageMult = 2f,
        bossLevelBonus = 2,
        weightGrowthPerStage = 0.1f,
        respawnDelay = 1.5f,
        graceAfterRespawn = 3f
    };
}

[System.Serializable]
public class StageData
{
    [Header("Info")]
    public string stageName = "Stage 1";

    [Header("Map")]
    [Tooltip("Prefab map chồng lên base map khi vào stage này.\n" +
             "Stage 1 (base map) để trống vì base map đã có sẵn trong scene.")]
    public GameObject mapPrefab;

    [Header("Reward khi clear stage")]
    public int bonusGold = 0;
    public int bonusExp = 0;

    [Header("Gameplay Config")]
    public StageGameplayConfig gameplayConfig = StageGameplayConfig.Default;
}

[CreateAssetMenu(fileName = "StageDatabase", menuName = "Data/StageDatabase")]
public class StageDataSO : ScriptableObject
{
    [Tooltip("Danh sách stage theo thứ tự. Index 0 = Stage 1, Index 1 = Stage 2, ...")]
    public List<StageData> stages = new();

    public StageData Get(int stageNumber)
    {
        int index = stageNumber - 1;
        if (index < 0 || index >= stages.Count) return null;
        return stages[index];
    }

    public StageData GetOrLast(int stageNumber)
    {
        if (stages == null || stages.Count == 0) return null;
        int index = (stageNumber - 1) % stages.Count;
        return stages[index];
    }
}
