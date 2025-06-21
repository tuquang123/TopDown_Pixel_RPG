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

        // Set icon and name
        nameText.text = currentSkill.skillName;
        iconImage.sprite = currentSkill.icon;

        // Get real level (allow level 0 if not yet learned)
        int level = skillSystem.GetSkillLevel(currentSkill.skillID);
        levelText.text = $"Cấp: {level}/{currentSkill.maxLevel}";

        // Preview level: if chưa học thì xem thử cấp 1, nếu đã học thì xem cấp hiện tại
        int previewLevel = level > 0 ? level : 1;

        int actualValue = currentSkill.value * previewLevel;
        int manaCost = currentSkill.manaCost;
        float cooldown = currentSkill.cooldown;
        float duration = currentSkill.duration;

        // Replace placeholders in template
        string description = currentSkill.descriptionTemplate
            .Replace("{value}", actualValue.ToString())
            .Replace("{mana}", manaCost.ToString())
            .Replace("{cooldown}", cooldown.ToString("0.#"))
            .Replace("{duration}", duration.ToString("0.#"));

        descriptionText.text = description;

        // Determine button visibility
        bool isUnlocked = skillSystem.IsSkillUnlocked(currentSkill.skillID);
        bool canLearn = skillSystem.CanUnlockSkill(currentSkill.skillID);
        bool isActive = currentSkill.skillType == SkillType.Active;

        learnButton.gameObject.SetActive(canLearn);
        assignButton.gameObject.SetActive(isUnlocked && isActive);

        // Reset listeners
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
                    Debug.Log($"Đã kích hoạt buff kỹ năng thụ động: {currentSkill.skillName}");
                }

                Setup(currentSkill, skillSystem); // Refresh UI
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
