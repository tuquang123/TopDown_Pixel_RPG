using UnityEngine;
using DG.Tweening;

public class ItemDrop : MonoBehaviour, IPooledObject
{
    [Header("Timings")]
    public float flyDuration = 0.3f;               
    public float autoCollectDelay = 0.5f;          
    public float collectDelayAfterReady = 0.2f;    

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

    [Header("UI References")]
    [SerializeField] private SpriteRenderer iconImage;

    // Data của vật phẩm rơi ra
    private ItemInstance itemInstance;
    private int quantity = 1;

    // ======================================================
    // ================== SETUP & SPAWN =====================
    // ======================================================

    /// <summary>
    /// Gọi khi enemy chết để spawn item
    /// </summary>
    public void Setup(ItemInstance instance, int amount)
    {
        itemInstance = instance;
        quantity = amount;

        if (iconImage != null && instance.itemData != null)
            iconImage.sprite = instance.itemData.icon;
    }

    public void OnObjectSpawn()
    {
        if (player == null)
            player = PlayerController.Instance?.transform;

        isReadyToCollect = false;
        isCollecting = false;
        spawnTime = Time.time;

        flyTween?.Kill();

        // Bay nhẹ khi spawn
        Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.2f, 0.6f), 0f);
        Vector3 targetPos = transform.position + offset;

        flyTween = transform.DOMove(targetPos, flyDuration)
            .SetEase(Ease.OutQuad);
    }

    // ======================================================
    // =================== UPDATE ===========================
    // ======================================================

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

        // Delay thêm trước khi hút
        if (isReadyToCollect && !isCollecting && Time.time - readyTime >= collectDelayAfterReady)
        {
            isCollecting = true;
        }

        // Hút về player
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

    // ======================================================
    // ==================== COLLECT =========================
    // ======================================================

    private void Collect()
    {
        if (itemInstance == null || itemInstance.itemData == null)
        {
            Debug.LogWarning("ItemDrop: itemInstance null hoặc không hợp lệ!");
            gameObject.SetActive(false);
            return;
        }

        // Tạo bản sao (để mỗi lần nhặt là 1 instance riêng biệt)
        ItemInstance collected = new ItemInstance(itemInstance.itemData, itemInstance.upgradeLevel);
        Inventory.Instance.AddItem(collected);

        // Hiệu ứng & log
        RewardPopupManager.Instance.ShowReward(itemInstance.itemData.icon, itemInstance.itemData.itemName, quantity);
        FloatingTextSpawner.Instance.SpawnText("+" + itemInstance.itemData.itemName, transform.position, Color.cyan);

        Debug.Log($"Player picked up: {itemInstance.itemData.itemName} x{quantity} (Lv+{itemInstance.upgradeLevel})");

        gameObject.SetActive(false);
    }
}
