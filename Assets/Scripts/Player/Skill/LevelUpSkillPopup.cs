using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    public Color confirmEnabledColor  = new Color(1f, 0.85f, 0.2f, 1f);  // Vàng khớp viền

    private List<SkillData> currentSkills = new List<SkillData>();
    private int selectedIndex = -1;

    protected override void Awake()
    {
        base.Awake();

        if (rerollButton  != null) rerollButton.onClick.AddListener(RerollSkills);
        if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmSelectedSkill);

        if (mainTitleText != null) mainTitleText.text = "Chọn 1 kỹ năng Passive mới";
        if (subTitleText  != null) subTitleText.text  = "Kỹ năng sẽ được áp dụng ngay lập tức";

        SetConfirmButton(false);
    }

    public void ShowLevelUpPopup(int newLevel)
    {
        if (levelText != null)
            levelText.text = $"LEVEL UP — CẤP {newLevel}";

        Show();
        RerollSkills();
    }

    // ====================== REROLL ======================
    private void RerollSkills()
    {
        SkillSystem skillSystem = CommonReferent.Instance.skill;
        if (skillSystem == null) return;

        List<SkillData> passiveSkills = skillSystem.skillList
            .Where(s => s.skillType == SkillType.Passive)
            .ToList();

        if (passiveSkills.Count < 3) return;

        List<SkillData> shuffled = new List<SkillData>(passiveSkills);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int rnd = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[rnd]) = (shuffled[rnd], shuffled[i]);
        }

        currentSkills = shuffled.Take(3).ToList();
        selectedIndex = -1;

        for (int i = 0; i < 3; i++)
        {
            if (skillDisplays[i] != null)
            {
                // FIX: ForceReset trước khi DisplaySkill mới
                skillDisplays[i].ForceReset();
                skillDisplays[i].DisplaySkill(currentSkills[i], i, OnSkillClicked);
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

        SetConfirmButton(true);
    }
    
    private void SetConfirmButton(bool interactable)
    {
        if (confirmButton == null) return;

        confirmButton.interactable = interactable;

        // Đổi màu nút theo trạng thái
        var img = confirmButton.GetComponent<Image>();
        if (img != null)
            img.color = interactable ? confirmEnabledColor : confirmDisabledColor;
    }
    // ====================== OVERRIDE HIDE ======================
    public override void Hide()
    {
        // Nếu chưa chọn skill thì tự random
        if (selectedIndex < 0 && currentSkills.Count > 0)
        {
            int randomIndex = Random.Range(0, currentSkills.Count);
            ApplySkill(currentSkills[randomIndex]);
            Debug.Log($"[LevelUp] Tự động chọn random: {currentSkills[randomIndex].skillName}");
        }

        base.Hide();
    }

// ====================== CONFIRM ======================
    private void ConfirmSelectedSkill()
    {
        if (selectedIndex < 0 || selectedIndex >= currentSkills.Count) return;

        ApplySkill(currentSkills[selectedIndex]);
        UIManager.Instance.HidePopupByType(PopupType.LevelUpSkill);
    }

// ====================== APPLY SKILL (dùng chung) ======================
    private void ApplySkill(SkillData skill)
    {
        SkillSystem skillSystem = CommonReferent.Instance.skill;
        if (skillSystem == null) return;

        bool success = skillSystem.UnlockSkill(skill.skillID);

        if (success) Debug.Log($"[LevelUp] Đã áp dụng: {skill.skillName}");
        else         Debug.LogWarning($"[LevelUp] Không thể unlock: {skill.skillName}");
    }
}