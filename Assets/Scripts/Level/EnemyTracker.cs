using System.Collections.Generic;
using UnityEngine;

public class EnemyTracker : Singleton<EnemyTracker>
{
    private readonly HashSet<EnemyAI> trackedEnemies = new();

    [Header("Culling Settings")]
    [Tooltip("Khoảng cách tối thiểu để kiểm tra culling (tránh tính toán quá thường xuyên)")]
    public float cullingCheckInterval = 0.25f;
    private float cullingTimer = 0f;
    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        cullingTimer += Time.deltaTime;
        if (cullingTimer >= cullingCheckInterval)
        {
            UpdateCulling();
            cullingTimer = 0f;
        }
    }

    /// <summary>
    /// Đăng ký enemy khi spawn / bật active
    /// </summary>
    public void Register(EnemyAI enemy)
    {
        if (enemy != null)
            trackedEnemies.Add(enemy);
    }

    /// <summary>
    /// Hủy đăng ký khi enemy tắt / pooling
    /// </summary>
    public void Unregister(EnemyAI enemy)
    {
        if (enemy != null)
            trackedEnemies.Remove(enemy);
    }

    /// <summary>
    /// Lấy toàn bộ enemy còn sống
    /// </summary>
    public IEnumerable<EnemyAI> GetAllEnemies()
    {
        foreach (var e in trackedEnemies)
            if (e != null && !e.IsDead)
                yield return e;
    }

    /// <summary>
    /// Lấy enemy trong range (2D)
    /// </summary>
    public List<EnemyAI> GetEnemiesInRange(Vector2 position, float range)
    {
        float sqrRange = range * range;
        List<EnemyAI> result = new();
        foreach (var enemy in trackedEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            if (((Vector2)enemy.transform.position - position).sqrMagnitude <= sqrRange)
                result.Add(enemy);
        }
        return result;
    }

    public List<EnemyAI> GetEnemiesInRange(Vector3 position, float range)
        => GetEnemiesInRange((Vector2)position, range);

    /// <summary>
    /// Tắt tất cả enemy (dùng khi chuyển scene hoặc reset)
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (var enemy in trackedEnemies)
        {
            if (enemy == null) continue;
            if (enemy.gameObject.activeInHierarchy)
            {
                enemy.gameObject.SetActive(false);
                enemy.EnemyHealthUI?.HideUI();
            }
        }
        trackedEnemies.Clear();
    }

    /// <summary>
    /// Update culling cho enemy ngoài camera
    /// </summary>
    private void UpdateCulling()
    {
        if (mainCam == null) return;

        foreach (var enemy in trackedEnemies)
        {
            if (enemy == null) continue;

            Vector3 viewportPos = mainCam.WorldToViewportPoint(enemy.transform.position);
            bool visible = viewportPos.z > 0 && viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;

            // tắt/bật SpriteRenderer
            if (enemy.TryGetComponent<SpriteRenderer>(out var sr))
                sr.enabled = visible;

            // tắt/bật Animator
            if (enemy.TryGetComponent<Animator>(out var anim))
                anim.enabled = visible;
        }
    }
}
