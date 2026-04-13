// ================= QuestRewardPopup.cs =================
// Chứa 2 class, Unity vẫn chấp nhận

using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ── class phụ, không cần file riêng ──────────────────────
public class RewardRowUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private TextMeshProUGUI valueText;

    public void Init(Sprite icon, string label, string value)
    {
        iconImage.sprite = icon;
        labelText.text   = label;
        valueText.text   = value;
    }
}

// ── class chính, tên khớp tên file ───────────────────────
public class QuestRewardPopup : BasePopup
{
    [Header("Header")]
    [SerializeField] private TextMeshProUGUI questNameText;

    [Header("Reward List")]
    [SerializeField] private Transform rewardContainer;
    [SerializeField] private RewardRowUI rewardRowPrefab;

    [Header("Confirm")]
    [SerializeField] private Button claimButton;

    private QuestProgress pendingQuestProgress;

    protected override void Awake()
    {
        base.Awake();
        claimButton.onClick.AddListener(OnClaimClicked);
    }

    public void Setup(QuestProgress qp)
    {
        pendingQuestProgress = qp;
        Quest quest = qp.quest;

        questNameText.text = quest.questName;

        foreach (Transform child in rewardContainer)
            Destroy(child.gameObject);

        if (quest.reward.experienceReward > 0)
            SpawnRow(CommonReferent.Instance.iconExp, "EXP", $"+{quest.reward.experienceReward}");

        if (quest.reward.goldReward > 0)
            SpawnRow(CommonReferent.Instance.iconGold, "Vàng", $"+{quest.reward.goldReward}");

        foreach (var itemID in quest.reward.itemIDs)
        {
            ItemData item = CommonReferent.Instance.itemDatabase.GetItemByID(itemID);
            if (item == null) continue;
            SpawnRow(item.icon, item.itemName, "x1");
        }
    }

    private void SpawnRow(Sprite icon, string label, string value)
    {
        var row = Instantiate(rewardRowPrefab, rewardContainer);
        row.Init(icon, label, value);
    }

    private void OnClaimClicked()
    {
        if (pendingQuestProgress != null)
            QuestManager.Instance.FinalizeTurnIn(pendingQuestProgress);
        Hide();
    }
}