using UnityEngine;
using DG.Tweening;

public enum CurrencyType { Gold, Gem }

public class CurrencyItem : MonoBehaviour, IPooledObject
{
    [Header("Currency")]
    public CurrencyType currencyType = CurrencyType.Gold;
    public int value = 1;

    [Header("Visual")]
    [Range(0.1f, 2f)]
    public float coinScale = 0.5f;

    [Header("Burst")]
    public int burstCount = 5;
    public float scatterRadius = 0.45f;
    public float scatterDuration = 0.18f;
    public float flyDelay = 0.1f;

    [Header("Fly To UI")]
    public float flyDuration = 0.4f;
    public float arcHeight = 0.5f;

    // Set bởi beforeSpawn trước khi OnObjectSpawn chạy
    [HideInInspector] public bool isBurstCoin = false;

    private Tween moveTween;
    private Tween delayTween;
    private bool isCollecting;

    public void OnObjectSpawn()
    {
        isCollecting = false;
        moveTween?.Kill();
        delayTween?.Kill();

        transform.localScale = Vector3.one * coinScale;
        transform.rotation = Quaternion.identity;

        if (!isBurstCoin) return; // coin gốc chờ trigger

        // ── Burst coin: scatter ra → delay → fly lên UI ──
        Vector3 startPos = transform.position;
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float dist  = Random.Range(scatterRadius * 0.4f, scatterRadius);

        Vector3 scatterTarget = startPos + new Vector3(
            Mathf.Cos(angle),
            Mathf.Sin(angle) * 0.7f,
            0f) * dist;

        // Pop scale
        transform.localScale = Vector3.zero;
        transform.DOScale(coinScale, scatterDuration * 0.5f).SetEase(Ease.OutBack);

        // Di chuyển tỏa ra
        moveTween = transform
            .DOMove(scatterTarget, scatterDuration)
            .SetEase(Ease.OutQuad);

        // Sau khi scatter xong → bay lên UI
        delayTween = DOVirtual.DelayedCall(
            scatterDuration + flyDelay,
            () => { if (gameObject.activeInHierarchy) FlyToUI(); });
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isBurstCoin) return;  // burst coin tự handle, không trigger
        if (isCollecting) return;
        if (!other.CompareTag("Player")) return;

        isCollecting = true;
        AddCurrency();
        SpawnBurstCoins();
        gameObject.SetActive(false); // coin gốc ẩn ngay
    }

    private void SpawnBurstCoins()
    {
        string tag = gameObject.name.Replace("(Clone)", "").Trim();

        for (int i = 0; i < burstCount; i++)
        {
            ObjectPooler.Instance.SpawnFromPool(
                tag,
                transform.position,
                Quaternion.identity,
                obj =>
                {
                    if (obj.TryGetComponent<CurrencyItem>(out var coin))
                        coin.isBurstCoin = true;
                });
        }
    }

    private void FlyToUI()
    {
        moveTween?.Kill();

        Vector3 start  = transform.position;
        Vector3 target = currencyType == CurrencyType.Gold
            ? CurrencyUI.Instance.GoldTargetWorldPos
            : CurrencyUI.Instance.GemTargetWorldPos;

        Vector3 mid = (start + target) * 0.5f
            + Vector3.up   * Random.Range(arcHeight * 0.5f, arcHeight)
            + Vector3.right * Random.Range(-0.2f, 0.2f);

        float duration = flyDuration + Random.Range(-0.06f, 0.06f);

        transform.DOScale(0f, duration).SetEase(Ease.InQuad);

        moveTween = transform
            .DOPath(
                new Vector3[] { start, mid, target },
                duration,
                PathType.CatmullRom)
            .SetEase(Ease.InQuad)
            .OnComplete(OnArriveUI);
    }

    private void AddCurrency()
    {
        switch (currencyType)
        {
            case CurrencyType.Gold:
                CurrencyManager.Instance.AddGold(value);
                break;
            case CurrencyType.Gem:
                CurrencyManager.Instance.AddGems(value);
                break;
        }
    }

    private void OnArriveUI()
    {
        switch (currencyType)
        {
            case CurrencyType.Gold:
                CurrencyUI.Instance.PlayGoldCollectEffect();
                break;
            case CurrencyType.Gem:
                CurrencyUI.Instance.PlayGemCollectEffect();
                break;
        }

        AudioManager.Instance.PlaySFX("PickUp");
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        moveTween?.Kill();
        delayTween?.Kill();
        isBurstCoin  = false; // reset sạch khi về pool
        isCollecting = false;
    }
}