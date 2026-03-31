using UnityEngine;

public class QuestArrow : MonoBehaviour
{
    private Transform cachedPlayer;
    private Transform target;

    [SerializeField] private float minDistanceToShow = 0.5f;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        FindPlayer();
    }

    void OnEnable()
    {
        if (cachedPlayer == null)
            FindPlayer();

        if (target != null)
            UpdateDirection();
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            cachedPlayer = playerObj.transform;
            return;
        }

        Debug.LogError("[QuestArrow] Không tìm thấy Player!");
    }

    private Transform PlayerTransform
    {
        get
        {
            if (cachedPlayer == null || !cachedPlayer.gameObject.activeInHierarchy)
                FindPlayer();
            return cachedPlayer;
        }
    }

    public void SetTarget(Transform t)
    {
        target = t;

        if (target == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (PlayerTransform != null)
            UpdateDirection();
    }

    void Update()
    {
        var player = PlayerTransform;

        if (player == null || target == null || !target.gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            return;
        }

        UpdateDirection();
    }

    private void UpdateDirection()
    {
        var player = PlayerTransform;
        if (player == null || target == null) return;

        Vector3 dir = target.position - player.position;
        dir.z = 0;

        float sqrDist = dir.sqrMagnitude;

        if (sqrDist > minDistanceToShow * minDistanceToShow)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + 90f);
        }
    }
}