using UnityEngine;
using DG.Tweening;

public class GoldItem : MonoBehaviour, IPooledObject
{
    public int value = 1;

    [Header("Timings")]
    public float flyDuration = 0.3f;               // thời gian bay ra ban đầu
    public float autoCollectDelay = 0.5f;          // thời gian đứng yên trước khi bắt đầu xét hút
    public float collectDelayAfterReady = 0.2f;    // delay thêm trước khi hút

    [Header("Collect Settings")]
    public float attractRange = 3f;
    public float pickupDistance = 0.3f;
    public float tweenDuration = 0.3f;

    private Transform player;
    private Tween flyTween;
    private Tween collectTween;

    private float spawnTime;
    private float readyTime;
    private bool isCollecting = false;
    private bool isReadyToCollect = false;

    public void OnObjectSpawn()
    {
        if (player == null)
            player = PlayerController.Instance?.transform;

        // Reset trạng thái
        isCollecting = false;
        isReadyToCollect = false;
        spawnTime = Time.time;

        // Kill tween cũ nếu có
        flyTween?.Kill();
        collectTween?.Kill();

        // Bay ngẫu nhiên khi spawn
        Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.2f, 0.6f), 0f);
        Vector3 targetPos = transform.position + offset;

        flyTween = transform.DOMove(targetPos, flyDuration)
            .SetEase(Ease.OutQuad);
    }

    private void Update()
    {
        if (player == null || isCollecting) return;

        float timeSinceSpawn = Time.time - spawnTime;

        // Đủ delay, kiểm tra có thể hút không
        if (!isReadyToCollect && timeSinceSpawn >= autoCollectDelay)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= attractRange)
            {
                isReadyToCollect = true;
                readyTime = Time.time;
            }
        }

        // Đủ delay thêm, bắt đầu hút
        if (isReadyToCollect && Time.time - readyTime >= collectDelayAfterReady)
        {
            StartCollecting();
            isReadyToCollect = false;
        }
    }

    private void StartCollecting()
    {
        isCollecting = true;

        collectTween = transform.DOMove(player.position, tweenDuration)
            .SetEase(Ease.InSine)
            .OnUpdate(() =>
            {
                if (Vector2.Distance(transform.position, player.position) <= pickupDistance)
                {
                    Collect();
                    collectTween.Kill();
                }
            })
            .OnComplete(() =>
            {
                if (gameObject.activeSelf) // tránh gọi đúp nếu đã kill ở OnUpdate
                {
                    Collect();
                }
            });
    }

    private void Collect()
    {
        CurrencyManager.Instance.AddGold(value);
        FloatingTextSpawner.Instance.SpawnText("+" + value, transform.position, Color.yellow);
        gameObject.SetActive(false);
    }
}
