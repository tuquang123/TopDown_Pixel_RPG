using UnityEngine;
using System.Collections;

public class DestructibleObject : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHits = 3;
    private int currentHits;

    [Header("Drop Settings")]
    [SerializeField] private int minGold = 1;
    [SerializeField] private int maxGold = 3;
    [SerializeField, Range(0f, 1f)] private float gemDropChance = 0.5f; // 30% rơi gem
    [SerializeField] private int minGem = 1;
    [SerializeField] private int maxGem = 2;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeMagnitude = 0.05f;

    private Vector3 originalPos;
    private Coroutine flashCoroutine;
    private string nameOBJ = "barrier";

    private void Awake()
    {
        originalPos = transform.localPosition;
    }

    private void OnEnable()
    {
        currentHits = 0;
        spriteRenderer.color = Color.white;
        transform.localPosition = originalPos;
        DestructibleTracker.Instance?.Register(this);
    }

    private void OnDisable()
    {
        DestructibleTracker.Instance?.Unregister(this);
    }

    // =====================================================
    // Bị đánh
    // =====================================================
    public void Hit()
    {
        currentHits++;

        PlayHitFlash();
        StartCoroutine(Shake());

        if (currentHits >= maxHits)
            HandleDestruction();
    }

    // =====================================================
    // Hiệu ứng nhấp nháy khi bị đánh
    // =====================================================
    private void PlayHitFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    // =====================================================
    // Hiệu ứng rung lắc khi bị đánh
    // =====================================================
    private IEnumerator Shake()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            Vector3 randomOffset = Random.insideUnitCircle * shakeMagnitude;
            transform.localPosition = originalPos + new Vector3(randomOffset.x, randomOffset.y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }

    // =====================================================
    // Phá hủy vật thể
    // =====================================================
    private void HandleDestruction()
    {
        // Hiệu ứng vỡ
        if (CommonReferent.Instance.destructionVFXPrefab != null)
        {
            ObjectPooler.Instance.Get("BreakVFX", CommonReferent.Instance.destructionVFXPrefab, transform.position, Quaternion.identity);
        }

        // Rơi vàng
        int totalGold = Random.Range(minGold, maxGold + 1);
        for (int i = 0; i < totalGold; i++)
        {
            Vector3 offset = Random.insideUnitCircle * 0.5f;
            ObjectPooler.Instance.Get("Gold", CommonReferent.Instance.goldPrefab, transform.position + offset, Quaternion.identity);
        }

        // Rơi gem (theo xác suất)
        if (Random.value < gemDropChance)
        {
            int totalGem = Random.Range(minGem, maxGem + 1);
            for (int i = 0; i < totalGem; i++)
            {
                Vector3 offset = Random.insideUnitCircle * 0.5f;
                ObjectPooler.Instance.Get("Gem", CommonReferent.Instance.gemPrefab, transform.position + offset, Quaternion.identity);
            }
        }

        // Báo nhiệm vụ
        QuestManager.Instance.ReportProgress("NV2", nameOBJ, 1);

        // Ẩn vật thể
        gameObject.SetActive(false);
    }
}
