using System.Collections.Generic;

public class EnemyTracker : Singleton<EnemyTracker>
{
    private List<EnemyAI> trackedEnemies = new List<EnemyAI>();

    public void Register(EnemyAI enemy)
    {
        if (!trackedEnemies.Contains(enemy))
            trackedEnemies.Add(enemy);
    }

    public void Unregister(EnemyAI enemy)
    {
        trackedEnemies.Remove(enemy);
    }
    
    public List<EnemyAI> GetAllEnemies()
    {
        return trackedEnemies;
    }
    
    public void ClearAllEnemies()
    {
        foreach (var enemy in trackedEnemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                enemy.gameObject.SetActive(false); 
                enemy.EnemyHealthUI?.HideUI();
            }
        }

        trackedEnemies.Clear();
    }
    
    public List<EnemyAI> GetEnemiesInRange(UnityEngine.Vector2 position, float range)
    {
        List<EnemyAI> result = new List<EnemyAI>();

        foreach (var enemy in trackedEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;

            float dist = UnityEngine.Vector2.Distance(position, enemy.transform.position);
            if (dist <= range)
            {
                result.Add(enemy);
            }
        }

        return result;
    }

}