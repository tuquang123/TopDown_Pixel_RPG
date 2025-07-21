using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillDetailPanel : MonoBehaviour
{
    [Header("UI Elements")] [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image iconImage;

    [SerializeField] private Button learnButton;
    [SerializeField] private Button assignButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button closeButtonFull;
    [SerializeField] private SkillAssignPanel assignPanel;

    private SkillData currentSkill;
    private SkillSystem skillSystem;

    private void OnClickAssign()
    {
        assignPanel.Show(currentSkill, skillSystem);
    }

    public void Setup(SkillData skillData, SkillSystem system)
    {
        currentSkill = skillData;
        skillSystem = system;

        nameText.text = currentSkill.skillName;
        iconImage.sprite = currentSkill.icon;

        int currentLevel = skillSystem.GetSkillLevel(currentSkill.skillID);
        levelText.text = $"Level: {currentLevel}/{currentSkill.maxLevel}";

        string fullDescription = "";

        // Current level description
        if (currentLevel > 0)
        {
            SkillLevelStat currentStat = currentSkill.GetLevelStat(currentLevel);
            if (currentStat != null)
            {
                string desc = currentSkill.descriptionTemplate
                    .Replace("{value}", currentStat.value.ToString())
                    .Replace("{mana}", currentStat.manaCost.ToString())
                    .Replace("{cooldown}", currentStat.cooldown.ToString("0.#"))
                    .Replace("{duration}", currentStat.duration.ToString("0.#"));

                fullDescription += $"<b>Current (Level {currentLevel}):</b>\n{desc}\n\n";
            }
        }

        // Next level preview
        int nextLevel = currentLevel + 1;
        if (nextLevel <= currentSkill.maxLevel)
        {
            SkillLevelStat nextStat = currentSkill.GetLevelStat(nextLevel);
            if (nextStat != null)
            {
                string desc = currentSkill.descriptionTemplate
                    .Replace("{value}", nextStat.value.ToString())
                    .Replace("{mana}", nextStat.manaCost.ToString())
                    .Replace("{cooldown}", nextStat.cooldown.ToString("0.#"))
                    .Replace("{duration}", nextStat.duration.ToString("0.#"));

                fullDescription += $"<b>Next (Level {nextLevel}):</b>\n{desc}";
            }
        }

        descriptionText.text = fullDescription;

        // Learn & assign condition check
        bool isUnlocked = skillSystem.IsSkillUnlocked(currentSkill.skillID);
        bool canLearn = skillSystem.CanUnlockSkill(currentSkill.skillID);
        bool isActive = currentSkill.skillType == SkillType.Active;

        learnButton.gameObject.SetActive(canLearn);
        assignButton.gameObject.SetActive(isUnlocked && isActive);

        // Hook up events
        learnButton.onClick.RemoveAllListeners();
        assignButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
        closeButtonFull.onClick.RemoveAllListeners();

        learnButton.onClick.AddListener(OnClickLearn);
        assignButton.onClick.AddListener(OnClickAssign);
        closeButton.onClick.AddListener(Hide);
        closeButtonFull.onClick.AddListener(Hide);

        gameObject.SetActive(true);
    }


    private void OnClickLearn()
    {
        if (skillSystem.CanUnlockSkill(currentSkill.skillID))
        {
            bool success = skillSystem.UnlockSkill(currentSkill.skillID);
            if (success)
            {
                skillSystem.DecrementSkillPoint();

                if (currentSkill.skillType == SkillType.Passive)
                {
                    skillSystem.UseSkill(currentSkill.skillID);
                    Debug.Log($"Đã kích hoạt kỹ năng thụ động: {currentSkill.skillName}");
                }

                Setup(currentSkill, skillSystem); // Refresh
            }
            else
            {
                Debug.Log("Đã đạt cấp tối đa.");
            }
        }
        else
        {
            Debug.Log("Không đủ điểm kỹ năng hoặc không thể nâng cấp.");
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}