using System.Collections.Generic;
using UnityEngine;

// ──────────────────────────────────────────────────────────
//  Data của từng stage
// ──────────────────────────────────────────────────────────
[System.Serializable]
public class StageData
{
    [Header("Info")]
    public string stageName = "Stage 1";

    [Header("Map")]
    [Tooltip("Tên scene sẽ load khi vào stage này (phải có trong Build Settings)")]
    public string sceneName;

    [Tooltip("Nếu không dùng scene riêng mà chỉ đổi background/tilemap trong scene hiện tại")]
    public GameObject mapPrefab;

   
    [Header("Waves")]
    [Tooltip("Số wave thường trước khi boss xuất hiện. 0 = dùng bossWaveFrequency của WaveManager")]
    public int waveCount = 0;

    [Header("Reward khi clear stage")]
    public int bonusGold = 0;
    public int bonusExp  = 0;
}

// ──────────────────────────────────────────────────────────
//  ScriptableObject chứa toàn bộ danh sách stage
// ──────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "StageDatabase", menuName = "Data/StageDatabase")]
public class StageDataSO : ScriptableObject
{
    [Tooltip("Danh sách stage theo thứ tự. Index 0 = Stage 1, Index 1 = Stage 2, ...")]
    public List<StageData> stages = new();

    /// <summary>Lấy data stage theo số stage (bắt đầu từ 1). Trả về null nếu vượt quá danh sách.</summary>
    public StageData Get(int stageNumber)
    {
        int index = stageNumber - 1;
        if (index < 0 || index >= stages.Count) return null;
        return stages[index];
    }

    /// <summary>Lấy data stage cuối cùng nếu stageNumber vượt quá danh sách (loop hoặc reuse).</summary>
    public StageData GetOrLast(int stageNumber)
    {
        if (stages == null || stages.Count == 0) return null;
        int index = Mathf.Clamp(stageNumber - 1, 0, stages.Count - 1);
        return stages[index];
    }
}