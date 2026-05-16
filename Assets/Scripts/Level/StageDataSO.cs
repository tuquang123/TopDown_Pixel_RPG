using System.Collections.Generic;
using UnityEngine;

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
    public int bonusExp  = 0;

    [Header("Wave Config (Override)")]
    [Tooltip("Bật để dùng config riêng cho stage này thay vì config mặc định trong WaveManager.")]
    public bool useWaveConfigOverride;
    public StageWaveConfig waveConfigOverride = StageWaveConfig.Default;
}

[System.Serializable]
public struct StageWaveConfig
{
    [Min(1)]  public int enemiesBaseCount;
    [Min(0)]  public int enemiesPerWave;
    [Min(0f)] public float baseSpawnInterval;
    [Min(0f)] public float spawnIntervalDecayPerWave;
    [Min(0.05f)] public float minSpawnInterval;
    [Min(1)]  public int bossWaveFrequency;

    public static StageWaveConfig Default => new()
    {
        enemiesBaseCount = 4,
        enemiesPerWave = 2,
        baseSpawnInterval = 0.8f,
        spawnIntervalDecayPerWave = 0.02f,
        minSpawnInterval = 0.15f,
        bossWaveFrequency = 5
    };
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
