using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillButton : MonoBehaviour
{
    private SkillData _skillData;
    private SkillSystem _skillSystem;

    [Header("UI References")]
    public TMP_Text skillNameText;        
    public TMP_Text skillDescriptionText; 
    public Image skillIconImage;
    
    public Button learnButton;
    public Button assignButton;

    public void Initialize(SkillData data, SkillSystem system)
    {
        _skillData = data;
        _skillSystem = system;

        learnButton.onClick.AddListener(OnLearnButtonClicked);
        assignButton.onClick.AddListener(OnAssignButtonClicked);

        UpdateUI();
    }

    private void UpdateUI()
    {
        SkillState skillState = _skillSystem.GetSkillState(_skillData.skillID);

        int currentLevel = skillState?.level ?? 0;
        bool isUnlocked = currentLevel > 0;
        bool isMaxLevel = currentLevel >= _skillData.maxLevel;

        skillNameText.text = $"{_skillData.skillName} Lv.{currentLevel}/{_skillData.maxLevel}";
        skillDescriptionText.text = _skillData.description;
        skillIconImage.sprite = _skillData.icon;

        learnButton.interactable = !isMaxLevel;
        assignButton.gameObject.SetActive(isUnlocked && _skillData.skillType == SkillType.Active);
    }

    private void OnLearnButtonClicked()
    {
        if (_skillSystem != null && _skillData != null)
        {
            if (_skillSystem.CanUnlockSkill(_skillData.skillID))
            {
                _skillSystem.UnlockSkill(_skillData.skillID);
                _skillSystem.DecrementSkillPoint();

                SkillState skillState = _skillSystem.GetSkillState(_skillData.skillID);
                int level = skillState?.level ?? 1;

                if (_skillData.skillType == SkillType.Passive)
                {
                    _skillSystem.UseSkill(_skillData.skillID);
                    Debug.Log($"Đã học/lên cấp kỹ năng thụ động: {_skillData.skillName} Lv.{level}");
                }
                else
                {
                    Debug.Log($"Đã học/lên cấp kỹ năng chủ động: {_skillData.skillName} Lv.{level}");
                }

                UpdateUI();
            }
            else
            {
                Debug.Log("Không đủ điểm kỹ năng để học hoặc đã đạt cấp tối đa.");
            }
        }
    }

    private void OnAssignButtonClicked()
    {
        if (_skillSystem != null)
        {
            int availableSlot = _skillSystem.GetFirstAvailableSlot();
            if (availableSlot != -1)
            {
                _skillSystem.AssignSkillToSlot(availableSlot, _skillData.skillID);
                Debug.Log($"Gán kỹ năng {_skillData.skillName} vào ô {availableSlot + 1}");
            }
        }
    }
}
