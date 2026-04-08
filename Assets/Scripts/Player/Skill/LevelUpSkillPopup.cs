using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

    private List<SkillData> currentSkills = new List<SkillData>();
    private int selectedIndex = -1;

    protected override void Awake()
    {
        base.Awake();

        if (rerollButton != null)
            rerollButton.onClick.AddListener(RerollSkills);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmSelectedSkill);

        if (mainTitleText != null)
            mainTitleText.text = "Chọn 1 kỹ năng Passive mới";

        if (subTitleText != null)
            subTitleText.text = "Kỹ năng sẽ được áp dụng ngay lập tức";

        if (confirmButton != null)
            confirmButton.interactable = false;
    }

    public void ShowLevelUpPopup(int newLevel)
    {
        if (levelText != null)
            levelText.text = $"LEVEL UP — CẤP {newLevel}";

        Show();
        RerollSkills();
    }

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
                skillDisplays[i].DisplaySkill(currentSkills[i], i, OnSkillClicked);
                skillDisplays[i].SetSelected(false);
            }
        }

        if (confirmButton != null)
            confirmButton.interactable = false;
    }

    private void OnSkillClicked(int index)
    {
        selectedIndex = index;

        for (int i = 0; i < 3; i++)
            if (skillDisplays[i] != null)
                skillDisplays[i].SetSelected(i == index);

        if (confirmButton != null)
            confirmButton.interactable = true;
    }

    // ====================== XÁC NHẬN VÀ ĐÓNG POPUP ======================
    private void ConfirmSelectedSkill()
    {
        if (selectedIndex < 0 || selectedIndex >= currentSkills.Count)
            return;

        SkillData chosenSkill = currentSkills[selectedIndex];

        // Unlock skill
        SkillSystem skillSystem = CommonReferent.Instance.skill;
        if (skillSystem != null)
        {
            bool success = skillSystem.UnlockSkill(chosenSkill.skillID);
            if (success)
                Debug.Log($"Đã áp dụng skill: {chosenSkill.skillName}");
            else
                Debug.LogWarning($"Không thể unlock skill: {chosenSkill.skillName}");
        }

        // ĐÓNG POPUP BẰNG UIMANAGER - giống như MapPopup
        UIManager.Instance.HidePopupByType(PopupType.LevelUpSkill);
    }
}