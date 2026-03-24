using UnityEngine;

public class QuestArrow : MonoBehaviour
{
    private Transform player;          // Không serialize nữa, tìm động
    [SerializeField] private Transform target;  // Mục tiêu NPC (set từ QuestManager)

    [Header("Optional")]
    [SerializeField] private float minDistanceToShow = 0.5f;  // Không hiển thị nếu quá gần (tùy chọn)

    void Awake()
    {
        FindPlayer();  // Tìm ngay khi khởi tạo
    }

    void OnEnable()
    {
        // Khi mũi tên bật lên (sau load level), tìm lại player nếu mất
        if (player == null)
        {
            FindPlayer();
        }

        // Nếu đã có target, update hướng ngay
        if (target != null && player != null)
        {
            UpdateDirection();
        }
    }

    private void FindPlayer()
    {
        // Cách 1: Tìm bằng tag (khuyến nghị, nhanh và ổn định)
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("[QuestArrow] Found Player by tag: " + player.name);
            return;
        }

        // Cách 2: Fallback dùng CommonReferent (nếu bạn có singleton này)
        if (CommonReferent.Instance != null && CommonReferent.Instance.playerPrefab != null)
        {
            // Giả sử CommonReferent lưu instance player hiện tại, hoặc tìm child
            player = CommonReferent.Instance.playerPrefab.transform;  // Nếu là prefab → sai, cần instance
            // Hoặc nếu bạn có field player instance:
            // player = CommonReferent.Instance.player.transform;
        }

        if (player == null)
        {
            Debug.LogError("[QuestArrow] Không tìm thấy Player! Đảm bảo Player có tag 'Player'.");
        }
    }

    public void SetTarget(Transform t)
    {
        target = t;
        gameObject.SetActive(target != null);

        // Update ngay khi set target mới (giúp mượt khi chuyển level)
        if (target != null && player != null)
        {
            UpdateDirection();
        }
    }

    void Update()
    {
        if (player == null || target == null || target.gameObject == null || !target.gameObject.activeInHierarchy)
        {
            // Tắt nếu player hoặc target mất (an toàn khi load level)
            gameObject.SetActive(false);
            return;
        }

        UpdateDirection();
    }

    private void UpdateDirection()
    {
        Vector3 dir = target.position - player.position;
        dir.z = 0;

        float sqrDist = dir.sqrMagnitude;
        if (sqrDist > minDistanceToShow * minDistanceToShow)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + 90f);  // +90 nếu mũi tên sprite hướng lên
        }
        else
        {
            // Quá gần → tùy chọn ẩn hoặc giữ nguyên
            // gameObject.SetActive(false);
        }
    }
}