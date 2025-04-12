using System.Collections.Generic;
using UnityEngine;

public interface IPooledObject
{
    void OnObjectSpawn();
}

[System.Serializable]
public class Pool
{
    public string tag;
    public GameObject prefab;
    public int size;
    public bool expandable = true; // Cho phép mở rộng khi hết object
}

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    [Header("Danh sách Pool Khởi Tạo")]
    public List<Pool> pools;

    private Dictionary<string, Queue<GameObject>> poolDictionary = new();
    private Dictionary<string, Pool> poolConfig = new();
    private Dictionary<string, Transform> poolParents = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        foreach (var pool in pools)
        {
            AddPool(pool);
        }
    }
    public GameObject GetToPool(string tag, GameObject prefab, Vector3 position, Quaternion rotation, int initSize = 1, bool expandable = true)
    {
        // Nếu chưa có pool -> tạo mới
        if (!poolDictionary.ContainsKey(tag))
        {
            Pool newPool = new Pool
            {
                tag = tag,
                prefab = prefab,
                size = initSize,
                expandable = expandable
            };
            AddPool(newPool);
        }

        // Nếu prefab chưa được set từ trước
        if (poolConfig[tag].prefab == null)
        {
            poolConfig[tag].prefab = prefab;
        }

        return SpawnFromPool(tag, position, rotation);
    }
    
    public void AddPool(Pool pool)
    {
        if (poolDictionary.ContainsKey(pool.tag))
        {
            Debug.LogWarning($"Pool '{pool.tag}' đã tồn tại!");
            return;
        }

        var objectQueue = new Queue<GameObject>();
        var parent = new GameObject($"Pool_{pool.tag}").transform;
        parent.SetParent(transform);

        for (int i = 0; i < pool.size; i++)
        {
            GameObject obj = Instantiate(pool.prefab, parent);
            obj.SetActive(false);
            objectQueue.Enqueue(obj);
        }

        poolDictionary.Add(pool.tag, objectQueue);
        poolConfig.Add(pool.tag, pool);
        poolParents.Add(pool.tag, parent);
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool '{tag}' chưa có – tạo mới tự động.");
            Pool defaultPool = new Pool { tag = tag, prefab = null, size = 1, expandable = true };
            poolDictionary[tag] = new Queue<GameObject>();
            poolConfig[tag] = defaultPool;
            return null;
        }

        Queue<GameObject> objectQueue = poolDictionary[tag];

        if (objectQueue.Count == 0)
        {
            if (poolConfig[tag].expandable && poolConfig[tag].prefab != null)
            {
                Debug.LogWarning($"Pool '{tag}' hết object – tự động tạo thêm.");
                GameObject newObj = Instantiate(poolConfig[tag].prefab, poolParents[tag]);
                newObj.SetActive(false);
                objectQueue.Enqueue(newObj);
            }
            else
            {
                Debug.LogError($"Pool '{tag}' không còn object và không thể mở rộng.");
                return null;
            }
        }

        GameObject objectToSpawn = objectQueue.Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        if (objectToSpawn.TryGetComponent(out IPooledObject pooledObj))
            pooledObj.OnObjectSpawn();

        objectQueue.Enqueue(objectToSpawn);
        return objectToSpawn;
    }

    // Optional: Clear pool at runtime
    public void ClearPool(string tag)
    {
        if (!poolDictionary.ContainsKey(tag)) return;

        foreach (var obj in poolDictionary[tag])
        {
            Destroy(obj);
        }

        poolDictionary.Remove(tag);
        poolConfig.Remove(tag);
        Destroy(poolParents[tag].gameObject);
        poolParents.Remove(tag);
    }
}
