using UnityEngine;
using UnityEngine.UIElements;

public class SkillUIController : MonoBehaviour
{
    private VisualElement root;
    private Button[] skillSlots = new Button[5];

    public SkillSystem skillSystem;

    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        
        // Lấy các button từ UI Toolkit (skill-slot-1, skill-slot-2,...)
        for (int i = 0; i < 5; i++)
        {
            skillSlots[i] = root.Q<Button>($"skill-slot-{i + 1}");
            int index = i; // Tránh lỗi closure
            skillSlots[i].clicked += () => UseSkill(index);
        }

        // Đăng ký sự kiện để cập nhật UI khi gán skill
        skillSystem.OnSkillAssigned += UpdateSkillSlotUI;

        Debug.Log("🎨 Skill UI Loaded");
        
        // Gán skill thử nghiệm
        skillSystem.AssignSkillToSlot(0, SkillID.ShurikenThrow);
    }

    private void UpdateSkillSlotUI(int slotIndex, SkillID skillID)
    {
        if (slotIndex >= 0 && slotIndex < skillSlots.Length)
        {
            skillSlots[slotIndex].text = skillID.ToString(); // Hiển thị tên kỹ năng trên button
            Debug.Log($"🔄 UI cập nhật: {skillID} vào ô {slotIndex + 1}");
        }
    }

    private void UseSkill(int slotIndex)
    {
        if (skillSystem != null)
        {
            SkillID skill = skillSystem.GetAssignedSkill(slotIndex);
            if (skill != SkillID.None)
            {
                skillSystem.UseSkill(skill);
                Debug.Log($"🎯 Đã sử dụng {skill} từ ô {slotIndex + 1}");
            }
            else
            {
                Debug.Log("⚠ Chưa gán kỹ năng vào ô này!");
            }
        }
    }
}