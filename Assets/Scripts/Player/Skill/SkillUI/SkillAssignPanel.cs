using UnityEngine;
using UnityEngine.UI;

public class SkillAssignPanel : MonoBehaviour
{
    [SerializeField] private SkillButtonAssign[] assignButtons; // 5 buttons
    private SkillData skillToAssign;
    private SkillSystem skillSystem;

    public void Show(SkillData skill, SkillSystem system)
    {
        skillToAssign = skill;
        skillSystem = system;

        for (int i = 0; i < assignButtons.Length; i++)
        {
            int index = i;

            SkillID existingSkill = skillSystem.GetAssignedSkill(index);
            SkillData assignedSkill = skillSystem.GetSkillData(existingSkill);

            if (assignedSkill != null)
            {
                assignButtons[i].icon.sprite = assignedSkill.icon;
                assignButtons[i].nameText.text = assignedSkill.skillName;
                assignButtons[i].icon.enabled = true;
                assignButtons[i].nameText.enabled = true;
            }
            else
            {
                assignButtons[i].icon.enabled = false;
                assignButtons[i].nameText.enabled = false;
            }

            Button btn = assignButtons[i].button;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => AssignToSlot(index));
        }

        gameObject.SetActive(true);
    }

    private void AssignToSlot(int index)
    {
        // Gỡ kỹ năng khỏi các ô khác nếu đã gán trước đó
        for (int i = 0; i < assignButtons.Length; i++)
        {
            if (i == index) continue;

            if (skillSystem.GetAssignedSkill(i) == skillToAssign.skillID)
            {
                skillSystem.AssignSkillToSlot(i, SkillID.None);
                assignButtons[i].icon.enabled = false;
                assignButtons[i].nameText.enabled = false;
            }
        }

        // Gán vào ô được chọn
        skillSystem.AssignSkillToSlot(index, skillToAssign.skillID);
        assignButtons[index].icon.sprite = skillToAssign.icon;
        assignButtons[index].nameText.text = skillToAssign.skillName;
        assignButtons[index].icon.enabled = true;
        assignButtons[index].nameText.enabled = true;

        Debug.Log($"Đã gán {skillToAssign.skillName} vào ô {index + 1}");

        //gameObject.SetActive(false);
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}