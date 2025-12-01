using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Optimization")]
    public float cullingRange = 15f; // tầm hiển thị enemy so với player

    private void Update()
    {
        if (PlayerController.Instance == null) return;
        // 1. Enable/disable enemy theo khoảng cách với player
        if (PlayerController.Instance != null)
        {
            Vector2 playerPos = PlayerController.Instance.transform.position;
            EnemyTracker.Instance.SetEnemiesActiveInRange(playerPos, cullingRange, true);
        }

        // 2. Batch update enemy
        EnemyTracker.Instance.UpdateEnemiesBatch();
    }
}