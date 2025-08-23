using UnityEngine;
using System.Collections;

public class DestructibleObject : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHits = 3;
    private int currentHits;

    [Header("Drops")]
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private int minGold = 1;
    [SerializeField] private int maxGold = 3;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeMagnitude = 0.05f;

    private Vector3 originalPos;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        originalPos = transform.localPosition;
    }

    private void OnDisable()
    {
        DestructibleTracker.Instance?.Unregister(this);
    }
    
    private void OnEnable()
    {
        currentHits = 0;
        spriteRenderer.color = Color.white;
        transform.localPosition = originalPos;
        DestructibleTracker.Instance?.Register(this);
    }

    public void Hit()
    {
        currentHits++;

        // Hiệu ứng feedback
        PlayHitFlash();
        StartCoroutine(Shake());
            
       // SpawnHitVFX();

        if (currentHits >= maxHits)
        {
            HandleDestruction();
        }
    }

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

    private void SpawnHitVFX()
    {
        if (CommonReferent.Instance != null)
        {
            ObjectPooler.Instance.Get("HitVFX", CommonReferent.Instance.hitVFXPrefab, transform.position, Quaternion.identity);
        }
    }

    private void HandleDestruction()
    {
        // Spawn break VFX
        if (CommonReferent.Instance.destructionVFXPrefab != null)
        {
            ObjectPooler.Instance.Get("BreakVFX", CommonReferent.Instance.destructionVFXPrefab, transform.position, Quaternion.identity);
        }

        // Spawn gold
        int totalGoldToDrop = Random.Range(minGold, maxGold + 1); // ví dụ từ 3 đến 8
        GoldDropHelper.SpawnGoldBurst(transform.position, totalGoldToDrop, CommonReferent.Instance.goldPrefab);

        gameObject.SetActive(false);
    }
}
