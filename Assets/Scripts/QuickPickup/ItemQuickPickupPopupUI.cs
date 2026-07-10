using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemQuickPickupPopupUI : MonoBehaviour
{
    public static ItemQuickPickupPopupUI Instance { get; private set; }

    [Header("Root")]
    public CanvasGroup canvasGroup;
    public RectTransform root;

    [Header("Info")]
    public Image background;
    public Image itemIcon;
    public TMP_Text itemNameText;
    public TMP_Text itemTierText;
    public Image tierGlow; // optional, để trống nếu không dùng

    [Header("Power Preview")]
    public TMP_Text powerDiffText;

    [Header("Stats")]
    public StatDisplayComponent statDisplay;

    [Header("Equip")]
    public Button equipButton;

    [Header("Feature Lock")]
    [SerializeField] private FeatureUnlockData unlockData;
    [SerializeField] private FeatureType requiredFeature = FeatureType.Inventory;
    private PlayerLevel _playerLevel;

    [Header("Timing")]
    public float showDuration = 0.25f;
    public float stayDuration = 20f;
    public float hideDuration = 0.2f;

    private Coroutine currentRoutine;
    private ItemInstance currentItem;

    private void Awake()
    {
        Instance = this;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        root.localScale = Vector3.zero;
        gameObject.SetActive(false);

        _playerLevel = FindObjectOfType<PlayerLevel>();

        if (equipButton != null)
            equipButton.onClick.AddListener(OnEquipClicked);
    }

    /// <summary>
    /// Gọi hàm này ở chỗ nhặt item để hiện popup.
    /// Nếu tính năng (inventory) chưa unlock theo level (theo FeatureUnlockData),
    /// popup sẽ KHÔNG hiện lên.
    /// Nếu popup đang hiện item khác, sẽ bị đè ngay bằng item mới.
    /// </summary>
    public void Show(ItemInstance itemInstance)
    {
        if (!IsFeatureUnlocked())
            return; // chưa đủ level mở inventory → không hiện popup luôn

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentItem = itemInstance;

        gameObject.SetActive(true);
        PopulateData(itemInstance);
        currentRoutine = StartCoroutine(PlaySequence());
    }

    private bool IsFeatureUnlocked()
    {
        if (unlockData == null) return true; // không gán data thì coi như luôn được phép hiện

        var entry = unlockData.Get(requiredFeature);
        if (entry == null) return true;

        if (_playerLevel == null)
            _playerLevel = FindObjectOfType<PlayerLevel>();

        int currentLevel = _playerLevel != null ? _playerLevel.levelSystem.level : 1;
        return currentLevel >= entry.requiredLevel;
    }

    private IEnumerator PlaySequence()
    {
        yield return PlayShowSequence();
        yield return new WaitForSeconds(stayDuration);
        yield return PlayHideSequence();

        gameObject.SetActive(false);
        currentRoutine = null;
    }

    private void PopulateData(ItemInstance itemInstance)
    {
        var data = itemInstance.itemData;

        itemNameText.text = data.itemName;
        if (itemTierText != null)
        {
            itemTierText.text = data.tier.ToString();
            itemTierText.color = ItemUtility.GetColorByTier(data.tier);
        }
        itemIcon.sprite = data.icon;

        if (background != null)
        {
            background.sprite = CommonReferent.Instance.itemTierColorConfig.GetBackground(data.tier);
            background.color = Color.white; // REQUIRED, otherwise sprite bị trong suốt
        }

        if (tierGlow != null)
            tierGlow.color = ItemUtility.GetColorByTier(data.tier);

        // So sánh chỉ số với item đang mặc cùng loại (nếu có)
        var equipmentManager = CommonReferent.Instance.playerPrefab.GetComponent<Equipment>();
        equipmentManager.equippedItems.TryGetValue(data.itemType, out ItemInstance equippedItem);

        statDisplay.SetCompareStats(itemInstance, equippedItem);

        UpdatePowerPreview(itemInstance);
    }

    /// <summary>
    /// Equip tạm item này để tính power sẽ thay đổi bao nhiêu, rồi revert lại ngay.
    /// Toàn bộ diễn ra trong cùng 1 frame (trước khi Unity render) nên không gây
    /// nhấp nháy hình player hay ảnh hưởng gameplay thật.
    /// </summary>
    private void UpdatePowerPreview(ItemInstance itemInstance)
    {
        if (powerDiffText == null) return;

        // Item tiêu hao (potion...) không có khái niệm Power, ẩn dòng preview
        if (itemInstance.itemData.itemType == ItemType.Consumable)
        {
            powerDiffText.text = "";
            return;
        }

        var playerPrefab = CommonReferent.Instance.playerPrefab;
        var equipmentManager = playerPrefab.GetComponent<Equipment>();
        var playerStats = playerPrefab.GetComponent<PlayerStats>();

        if (equipmentManager == null || playerStats == null)
        {
            powerDiffText.text = "";
            return;
        }

        ItemType type = itemInstance.itemData.itemType;
        equipmentManager.equippedItems.TryGetValue(type, out ItemInstance oldItem);

        float beforePower = playerStats.CurrentPower;

        // Equip tạm để lấy power mới
        equipmentManager.EquipItem(itemInstance, playerStats);
        playerStats.CalculatePower();
        float afterPower = playerStats.CurrentPower;

        // Revert lại trạng thái cũ ngay lập tức
        if (oldItem != null)
            equipmentManager.EquipItem(oldItem, playerStats);
        else
            equipmentManager.UnequipItem(type, playerStats);

        playerStats.CalculatePower();

        float diff = afterPower - beforePower;

        if (Mathf.Approximately(diff, 0))
        {
            powerDiffText.text = "Power: +0";
            powerDiffText.color = Color.white;
        }
        else if (diff > 0)
        {
            powerDiffText.text = $"Power: +{diff:N0}";
            powerDiffText.color = new Color(0.2f, 1f, 0.3f);
        }
        else
        {
            powerDiffText.text = $"Power: {diff:N0}";
            powerDiffText.color = new Color(1f, 0.3f, 0.3f);
        }
    }

    private void OnEquipClicked()
    {
        if (currentItem == null)
        {
            Debug.LogWarning("ItemQuickPickupPopupUI: currentItem null, không thể equip.");
            return;
        }

        var playerPrefab = CommonReferent.Instance.playerPrefab;
        var equipmentManager = playerPrefab.GetComponent<Equipment>();
        var playerStats = playerPrefab.GetComponent<PlayerStats>();

        if (equipmentManager == null || playerStats == null)
        {
            Debug.LogWarning("ItemQuickPickupPopupUI: thiếu Equipment/PlayerStats trên playerPrefab.");
            return;
        }

        float beforePower = PlayerStats.Instance.CurrentPower;

        ItemType type = currentItem.itemData.itemType;
        if (equipmentManager.equippedItems.ContainsKey(type))
        {
            ItemInstance old = equipmentManager.UnequipItem(type, playerStats);
            if (old != null)
                Inventory.Instance.AddItem(old);
        }

        GameEvents.OnEquipItemRange.Raise(currentItem.itemData.weaponCategory);

        equipmentManager.EquipItem(currentItem, playerStats);
        Inventory.Instance.RemoveItem(currentItem);

        QuestManager.Instance?.ReportProgressByObjectiveName("EquipItem", 1);

        PlayerStats.Instance.CalculatePower();

        float afterPower = PlayerStats.Instance.CurrentPower;
        ShowPowerDiff(beforePower, afterPower);

        var equipmentUi = FindObjectOfType<EquipmentUI>(true);
        if (equipmentUi != null) equipmentUi.UpdateEquipmentUI();

        var inventoryUi = FindObjectOfType<InventoryUI>(true);
        if (inventoryUi != null) inventoryUi.UpdateInventoryUI();

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        StartCoroutine(HideImmediately());
    }

    private void ShowPowerDiff(float before, float after)
    {
        float diff = after - before;
        if (Mathf.Approximately(diff, 0)) return;

        string text = diff > 0
            ? $"<color=#00FF00>+{diff:N0} Power</color>"
            : $"<color=#FF4D4D>{diff:N0} Power</color>";

        GameEvents.OnShowToast.Raise(text);
    }

    private IEnumerator HideImmediately()
    {
        yield return PlayHideSequence();
        gameObject.SetActive(false);
        currentRoutine = null;
    }

    private IEnumerator PlayShowSequence()
    {
        canvasGroup.DOKill();
        root.DOKill();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        root.localScale = Vector3.one * 0.7f;

        Sequence seq = DOTween.Sequence();
        seq.Join(canvasGroup.DOFade(1f, showDuration));
        seq.Join(root.DOScale(1f, showDuration).SetEase(Ease.OutBack));

        yield return seq.WaitForCompletion();

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private IEnumerator PlayHideSequence()
    {
        canvasGroup.DOKill();
        root.DOKill();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence();
        seq.Join(canvasGroup.DOFade(0f, hideDuration));
        seq.Join(root.DOScale(0.7f, hideDuration).SetEase(Ease.InBack));

        yield return seq.WaitForCompletion();
    }
}