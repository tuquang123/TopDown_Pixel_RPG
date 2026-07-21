using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class StageData
{
    [Header("Info")]
    [Tooltip("Map data này áp dụng từ stage này trở đi (cho đến khi gặp entry có fromStage lớn hơn).\n" +
             "Ví dụ: entry A có fromStage = 1, entry B có fromStage = 5 → stage 2, 3, 4 vẫn dùng map/config của entry A.")]
    [Min(1)]
    public int fromStage = 1;

    public string stageName = "Stage 1";

    [Header("Map")]
    [Tooltip("Prefab map chồng lên base map khi vào stage này.\n" +
             "Stage 1 (base map) để trống vì base map đã có sẵn trong scene.")]
    public GameObject mapPrefab;

    [Header("Wave Config (Override)")]
    [Tooltip("Bật để dùng config riêng cho stage này thay vì config mặc định trong WaveManagerConfigSO.")]
    public bool useWaveConfigOverride;

    [Tooltip("Override số lượng quái, tốc độ spawn và chu kỳ boss cho stage này. Dùng để designer cân bằng difficulty từng stage.")]
    public StageWaveConfig waveConfigOverride = StageWaveConfig.Default;

    [Header("Clear Reward")]
    [Tooltip("Phan thuong nhan khi clear dung stage nay. De trong hoac 0 neu stage khong co thuong.")]
    public QuestReward clearReward;
}

[System.Serializable]
public struct StageWaveConfig
{
    [Tooltip("Số quái cơ bản ở wave 1 của stage này.")]
    [Min(1)] public int enemiesBaseCount;

    [Tooltip("Số quái cộng thêm sau mỗi wave trong stage này.")]
    [Min(0)] public int enemiesPerWave;

    [Tooltip("Thời gian giữa mỗi lần spawn ở wave 1 của stage này.")]
    [Min(0f)] public float baseSpawnInterval;

    [Tooltip("Mỗi wave sẽ giảm spawn interval bao nhiêu giây trong stage này.")]
    [Min(0f)] public float spawnIntervalDecayPerWave;

    [Tooltip("Spawn interval thấp nhất để tránh spawn quá dày.")]
    [Min(0.05f)] public float minSpawnInterval;

    [Tooltip("Cứ bao nhiêu wave thì gặp boss trong stage này.")]
    [Min(1)] public int bossWaveFrequency;

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
    [Tooltip("Danh sách các entry stage. Không cần khai báo liên tục từng stage —\n" +
             "chỉ cần khai báo những stage nào có thay đổi (map mới / config mới),\n" +
             "các stage ở giữa sẽ tự dùng lại entry gần nhất phía trước (theo fromStage).")]
    public List<StageData> stages = new();

    /// <summary>
    /// Lấy entry áp dụng cho đúng stageNumber này: entry có fromStage lớn nhất
    /// nhưng vẫn <= stageNumber. Nếu vượt quá entry cuối cùng thì giữ nguyên entry cuối
    /// (không lặp vòng lại từ đầu).
    /// </summary>
    public StageData GetOrLast(int stageNumber)
    {
        if (stages == null || stages.Count == 0) return null;

        StageData best = null;

        foreach (var s in stages)
        {
            if (s == null) continue;

            if (s.fromStage <= stageNumber)
            {
                if (best == null || s.fromStage > best.fromStage)
                    best = s;
            }
        }

        // Nếu stageNumber nhỏ hơn cả entry đầu tiên (ví dụ thiếu entry fromStage = 1),
        // fallback về entry có fromStage nhỏ nhất để không trả về null.
        if (best == null)
            best = stages.Where(s => s != null).OrderBy(s => s.fromStage).FirstOrDefault();

        return best;
    }

    /// <summary>
    /// Lấy đúng entry khớp chính xác fromStage == stageNumber (không fallback).
    /// Trả về null nếu không có entry nào khai báo đúng stage này.
    /// </summary>
    public StageData Get(int stageNumber)
    {
        if (stages == null) return null;
        return stages.FirstOrDefault(s => s != null && s.fromStage == stageNumber);
    }
}
