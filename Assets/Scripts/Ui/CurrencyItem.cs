using UnityEngine;
using DG.Tweening;

public enum CurrencyType { Gold, Gem }

public class CurrencyItem : MonoBehaviour, IPooledObject
{
    [Header("Currency Settings")]
    public CurrencyType currencyType = CurrencyType.Gold;
    public int value = 1;

    [Header("Spawn")]
    public float bounceDuration = 0.4f;
    public float bounceDistance = 1.5f;

    [Header("Auto Collect")]
    public float autoCollectDelay = 0.6f;
    public float attractRange = 4f;

    [Header("Fly To UI")]
    public float flyToUIDuration = 0.7f;   // chậm rãi
    public float arcHeight = 1.5f;

    private Tween flyTween;
    private float spawnTime;
    private bool isFlyingToUI = false;
    private Transform player;

    public void OnObjectSpawn()
    {
        if (player == null)
            player = PlayerController.Instance?.transform;

        isFlyingToUI = false;
        spawnTime = Time.time;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;

        flyTween?.Kill();

        // Bounce ra ngẫu nhiên, giữ nguyên kích thước
        Vector2 dir = Random.insideUnitCircle.normalized;
        Vector3 offset = new Vector3(dir.x, Mathf.Abs(dir.y) * 0.5f + 0.3f, 0f)
                         * Random.Range(1f, bounceDistance);

        flyTween = transform
            .DOMove(transform.position + offset, bounceDuration)
            .SetEase(Ease.OutQuad);
    }

    private void Update()
    {
        if (isFlyingToUI || player == null) return;
        if (Time.time - spawnTime < autoCollectDelay) return;

        if (Vector3.Distance(transform.position, player.position) <= attractRange)
            StartFlyToUI();
    }

    private void StartFlyToUI()
    {
        if (isFlyingToUI) return;
        isFlyingToUI = true;
        flyTween?.Kill();

        Vector3 start  = transform.position;
        Vector3 target = currencyType == CurrencyType.Gold
            ? CurrencyUI.Instance.GoldTargetWorldPos
            : CurrencyUI.Instance.GemTargetWorldPos;

        // Arc đơn giản
        Vector3 mid = (start + target) * 0.5f + Vector3.up * arcHeight;

        flyTween = transform
            .DOPath(new[] { mid, target }, flyToUIDuration, PathType.CatmullRom)
            .SetEase(Ease.InOutSine)   // chậm → nhanh dần → chậm
            .OnComplete(Collect);
    }

    private void Collect()
    {
        switch (currencyType)
        {
            case CurrencyType.Gold:
                CurrencyManager.Instance.AddGold(value);
                CurrencyUI.Instance.PlayGoldCollectEffect();
                break;
            case CurrencyType.Gem:
                CurrencyManager.Instance.AddGems(value);
                CurrencyUI.Instance.PlayGemCollectEffect();
                break;
        }
        AudioManager.Instance.PlaySFX("PickUp");
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        flyTween?.Kill();
    }
}