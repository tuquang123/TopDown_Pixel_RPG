// ================= SpawnPoint.cs =================
using System;
using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    [SerializeField, Range(1, 10)] private int enemyLevel = 1;
    public float respawnDelay = 10f;

    [Header("Spawn Conditions")]
    Transform player;
    float spawnRange = 12f;       
    float keepAliveRange = 20f;   

    private GameObject _currentEnemy;
    private bool _waitingRespawn = false;

    private void Start()
    {
        player = CommonReferent.Instance.playerPrefab.transform;
        spawnRange = CommonReferent.Instance.spawnRange;
        keepAliveRange = CommonReferent.Instance.keepAliveRange;
    }

    private void Update()
    {
        if (player == null) return;

        if (_currentEnemy == null && !_waitingRespawn)
        {
            TrySpawn();
            return;
        }

        if (_currentEnemy != null)
        {
            float dist = Vector3.Distance(player.position, transform.position);
            if (dist > keepAliveRange)
            {
                ForceDespawn();
            }
        }
    }
    
    public void ResetSpawnPoint()
    {
        if (_currentEnemy != null)
        {
            ObjectPooler.Instance.ReturnToPool(_currentEnemy);
            _currentEnemy = null;
        }
        _waitingRespawn = false;
    }


    private void TrySpawn()
    {
        float dist = Vector3.Distance(player.position, transform.position);
        if (dist > spawnRange) return;
        Spawn(CommonReferent.Instance.enemyLevelDatabase);
    }

    private void ForceDespawn()
    {
        if (_currentEnemy != null)
        {
            ObjectPooler.Instance.ReturnToPool(_currentEnemy);
            _currentEnemy = null;
        }
    }

    public void Spawn(EnemyLevelDatabase levelDB)
    {
        if (_currentEnemy != null) return;
        if (this == null) return;
        
        _currentEnemy = Instantiate(enemyPrefab, transform.position,
            Quaternion.identity , gameObject.transform);

        if (_currentEnemy == null) return;

        var ai = _currentEnemy.GetComponent<EnemyAI>();
        var levelData = levelDB.GetDataByLevel(enemyLevel);
        ai.ApplyLevelData(levelData);
        ai.ResetEnemy();  // Set currentHealth = maxHealth
        
        GameObject uiObj = Instantiate(CommonReferent.Instance.hpSliderUi, _currentEnemy.transform.position, Quaternion.identity);
        uiObj.transform.SetParent(CommonReferent.Instance.canvasHp.transform, false);
        uiObj.transform.localScale = Vector3.one;
        
        ai.EnemyHealthUI = uiObj.GetComponent<EnemyHealthUI>();
        ai.EnemyHealthUI.SetTarget(_currentEnemy);
    
        ai.OnDeath += () =>
        {
            _currentEnemy = null;
            StartCoroutine(RespawnDelayRoutine());
        };
    }

    private IEnumerator RespawnDelayRoutine()
    {
        if (this == null || !gameObject)
            yield break;

        _waitingRespawn = true;
        yield return new WaitForSeconds(respawnDelay);
        _waitingRespawn = false;
    }
   
}
