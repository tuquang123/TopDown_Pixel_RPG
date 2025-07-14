using UnityEngine;

public static class GoldDropHelper
{
    public static void SpawnGoldBurst(Vector3 centerPos, int goldCount, GameObject goldPrefab)
    {
        for (int i = 0; i < goldCount; i++)
        {
            // Lệch vị trí spawn một chút
            Vector3 offset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.1f, 0.3f), 0f);
            Vector3 spawnPos = centerPos + offset;

            // Lấy từ pool
            ObjectPooler.Instance.Get(goldPrefab.name, goldPrefab, spawnPos, Quaternion.identity);
        }
    }
}