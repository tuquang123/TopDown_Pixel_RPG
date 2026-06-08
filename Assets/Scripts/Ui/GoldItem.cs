using UnityEngine;
using DG.Tweening;

public class GoldItem : MonoBehaviour, IPooledObject
{
    public int value = 1;

    [Header("Visual")]
    public float goldScale = 0.5f;

    [Header("Spawn Effect")]
    public float bounceDuration = 0.15f;
    public float bounceDistance = 0.2f;

    [Header("Auto Collect")]
    public float autoCollectDelay = 0.4f;

    [Header("Fly To UI")]
    public float flyToUIDuration = 0.45f;

    private Tween moveTween;
    private Tween autoCollectTween;
    private bool isFlyingToUI;

    public void OnObjectSpawn()
    {
        isFlyingToUI = false;

        moveTween?.Kill();
        autoCollectTween?.Kill();

        transform.localScale = Vector3.one * goldScale;
        transform.rotation = Quaternion.identity;

        Vector3 startPos = transform.position;

        Vector2 randomDir = Random.insideUnitCircle.normalized;

        Vector3 scatterOffset =
            new Vector3(
                randomDir.x,
                randomDir.y,
                0f
            ) * Random.Range(0.05f, bounceDistance);

        moveTween = transform
            .DOMove(startPos + scatterOffset, bounceDuration)
            .SetEase(Ease.OutQuad);

        autoCollectTween = DOVirtual.DelayedCall(
            autoCollectDelay,
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

        moveTween?.Kill();
        autoCollectTween?.Kill();

        Vector3 target = CurrencyUI.Instance.GoldTargetWorldPos;

        moveTween = transform
            .DOMove(
                target,
                flyToUIDuration + Random.Range(-0.05f, 0.05f)
            )
            .SetEase(Ease.InQuad)
            .OnComplete(Collect);
    }

    private void Collect()
    {
        CurrencyManager.Instance.AddGold(value);

        CurrencyUI.Instance.PlayGoldCollectEffect();

        AudioManager.Instance.PlaySFX("PickUp");

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        moveTween?.Kill();
        autoCollectTween?.Kill();
    }
}
