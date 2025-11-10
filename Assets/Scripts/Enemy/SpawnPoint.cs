using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    [SerializeField, Range(1, 10)] private int enemyLevel = 1;
    public float respawnDelay = 10f;

    private GameObject _currentEnemy;

    /// <summary>
    /// Spawn enemy tại vị trí spawnpoint, kèm UI máu pool.
    /// </summary>
    public void Spawn(EnemyLevelDatabase levelDB)
    {
        if (_currentEnemy != null) return; // enemy vẫn tồn tại, không spawn thêm

        if (enemyPrefab == null)
        {
            Debug.LogError($"SpawnPoint at {transform.position} is missing enemyPrefab reference!");
            return;
        }

        if (ObjectPooler.Instance == null)
        {
            Debug.LogError("ObjectPooler.Instance is NULL! Hãy chắc chắn có ObjectPooler trong scene!");
            return;
        }

        // Spawn enemy từ pool
        _currentEnemy = ObjectPooler.Instance.Get(
            enemyPrefab.name,
            enemyPrefab,
            transform.position,
            Quaternion.identity,
            initSize: 10,
            expandable: true
        );

        if (_currentEnemy == null) return;

        // Setup level data
        var ai = _currentEnemy.GetComponent<EnemyAI>();
        var levelData = levelDB.GetDataByLevel(enemyLevel);
        ai.ApplyLevelData(levelData);
        ai.ResetEnemy();

        // Setup EnemyHealthUI từ pool
        if (ai.EnemyHealthUI == null)
        {
            GameObject uiObj = ObjectPooler.Instance.Get(
                CommonReferent.Instance.hpSliderUi.name,
                CommonReferent.Instance.hpSliderUi,
                Vector3.zero,
                Quaternion.identity
            );

            uiObj.transform.SetParent(CommonReferent.Instance.canvasHp.transform, false);

            EnemyHealthUI uiComp = uiObj.GetComponent<EnemyHealthUI>();
            uiComp.SetTarget(_currentEnemy);
            ai.EnemyHealthUI = uiComp;
        }
        else
        {
            ai.EnemyHealthUI.SetTarget(_currentEnemy);
            ai.EnemyHealthUI.HideUI();
        }

        // Khi enemy chết
        ai.OnDeath += () =>
        {
            // hide UI và trả về pool
            if (ai.EnemyHealthUI != null)
            {
                ai.EnemyHealthUI.HideUI();
            }

            _currentEnemy = null;
            StartCoroutine(RespawnAfterDelay(levelDB));
        };

        // Spawn dead VFX pool (chuẩn bị)
        ObjectPooler.Instance.Get(
            CommonReferent.Instance.deadVFXPrefab.name,
            CommonReferent.Instance.deadVFXPrefab,
            transform.position,
            Quaternion.identity,
            initSize: 2,
            expandable: true
        );
    }

    private IEnumerator RespawnAfterDelay(EnemyLevelDatabase levelDB)
    {
        yield return new WaitForSeconds(respawnDelay);
        Spawn(levelDB);
    }
}
