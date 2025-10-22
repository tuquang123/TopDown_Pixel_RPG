using UnityEngine;
using DG.Tweening;

public enum CurrencyType { Gold, Gem }

public class CurrencyItem : MonoBehaviour, IPooledObject
{
    [Header("Currency Settings")]
    public CurrencyType currencyType = CurrencyType.Gold;
    public int value = 1;

    [Header("Timings")]
    public float flyDuration = 0.3f;               // Thời gian bay ra ban đầu
    public float autoCollectDelay = 0.5f;          // Delay trước khi hút
    public float collectDelayAfterReady = 0.2f;    // Thêm delay trước khi hút

    [Header("Collect Settings")]
    public float attractRange = 3f;
    public float pickupDistance = 0.25f;
    public float attractSpeed = 10f;

    private Transform player;
    private Tween flyTween;
    private float spawnTime;
    private float readyTime;

    private bool isReadyToCollect = false;
    private bool isCollecting = false;

    public void OnObjectSpawn()
    {
        if (player == null)
            player = PlayerController.Instance?.transform;

        isReadyToCollect = false;
        isCollecting = false;
        spawnTime = Time.time;

        flyTween?.Kill();

        // Bay ra hướng ngẫu nhiên khi spawn
        Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.2f, 0.6f), 0f);
        Vector3 targetPos = transform.position + offset;

        flyTween = transform.DOMove(targetPos, flyDuration).SetEase(Ease.OutQuad);
    }

    private void Update()
    {
        if (player == null) return;

        float timeSinceSpawn = Time.time - spawnTime;

        // Chuẩn bị hút
        if (!isReadyToCollect && timeSinceSpawn >= autoCollectDelay)
        {
            if (Vector3.Distance(transform.position, player.position) <= attractRange)
            {
                isReadyToCollect = true;
                readyTime = Time.time;
            }
        }

        // Hút sau delay
        if (isReadyToCollect && !isCollecting && Time.time - readyTime >= collectDelayAfterReady)
        {
            isCollecting = true;
        }

        // Đang hút
        if (isCollecting)
        {
            Vector3 target = player.position;
            transform.position = Vector3.MoveTowards(transform.position, target, attractSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) <= pickupDistance)
            {
                Collect();
            }
        }
    }

    private void Collect()
    {
        switch (currencyType)
        {
            case CurrencyType.Gold:
                CurrencyManager.Instance.AddGold(value);
                FloatingTextSpawner.Instance.SpawnText("+" + value, transform.position, Color.yellow);
                break;

            case CurrencyType.Gem:
                CurrencyManager.Instance.AddGems(value);
                FloatingTextSpawner.Instance.SpawnText("+" + value, transform.position, Color.magenta);
                break;
        }

        gameObject.SetActive(false);
    }
}
