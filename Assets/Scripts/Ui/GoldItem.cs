using UnityEngine;
using DG.Tweening;

public class GoldItem : MonoBehaviour, IPooledObject
{
    public int value = 1;

    [Header("Timings")]
    public float flyDuration = 0.3f;               // thời gian bay ra ban đầu
    public float autoCollectDelay = 0.5f;          // thời gian đứng yên trước khi xét hút
    public float collectDelayAfterReady = 0.2f;    // delay thêm trước khi hút

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

        // Kill tween bay nếu có
        flyTween?.Kill();

        // Vàng bay nhẹ ra hướng ngẫu nhiên
        Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.2f, 0.6f), 0f);
        Vector3 targetPos = transform.position + offset;

        flyTween = transform.DOMove(targetPos, flyDuration)
            .SetEase(Ease.OutQuad);
    }

    private void Update()
    {
        if (player == null) return;

        float timeSinceSpawn = Time.time - spawnTime;

        // Sau thời gian delay, kiểm tra khoảng cách để chuẩn bị hút
        if (!isReadyToCollect && timeSinceSpawn >= autoCollectDelay)
        {
            if (Vector3.Distance(transform.position, player.position) <= attractRange)
            {
                isReadyToCollect = true;
                readyTime = Time.time;
            }
        }

        // Sau delay hút → bắt đầu hút
        if (isReadyToCollect && !isCollecting && Time.time - readyTime >= collectDelayAfterReady)
        {
            isCollecting = true;
        }

        // Đang hút → bay về theo vị trí sống của Player
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
        CurrencyManager.Instance.AddGold(value);
        FloatingTextSpawner.Instance.SpawnText("+" + value, transform.position, Color.yellow);
        gameObject.SetActive(false);
    }
}
