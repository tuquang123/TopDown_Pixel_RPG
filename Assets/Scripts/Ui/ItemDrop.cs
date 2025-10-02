using UnityEngine;
using DG.Tweening;

public class ItemDrop : MonoBehaviour, IPooledObject
{
    public string itemID;              // ID item trong database
    public int quantity = 1;           // số lượng nhặt được

    [Header("Timings")]
    public float flyDuration = 0.3f;               // bay tung nhẹ ban đầu
    public float autoCollectDelay = 0.5f;          // delay trước khi hút
    public float collectDelayAfterReady = 0.2f;    // delay thêm trước khi hút

    [Header("Collect Settings")]
    public float attractRange = 3f;                // khoảng cách xét hút
    public float pickupDistance = 0.25f;           // khoảng cách chạm player
    public float attractSpeed = 10f;               // tốc độ hút

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

        // kill tween cũ
        flyTween?.Kill();

        // Bay tung ngẫu nhiên khi spawn
        Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.2f, 0.6f), 0f);
        Vector3 targetPos = transform.position + offset;

        flyTween = transform.DOMove(targetPos, flyDuration)
            .SetEase(Ease.OutQuad);
    }

    private void Update()
    {
        if (player == null) return;

        float timeSinceSpawn = Time.time - spawnTime;

        // Sau delay → sẵn sàng hút
        if (!isReadyToCollect && timeSinceSpawn >= autoCollectDelay)
        {
            if (Vector3.Distance(transform.position, player.position) <= attractRange)
            {
                isReadyToCollect = true;
                readyTime = Time.time;
            }
        }

        // delay trước khi hút
        if (isReadyToCollect && !isCollecting && Time.time - readyTime >= collectDelayAfterReady)
        {
            isCollecting = true;
        }

        // đang hút
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
        // Lấy data từ DB
        ItemData itemData = CommonReferent.Instance.itemDatabase.GetItemByID(itemID);
        if (itemData != null)
        {
            ItemInstance itemInstance = new ItemInstance(itemData, quantity);
            Inventory.Instance.AddItem(itemInstance);

            // popup
            RewardPopupManager.Instance.ShowReward(itemData.icon, itemData.itemName, quantity);
            FloatingTextSpawner.Instance.SpawnText("+" + itemData.itemName, transform.position, Color.cyan);

            Debug.Log($"Player picked up: {itemData.itemName} x{quantity}");
        }
        else
        {
            Debug.LogWarning($"ItemDrop: ItemID {itemID} không tồn tại trong database!");
        }

        gameObject.SetActive(false);
    }
}
