// ================= ObjectPooler.cs =================

using System.Collections.Generic;
using UnityEngine;

public interface IPooledObject
{
    void OnObjectSpawn();
}

[System.Serializable]
public class PoolConfig
{
    public string tag;
    public GameObject prefab;
    public int initialSize = 5;
    public bool expandable = true;
}

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    [Header("Pool List")]
    public List<PoolConfig> poolConfigs;

    private readonly Dictionary<string, Queue<GameObject>> poolMap = new();
    private readonly Dictionary<string, PoolConfig> configMap = new();
    private readonly Dictionary<string, Transform> parentMap = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        foreach (var config in poolConfigs)
        {
            CreatePool(config);
        }
    }

    public void CreatePool(PoolConfig config)
    {
        if (poolMap.ContainsKey(config.tag))
        {
            Debug.LogWarning($"Pool '{config.tag}' đã tồn tại.");
            return;
        }

        var queue = new Queue<GameObject>();
        var poolParent = new GameObject($"Pool_{config.tag}").transform;
        poolParent.SetParent(transform);

        for (int i = 0; i < config.initialSize; i++)
        {
            GameObject obj = Instantiate(config.prefab, poolParent);
            obj.SetActive(false);
            queue.Enqueue(obj);
        }

        poolMap[config.tag] = queue;
        configMap[config.tag] = config;
        parentMap[config.tag] = poolParent;
    }

    /// <summary>
    /// Lấy object từ pool (tự tạo pool mới nếu chưa có).
    /// </summary>
    public GameObject Get(string prefabName ,GameObject prefab, Vector3 position, Quaternion rotation, int initSize = 5, bool expandable = true)
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab NULL khi gọi Get!");
            return null;
        }

        string tag = prefab.name;

        // Tạo pool mới nếu chưa có
        if (!poolMap.ContainsKey(tag))
        {
            PoolConfig config = new()
            {
                tag = tag,
                prefab = prefab,
                initialSize = initSize,
                expandable = expandable
            };
            CreatePool(config);
        }

        return SpawnFromPool(tag, position, rotation);
    }

    private GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolMap.TryGetValue(tag, out var queue))
        {
            Debug.LogError($"Không tìm thấy pool '{tag}'!");
            return null;
        }

        var config = configMap[tag];
        GameObject objToSpawn = null;

        foreach (var obj in queue)
        {
            if (obj == null) continue;           
            if (!obj.activeInHierarchy)
            {
                objToSpawn = obj;
                break;
            }
        }


        // Nếu không còn object nào free -> tạo mới nếu cho phép
        if (objToSpawn == null)
        {
            if (config.expandable)
            {
                objToSpawn = Instantiate(config.prefab, parentMap[tag]);
                queue.Enqueue(objToSpawn);
            }
            else
            {
                Debug.LogWarning($"Pool '{tag}' đã hết object và không thể mở rộng!");
                return null;
            }
        }
        
        objToSpawn.transform.SetPositionAndRotation(position, rotation);
        objToSpawn.SetActive(true);

        if (objToSpawn.TryGetComponent<IPooledObject>(out var pooled))
            pooled.OnObjectSpawn();
        

        return objToSpawn;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
    }

    public void ClearAllPools()
    {
        // Duyệt tất cả pool tags
        foreach (var tag in new List<string>(poolMap.Keys))
        {
            // Hủy tất cả object trong pool
            foreach (var obj in poolMap[tag])
            {
                if (obj != null)
                    Destroy(obj);
            }

            // Hủy parent của pool
            if (parentMap.ContainsKey(tag) && parentMap[tag] != null)
                Destroy(parentMap[tag].gameObject);
        }

        // Xóa các dictionary
        poolMap.Clear();
        configMap.Clear();
        parentMap.Clear();
    
        Debug.Log("Đã clear toàn bộ pool!");
    }

}
