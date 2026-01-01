// ================= EnemyTracker.cs =================
using System.Collections.Generic;
using UnityEngine;
using VHierarchy.Libs;

public class EnemyTracker : Singleton<EnemyTracker>
{
    private readonly HashSet<EnemyAI> trackedEnemies = new();

    // Đăng ký enemy
    public void Register(EnemyAI enemy)
    {
        if (enemy != null)
            trackedEnemies.Add(enemy);
    }

    // Hủy đăng ký
    public void Unregister(EnemyAI enemy)
    {
        if (enemy != null)
            trackedEnemies.Remove(enemy);
    }

    // Lấy tất cả enemy
    public IEnumerable<EnemyAI> GetAllEnemies() => trackedEnemies;

    // Lấy enemy trong range (sqrMagnitude)
    public List<EnemyAI> GetEnemiesInRange(Vector2 position, float range)
    {
        float sqrRange = range * range;
        List<EnemyAI> result = new();

        foreach (var enemy in trackedEnemies)
        {
            if (enemy == null || enemy.IsDead || !enemy.gameObject.activeInHierarchy) continue;

            float sqrDist = ((Vector2)enemy.transform.position - position).sqrMagnitude;
            if (sqrDist <= sqrRange)
                result.Add(enemy);
        }

        return result;
    }

    // Tắt/bật enemy theo range
    public void SetEnemiesActiveInRange(Vector2 position, float range, bool active)
    {
        float sqrRange = range * range;
        foreach (var enemy in trackedEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;

            float sqrDist = ((Vector2)enemy.transform.position - position).sqrMagnitude;
            if (sqrDist <= sqrRange)
            {
                enemy.SetActiveForOptimization(active);
            }
            else
            {
                enemy.SetActiveForOptimization(!active); // ví dụ, tắt khi quá xa
            }
        }
    }
    
    public void ClearAllEnemies()
    {
        if (trackedEnemies.Count == 0) return;
        
        EnemyAI[] snapshot = new EnemyAI[trackedEnemies.Count];
        trackedEnemies.CopyTo(snapshot);

        foreach (var enemy in snapshot)
        {
            if (enemy == null) continue;

            //if (enemy.gameObject.activeInHierarchy)
            {
                //enemy.gameObject.SetActive(false);
                enemy.gameObject.Destroy();
                enemy.EnemyHealthUI?.HideUI();
            }
        }

        trackedEnemies.Clear();
    }
    
    // Batch update enemy để giảm spike CPU
    private int updateIndex = 0;
    private int batchSize = 10;

    public void UpdateEnemiesBatch()
    {
        if (trackedEnemies.Count == 0) return;

        var enemiesArray = new EnemyAI[trackedEnemies.Count];
        trackedEnemies.CopyTo(enemiesArray);

        for (int i = 0; i < batchSize; i++)
        {
            updateIndex++;
            if (updateIndex >= enemiesArray.Length) updateIndex = 0;

            var enemy = enemiesArray[updateIndex];
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                enemy.OptimizedUpdate();
            }
        }
    }
}
