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

    private Dictionary<string, Queue<GameObject>> poolMap = new();
    private Dictionary<string, PoolConfig> configMap = new();
    private Dictionary<string, Transform> parentMap = new();

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

        Queue<GameObject> objectQueue = new();
        Transform poolParent = new GameObject($"Pool_{config.tag}").transform;
        poolParent.SetParent(transform);

        for (int i = 0; i < config.initialSize; i++)
        {
            GameObject obj = Instantiate(config.prefab, poolParent);
            obj.SetActive(false);
            objectQueue.Enqueue(obj);
        }

        poolMap[config.tag] = objectQueue;
        configMap[config.tag] = config;
        parentMap[config.tag] = poolParent;
    }

    public GameObject Get(string tag, GameObject prefab, Vector3 position, Quaternion rotation, int initSize = 1, bool expandable = true)
    {
        string poolTag = prefab.name;  // Sử dụng tên prefab làm tag

        if (!poolMap.ContainsKey(poolTag))
        {
            PoolConfig config = new()
            {
                tag = poolTag,
                prefab = prefab,
                initialSize = initSize,
                expandable = expandable
            };
            CreatePool(config);
        }

        if (configMap[poolTag].prefab == null)
            configMap[poolTag].prefab = prefab;

        return SpawnFromPool(poolTag, position, rotation);
    }

    private GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolMap.TryGetValue(tag, out var queue))
        {
            Debug.LogError($"Không tìm thấy pool với tag '{tag}'!");
            return null;
        }

        if (queue.Count == 0)
        {
            var config = configMap[tag];
            if (config.expandable && config.prefab != null)
            {
                GameObject obj = Instantiate(config.prefab, parentMap[tag]);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }
            else
            {
                Debug.LogWarning($"Pool '{tag}' đã cạn và không thể mở rộng.");
                return null;
            }
        }

        GameObject objToSpawn = queue.Dequeue();
        objToSpawn.SetActive(true);
        objToSpawn.transform.SetPositionAndRotation(position, rotation);

        if (objToSpawn.TryGetComponent<IPooledObject>(out var pooledObj))
            pooledObj.OnObjectSpawn();

        queue.Enqueue(objToSpawn);
        return objToSpawn;
    }

    public void ClearPool(string tag)
    {
        if (!poolMap.ContainsKey(tag)) return;

        foreach (var obj in poolMap[tag])
        {
            Destroy(obj);
        }

        Destroy(parentMap[tag].gameObject);

        poolMap.Remove(tag);
        configMap.Remove(tag);
        parentMap.Remove(tag);
    }
}

