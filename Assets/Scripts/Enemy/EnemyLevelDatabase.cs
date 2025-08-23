using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif

[CreateAssetMenu(menuName = "GameData/Enemy Level Database", fileName = "EnemyLevelDatabase")]
public class EnemyLevelDatabase : ScriptableObject
{
#if UNITY_EDITOR
    [ListDrawerSettings(Expanded = true, ShowIndexLabels = true)]
#endif
    public List<EnemyLevelData> levels = new();

    public EnemyLevelData GetDataByLevel(int level)
    {
        foreach (var data in levels)
        {
            if (data.level == level)
                return data;
        }

        Debug.LogWarning($"Không tìm thấy dữ liệu cho level {level}");
        return null;
    }
}