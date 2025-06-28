using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour
{
    public GameObject enemyPrefab;
    private GameObject _currentEnemy;
    public float respawnDelay = 10f;

    public GameObject enemyUI;
    public GameObject canvasHp;

    [SerializeField, Range(1, 10)] private int enemyLevel = 1; 

    public void Spawn(EnemyLevelDatabase levelDB)
    {
        if (_currentEnemy != null) return;

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
            GameObject ui = Instantiate(enemyUI, canvasHp.transform, false);
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
