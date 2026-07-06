using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LevelUpSkillPopup : BasePopup
{
    [Header("Header Texts")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI mainTitleText;
    public TextMeshProUGUI subTitleText;

    [Header("3 Skill Displays")]
    public SkillDisplayUI[] skillDisplays = new SkillDisplayUI[3];

    [Header("Buttons")]
    public Button rerollButton;
    public Button confirmButton;

    [Header("Confirm Button Colors")]
    public Color confirmDisabledColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    public Color confirmEnabledColor  = new Color(1f, 0.85f, 0.2f, 1f);

    [Header("Effect Settings")]
    [Tooltip("Thời gian card scale vào")]
    public float cardEntranceDuration = 0.4f;
    [Tooltip("Delay giữa các card để tạo hiệu ứng so le")]
    public float cardEntranceDelay = 0.08f;
    [Tooltip("Scale bắt đầu của card trước khi nảy vào")]
    public float cardEntranceStartScale = 0.7f;
    public Ease cardEntranceEase = Ease.OutBack;

    [Tooltip("Độ mạnh punch khi chọn card")]
    public float selectPunchScale = 0.15f;
    public float selectPunchDuration = 0.3f;

    [Tooltip("Scale tối đa khi nút Confirm tự pulse")]
    public float confirmPulseScale = 1.08f;
    public float confirmPulseDuration = 0.6f;

    [Header("Confirm Flourish Settings")]
    [Tooltip("Độ mạnh punch của nút Confirm khi bấm")]
    public float confirmPunchScale = 0.25f;
    public float confirmPunchDuration = 0.35f;
    [Tooltip("Card được chọn phóng to lên bao nhiêu khi xác nhận")]
    public float confirmSelectedScale = 1.15f;
    [Tooltip("Card không được chọn thu nhỏ còn bao nhiêu khi xác nhận")]
    public float confirmLoserScale = 0.8f;
    public float confirmCardAnimDuration = 0.35f;
    [Tooltip("Thời gian chờ trước khi thực sự apply skill và đóng popup, để thấy hiệu ứng")]
    public float confirmDelayBeforeHide = 0.35f;

    private List<SkillData> currentSkills = new List<SkillData>();
    private int selectedIndex = -1;
    private Tween confirmPulseTween;

    protected override void Awake()
    {
        base.Awake();

        if (rerollButton  != null) rerollButton.onClick.AddListener(RerollSkills);
        if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmSelectedSkill);

        if (mainTitleText != null) mainTitleText.text = "Choose 1 new Passive skill";
        if (subTitleText  != null) subTitleText.text  = "The skill will be applied immediately";

        SetConfirmButton(false);
    }

    public void ShowLevelUpPopup(int newLevel)
    {
        if (levelText != null)
            levelText.text = $"LEVEL UP — LEVEL {newLevel}";

        Time.timeScale = 0f; // Pause time
        Show();

        // FIX: Show() (kế thừa từ BasePopup) có thể chạy DOTween fade/scale bằng
        // scaled time (mặc định). Khi timeScale = 0, các tween đó sẽ bị đứng hình
        // ngay giữa animation. Ép toàn bộ tween đang chạy trên popup này (và các
        // object con) sang dùng unscaled time để animation show vẫn mượt dù đang pause.
        ForceUnscaledTweens();

        RerollSkills();
    }

    // FIX: quét mọi tween đang chạy trên GameObject này + transform, ép SetUpdate(true)
    private void ForceUnscaledTweens()
    {
        var tweensOnObject = DOTween.TweensByTarget(gameObject, true);
        if (tweensOnObject != null)
            foreach (var t in tweensOnObject) t?.SetUpdate(true);

        var tweensOnTransform = DOTween.TweensByTarget(transform, true);
        if (tweensOnTransform != null)
            foreach (var t in tweensOnTransform) t?.SetUpdate(true);
    }

    // ====================== REROLL ======================
    private void RerollSkills()
    {
        SkillSystem skillSystem = CommonReferent.Instance.skill;
        if (skillSystem == null) return;

        List<SkillData> passiveSkills = skillSystem.skillList
            .Where(s => s.skillType == SkillType.Passive)
            .Where(s => skillSystem.GetSkillLevel(s.skillID) < s.maxLevel) // ← fix
            .ToList();

        // Handle the case where there aren't enough skills to choose from
        if (passiveSkills.Count == 0)
        {
            Debug.Log("[LevelUp] All skills are already max level!");
            UIManager.Instance.HidePopupByType(PopupType.LevelUpSkill);
            return;
        }

        List<SkillData> shuffled = new List<SkillData>(passiveSkills);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int rnd = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[rnd]) = (shuffled[rnd], shuffled[i]);
        }

        currentSkills = shuffled.Take(Mathf.Min(3, shuffled.Count)).ToList();
        selectedIndex = -1;

        // Đảm bảo nút không bị kẹt ở trạng thái disable từ lần confirm trước
        if (rerollButton != null) rerollButton.interactable = true;

        // Hide all 3 slots first
        for (int i = 0; i < 3; i++)
            if (skillDisplays[i] != null)
                skillDisplays[i].gameObject.SetActive(false);

        // Only show as many slots as there are remaining skills
        for (int i = 0; i < currentSkills.Count; i++)
        {
            if (skillDisplays[i] != null)
            {
                skillDisplays[i].gameObject.SetActive(true);
                skillDisplays[i].ForceReset();
                skillDisplays[i].DisplaySkill(currentSkills[i], i, OnSkillClicked);

                // FIX: DisplaySkill có thể tự chạy tween riêng (fade/scale slot).
                // Đảm bảo các tween này cũng không bị đứng khi timeScale = 0.
                var tweens = DOTween.TweensByTarget(skillDisplays[i].gameObject, true);
                if (tweens != null)
                    foreach (var t in tweens) t?.SetUpdate(true);

                // EFFECT: card bay vào so le, nảy nhẹ (OutBack), không phụ thuộc timeScale
                Transform cardTf = skillDisplays[i].transform;
                cardTf.DOKill();
                cardTf.localScale = Vector3.one * cardEntranceStartScale;
                cardTf.DOScale(1f, cardEntranceDuration)
                    .SetEase(cardEntranceEase)
                    .SetDelay(i * cardEntranceDelay)
                    .SetUpdate(true);
            }
        }

        SetConfirmButton(false);
    }

    private void OnSkillClicked(int index)
    {
        selectedIndex = index;

        for (int i = 0; i < 3; i++)
            if (skillDisplays[i] != null)
                skillDisplays[i].SetSelected(i == index);

        // EFFECT: punch scale cho card vừa được chọn
        if (skillDisplays[index] != null)
        {
            Transform cardTf = skillDisplays[index].transform;
            cardTf.DOKill();
            cardTf.localScale = Vector3.one;
            cardTf.DOPunchScale(Vector3.one * selectPunchScale, selectPunchDuration, 8, 0.8f)
                .SetUpdate(true);
        }

        SetConfirmButton(true);
    }

    private void SetConfirmButton(bool interactable)
    {
        if (confirmButton == null) return;

        confirmButton.interactable = interactable;

        var img = confirmButton.GetComponent<Image>();
        if (img != null)
            img.color = interactable ? confirmEnabledColor : confirmDisabledColor;

        // EFFECT: nút Confirm tự pulse nhẹ khi có thể bấm, để hút mắt người chơi
        if (confirmPulseTween != null)
        {
            confirmPulseTween.Kill();
            confirmPulseTween = null;
        }
        confirmButton.transform.localScale = Vector3.one;

        if (interactable)
        {
            confirmPulseTween = confirmButton.transform.DOScale(confirmPulseScale, confirmPulseDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
        }
    }

    // ====================== OVERRIDE HIDE ======================
    public override void Hide()
    {
        // If no skill was selected, pick one at random
        if (selectedIndex < 0 && currentSkills.Count > 0)
        {
            int randomIndex = Random.Range(0, currentSkills.Count);
            ApplySkill(currentSkills[randomIndex]);
            Debug.Log($"[LevelUp] Auto-selected at random: {currentSkills[randomIndex].skillName}");
        }

        // Dọn tween pulse của confirm để tránh leak khi popup ẩn
        if (confirmPulseTween != null)
        {
            confirmPulseTween.Kill();
            confirmPulseTween = null;
        }
        if (confirmButton != null)
            confirmButton.transform.localScale = Vector3.one;

        // FIX: base.Hide() có thể chạy tween fade-out bằng scaled time.
        // Ép unscaled TRƯỚC khi restore timeScale = 1, để nếu Hide() start tween
        // ngay trong lúc timeScale vẫn = 0, nó vẫn chạy được thay vì đứng hình
        // cho tới khi có ai đó vô tình set lại timeScale.
        ForceUnscaledTweens();

        Time.timeScale = 1f; // Restore time
        base.Hide();

        // FIX: phòng trường hợp base.Hide() mới là nơi khởi tạo tween (thay vì
        // trước đó), quét lại lần nữa sau khi gọi base.Hide().
        ForceUnscaledTweens();
    }

    // ====================== CONFIRM ======================
    private void ConfirmSelectedSkill()
    {
        if (selectedIndex < 0 || selectedIndex >= currentSkills.Count) return;

        SkillData chosen = currentSkills[selectedIndex];

        // Chặn bấm nhiều lần trong lúc hiệu ứng đang chạy
        if (confirmButton != null) confirmButton.interactable = false;
        if (rerollButton  != null) rerollButton.interactable  = false;

        // Dừng pulse đang chạy trên nút Confirm
        if (confirmPulseTween != null)
        {
            confirmPulseTween.Kill();
            confirmPulseTween = null;
        }

        // EFFECT: nút Confirm nảy mạnh khi bấm
        if (confirmButton != null)
        {
            confirmButton.transform.DOKill();
            confirmButton.transform.localScale = Vector3.one;
            confirmButton.transform.DOPunchScale(Vector3.one * confirmPunchScale, confirmPunchDuration, 10, 1f)
                .SetUpdate(true);
        }

        // EFFECT: card được chọn bật to lên nổi bật, 2 card còn lại thu nhỏ + mờ dần
        for (int i = 0; i < currentSkills.Count; i++)
        {
            if (skillDisplays[i] == null) continue;

            Transform cardTf = skillDisplays[i].transform;
            cardTf.DOKill();

            if (i == selectedIndex)
            {
                cardTf.DOScale(confirmSelectedScale, confirmCardAnimDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);
            }
            else
            {
                cardTf.DOScale(confirmLoserScale, confirmCardAnimDuration)
                    .SetEase(Ease.InBack)
                    .SetUpdate(true);

                var cg = skillDisplays[i].GetComponent<CanvasGroup>();
                if (cg != null)
                    cg.DOFade(0f, confirmCardAnimDuration).SetUpdate(true);
            }
        }

        // Chờ hiệu ứng chạy xong rồi mới thật sự apply skill + đóng popup
        DOVirtual.DelayedCall(confirmDelayBeforeHide, () =>
        {
            ApplySkill(chosen);
            UIManager.Instance.HidePopupByType(PopupType.LevelUpSkill);
        }).SetUpdate(true);
    }

    // ====================== APPLY SKILL ======================
    private void ApplySkill(SkillData skill)
    {
        SkillSystem skillSystem = CommonReferent.Instance.skill;
        if (skillSystem == null) return;

        bool success = skillSystem.UnlockSkill(skill.skillID);

        if (success) Debug.Log($"[LevelUp] Applied: {skill.skillName}");
        else         Debug.LogWarning($"[LevelUp] Could not unlock: {skill.skillName}");
    }
}