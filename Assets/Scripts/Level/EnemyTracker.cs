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
}