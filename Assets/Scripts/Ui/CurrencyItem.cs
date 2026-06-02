using UnityEngine;
using DG.Tweening;

public enum CurrencyType
{
    Gold,
    Gem
}

public class CurrencyItem : MonoBehaviour, IPooledObject
{
    [Header("Currency")]
    public CurrencyType currencyType = CurrencyType.Gold;
    public int value = 1;

    [Header("Visual")]
    [Range(0.1f, 2f)]
    public float coinScale = 0.5f; // 50% kích thước

    [Header("Spawn Effect")]
    public float bounceDuration = 0.25f;
    public float bounceDistance = 1.2f;

    [Header("Auto Collect")]
    public float autoCollectDelay = 0.6f;

    [Header("Fly To UI")]
    public float flyToUIDuration = 0.7f;
    public float minArcHeight = 0.8f;
    public float maxArcHeight = 2.0f;
    public float arcRandomX = 1.2f;

    private Tween flyTween;
    private bool isFlyingToUI;

    public void OnObjectSpawn()
    {
        isFlyingToUI = false;

        flyTween?.Kill();

        // Giảm kích thước coin xuống 50%
        transform.localScale = Vector3.one * coinScale;
        transform.rotation = Quaternion.identity;

        Vector3 startPos = transform.position;

        Vector2 randomDir = Random.insideUnitCircle.normalized;

        Vector3 scatterOffset =
            new Vector3(
                randomDir.x,
                Mathf.Abs(randomDir.y) + 0.4f,
                0f
            ) * Random.Range(0.5f, bounceDistance);

        transform.DOMove(startPos + scatterOffset, bounceDuration)
            .SetEase(Ease.OutQuad);

        DOVirtual.DelayedCall(
            autoCollectDelay + Random.Range(0f, 0.15f),
            () =>
            {
                if (gameObject.activeInHierarchy)
                    StartFlyToUI();
            });
    }

    private void StartFlyToUI()
    {
        if (isFlyingToUI)
            return;

        isFlyingToUI = true;

        flyTween?.Kill();

        Vector3 start = transform.position;

        Vector3 target =
            currencyType == CurrencyType.Gold
                ? CurrencyUI.Instance.GoldTargetWorldPos
                : CurrencyUI.Instance.GemTargetWorldPos;

        float randomX = Random.Range(-arcRandomX, arcRandomX);
        float randomY = Random.Range(minArcHeight, maxArcHeight);

        Vector3 mid =
            (start + target) * 0.5f +
            new Vector3(randomX, randomY, 0f);

        flyTween = transform
            .DOPath(
                new Vector3[]
                {
                    start,
                    mid,
                    target
                },
                flyToUIDuration + Random.Range(-0.1f, 0.15f),
                PathType.CatmullRom
            )
            .SetEase(Ease.InQuad)
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