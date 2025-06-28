using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillDetailPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image iconImage;

    [SerializeField] private Button learnButton;
    [SerializeField] private Button assignButton;
    [SerializeField] private Button closeButton;

    private SkillData currentSkill;
    private SkillSystem skillSystem;

    public void Setup(SkillData skillData, SkillSystem system)
    {
        currentSkill = skillData;
        skillSystem = system;

        nameText.text = currentSkill.skillName;
        iconImage.sprite = currentSkill.icon;

        int currentLevel = skillSystem.GetSkillLevel(currentSkill.skillID);
        levelText.text = $"Cấp: {currentLevel}/{currentSkill.maxLevel}";

        string fullDescription = "";

        // Mô tả cấp hiện tại
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

                fullDescription += $"<b>Hiện tại (Cấp {currentLevel}):</b>\n{desc}\n\n";
            }
        }

        // Mô tả cấp tiếp theo
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

                fullDescription += $"<b>Cấp tiếp theo (Cấp {nextLevel}):</b>\n{desc}";
            }
        }

        descriptionText.text = fullDescription;

        // Kiểm tra điều kiện gán / học
        bool isUnlocked = skillSystem.IsSkillUnlocked(currentSkill.skillID);
        bool canLearn = skillSystem.CanUnlockSkill(currentSkill.skillID);
        bool isActive = currentSkill.skillType == SkillType.Active;

        learnButton.gameObject.SetActive(canLearn);
        assignButton.gameObject.SetActive(isUnlocked && isActive);

        // Gán sự kiện
        learnButton.onClick.RemoveAllListeners();
        assignButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        learnButton.onClick.AddListener(OnClickLearn);
        assignButton.onClick.AddListener(OnClickAssign);
        closeButton.onClick.AddListener(Hide);

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

    private void OnClickAssign()
    {
        int availableSlot = skillSystem.GetFirstAvailableSlot();
        if (availableSlot != -1)
        {
            skillSystem.AssignSkillToSlot(availableSlot, currentSkill.skillID);
            Debug.Log($"Đã gán kỹ năng {currentSkill.skillName} vào ô {availableSlot + 1}");
        }
        else
        {
            Debug.Log("Không còn ô kỹ năng trống.");
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
