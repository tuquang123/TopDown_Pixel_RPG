using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour
{
    public GameObject enemyPrefab;
    private GameObject _currentEnemy;
    public float respawnDelay = 10f;

    public GameObject enemyUI;
    public GameObject canvasHp;

    public void Spawn()
    {
        if (_currentEnemy != null) return;

        _currentEnemy = ObjectPooler.Instance.Get(
            enemyPrefab.name, enemyPrefab, transform.position, Quaternion.identity, initSize: 10, expandable: true
        );

        if (_currentEnemy == null) return;

        var ai = _currentEnemy.GetComponent<EnemyAI>();
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
            StartCoroutine(RespawnAfterDelay());
        };
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        Spawn();
    }
}