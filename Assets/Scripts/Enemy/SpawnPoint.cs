using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour
{
    public GameObject enemyPrefab;
    private GameObject _currentEnemy;
    public float respawnDelay = 10f;
    
    [SerializeField, Range(1, 10)] private int enemyLevel = 1; 

    public void Spawn(EnemyLevelDatabase levelDB)
    {
        if (_currentEnemy != null) return;
        
        if (enemyPrefab == null)
        {
            Debug.LogError($"SpawnPoint at {transform.position} is missing enemyPrefab reference!");
            return;
        }
        
        if (ObjectPooler.Instance == null)
        {
            Debug.LogError("ObjectPooler.Instance is NULL! Bạn đã chắc chắn có ObjectPooler trong scene chưa?");
            return;
        }
        
        _currentEnemy = ObjectPooler.Instance.Get(
            enemyPrefab.name, enemyPrefab, transform.position, Quaternion.identity, initSize: 10, expandable: true
        );

        if (_currentEnemy == null) return;

        var ai = _currentEnemy.GetComponent<EnemyAI>();
        var levelData = levelDB.GetDataByLevel(enemyLevel); 
        ai.ApplyLevelData(levelData);
        ai.ResetEnemy();

        if (ai.EnemyHealthUI == null)
        {
            GameObject ui = Instantiate(CommonReferent.Instance.hpSliderUi , CommonReferent.Instance.canvasHp.transform, false);
            var uiComp = ui.GetComponent<EnemyHealthUI>();
            uiComp.SetTarget(_currentEnemy);
            ai.EnemyHealthUI = uiComp;
        }

        ai.OnDeath += () =>
        {
            _currentEnemy = null;
            StartCoroutine(RespawnAfterDelay(levelDB)); 
        };
    }

    private IEnumerator RespawnAfterDelay(EnemyLevelDatabase levelDB)
    {
        yield return new WaitForSeconds(respawnDelay);
        Spawn(levelDB);
    }
}
