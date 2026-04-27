using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageData
{
    [Header("Info")]
    public string stageName = "Stage 1";

    [Header("Map")]
    [Tooltip("Prefab map sẽ được spawn khi vào stage này")]
    public GameObject mapPrefab;

    [Header("Reward khi clear stage")]
    public int bonusGold = 0;
    public int bonusExp  = 0;
}

[CreateAssetMenu(fileName = "StageDatabase", menuName = "Data/StageDatabase")]
public class StageDataSO : ScriptableObject
{
    [Tooltip("Danh sách stage theo thứ tự. Index 0 = Stage 1, Index 1 = Stage 2, ...")]
    public List<StageData> stages = new();

    /// <summary>Lấy data stage theo số stage (bắt đầu từ 1). Trả về null nếu list rỗng.</summary>
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