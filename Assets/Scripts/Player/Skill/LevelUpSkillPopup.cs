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
    [Tooltip("Card scale-in duration.")]
    public float cardEntranceDuration = 0.4f;
    [Tooltip("Delay between cards for staggered entrance.")]
    public float cardEntranceDelay = 0.08f;
    [Tooltip("Initial card scale before the pop-in animation.")]
    public float cardEntranceStartScale = 0.7f;
    public Ease cardEntranceEase = Ease.OutBack;

    [Tooltip("Punch strength when a card is selected.")]
    public float selectPunchScale = 0.15f;
    public float selectPunchDuration = 0.3f;

    [Tooltip("Max scale while the Confirm button pulses.")]
    public float confirmPulseScale = 1.08f;
    public float confirmPulseDuration = 0.6f;

    [Header("Confirm Flourish Settings")]
    [Tooltip("Confirm button punch strength when clicked.")]
    public float confirmPunchScale = 0.25f;
    public float confirmPunchDuration = 0.35f;
    [Tooltip("Scale applied to the selected card on confirm.")]
    public float confirmSelectedScale = 1.15f;
    [Tooltip("Scale applied to unselected cards on confirm.")]
    public float confirmLoserScale = 0.8f;
    public float confirmCardAnimDuration = 0.35f;
    [Tooltip("Delay before applying the skill and closing the popup.")]
    public float confirmDelayBeforeHide = 0.35f;

    private List<SkillData> currentSkills = new List<SkillData>();
    private int selectedIndex = -1;
    private Tween confirmPulseTween;
    private int currentPlayerLevel = 1;

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
        currentPlayerLevel = Mathf.Max(1, newLevel);

        if (levelText != null)
            levelText.text = $"LEVEL UP - LEVEL {newLevel}";

        Time.timeScale = 0f;
        Show();

        ForceUnscaledTweens();

        RerollSkills();
    }

    private void ForceUnscaledTweens()
    {
        var tweensOnObject = DOTween.TweensByTarget(gameObject, true);
        if (tweensOnObject != null)
            foreach (var t in tweensOnObject) t?.SetUpdate(true);

        var tweensOnTransform = DOTween.TweensByTarget(transform, true);
        if (tweensOnTransform != null)
            foreach (var t in tweensOnTransform) t?.SetUpdate(true);
    }

    private void RerollSkills()
    {
        SkillSystem skillSystem = CommonReferent.Instance.skill;
        if (skillSystem == null) return;

        List<SkillData> passiveSkills = skillSystem.skillList
            .Where(s => s.skillType == SkillType.Passive)
            .Where(s => s.requiredLevel <= currentPlayerLevel)
            .Where(s => skillSystem.GetSkillLevel(s.skillID) < s.maxLevel)
            .ToList();

        if (passiveSkills.Count == 0)
        {
            passiveSkills = skillSystem.skillList
                .Where(s => s.skillType == SkillType.Passive)
                .Where(s => skillSystem.GetSkillLevel(s.skillID) < s.maxLevel)
                .ToList();
        }

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

        if (rerollButton != null) rerollButton.interactable = true;

        for (int i = 0; i < 3; i++)
            if (skillDisplays[i] != null)
                skillDisplays[i].gameObject.SetActive(false);

        for (int i = 0; i < currentSkills.Count; i++)
        {
            if (skillDisplays[i] != null)
            {
                skillDisplays[i].gameObject.SetActive(true);
                skillDisplays[i].ForceReset();
                skillDisplays[i].DisplaySkill(currentSkills[i], i, OnSkillClicked);

                var tweens = DOTween.TweensByTarget(skillDisplays[i].gameObject, true);
                if (tweens != null)
                    foreach (var t in tweens) t?.SetUpdate(true);

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

    public override void Hide()
    {
        if (selectedIndex < 0 && currentSkills.Count > 0)
        {
            int randomIndex = Random.Range(0, currentSkills.Count);
            ApplySkill(currentSkills[randomIndex]);
            Debug.Log($"[LevelUp] Auto-selected at random: {currentSkills[randomIndex].skillName}");
        }

        if (confirmPulseTween != null)
        {
            confirmPulseTween.Kill();
            confirmPulseTween = null;
        }
        if (confirmButton != null)
            confirmButton.transform.localScale = Vector3.one;

        ForceUnscaledTweens();

        Time.timeScale = 1f;
        base.Hide();

        ForceUnscaledTweens();
    }

    private void ConfirmSelectedSkill()
    {
        if (selectedIndex < 0 || selectedIndex >= currentSkills.Count) return;

        SkillData chosen = currentSkills[selectedIndex];

        if (confirmButton != null) confirmButton.interactable = false;
        if (rerollButton  != null) rerollButton.interactable  = false;

        if (confirmPulseTween != null)
        {
            confirmPulseTween.Kill();
            confirmPulseTween = null;
        }

        if (confirmButton != null)
        {
            confirmButton.transform.DOKill();
            confirmButton.transform.localScale = Vector3.one;
            confirmButton.transform.DOPunchScale(Vector3.one * confirmPunchScale, confirmPunchDuration, 10, 1f)
                .SetUpdate(true);
        }

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

        DOVirtual.DelayedCall(confirmDelayBeforeHide, () =>
        {
            ApplySkill(chosen);
            UIManager.Instance.HidePopupByType(PopupType.LevelUpSkill);
        }).SetUpdate(true);
    }

    private void ApplySkill(SkillData skill)
    {
        SkillSystem skillSystem = CommonReferent.Instance.skill;
        if (skillSystem == null) return;

        bool success = skillSystem.UnlockSkill(skill.skillID);

        if (success) Debug.Log($"[LevelUp] Applied: {skill.skillName}");
        else         Debug.LogWarning($"[LevelUp] Could not unlock: {skill.skillName}");
    }
}
