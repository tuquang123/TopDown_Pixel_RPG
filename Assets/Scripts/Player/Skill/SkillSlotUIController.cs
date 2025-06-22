using UnityEngine;

public class SkillSlotUIController : MonoBehaviour
{
    public SkillSystem skillSystem;
    public SkillSlotUI[] skillSlots;

    private void OnEnable()
    {
        skillSystem.OnSkillAssigned += UpdateSkillSlot;
        skillSystem.OnSkillUsed += HandleSkillUsed;

        for (int i = 0; i < skillSlots.Length; i++)
        {
            int index = i;
            skillSlots[i].button.onClick.AddListener(() => OnSkillClicked(index));
            UpdateSkillSlot(index, skillSystem.GetAssignedSkill(index));
        }
    }

    private void OnDisable()
    {
        skillSystem.OnSkillAssigned -= UpdateSkillSlot;
        skillSystem.OnSkillUsed -= HandleSkillUsed;
    }

    private void UpdateSkillSlot(int index, SkillID skillID)
    {
        var skillData = skillSystem.GetSkillData(skillID);
        skillSlots[index].SetSkill(skillData);
    }

    private void HandleSkillUsed(SkillID skillID)
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSystem.GetAssignedSkill(i) == skillID)
            {
                var skillData = skillSystem.GetSkillData(skillID);
                int currentLevel = skillSystem.PlayerStats.GetSkillLevel(skillData.skillID);
                SkillLevelStat currentLevelStat = skillData.GetLevelStat(currentLevel);

                if (currentLevelStat == null)
                {
                    Debug.LogError($"Không tìm thấy dữ liệu cấp độ {currentLevel} cho kỹ năng {skillData.skillName}");
                    return;
                }
                float cd = currentLevelStat.cooldown;
                skillSlots[i].StartCooldown(cd);
                break;
            }
        }
    }

    private void OnSkillClicked(int index)
    {
        SkillID id = skillSystem.GetAssignedSkill(index);
        if (id != SkillID.None)
            skillSystem.UseSkill(id);
        else
            Debug.Log("Chưa có kỹ năng gán vào slot " + index);
    }
}