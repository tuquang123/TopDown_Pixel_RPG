using System.Collections.Generic;
using UnityEngine;

public class EnemyTracker : Singleton<EnemyTracker>
{
    private readonly HashSet<EnemyAI> trackedEnemies = new();

    public void Register(EnemyAI enemy)
    {
        if (enemy != null)
            trackedEnemies.Add(enemy);
    }

    public void Unregister(EnemyAI enemy)
    {
        if (enemy != null)
            trackedEnemies.Remove(enemy);
    }

    public IEnumerable<EnemyAI> GetAllEnemies() => trackedEnemies;

    public void ClearAllEnemies()
    {
        foreach (var enemy in trackedEnemies)
        {
            if (enemy == null) continue;
            if (enemy.gameObject.activeInHierarchy)
            {
                enemy.gameObject.SetActive(false);
                enemy.EnemyHealthUI?.HideUI();
            }
        }
        trackedEnemies.Clear();
    }

    public List<EnemyAI> GetEnemiesInRange(Vector2 position, float range)
    {
        float sqrRange = range * range;
        List<EnemyAI> result = new();

        foreach (var enemy in trackedEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            float sqrDist = ((Vector2)enemy.transform.position - position).sqrMagnitude;
            if (sqrDist <= sqrRange)
                result.Add(enemy);
        }

        return result;
    }
}