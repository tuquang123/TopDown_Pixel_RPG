using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkillSlotUIController : MonoBehaviour
{
    public Button[] skillSlots = new Button[5]; // 5 ô slot
    public SkillSystem skillSystem;
    public Image[] cooldownOverlays = new Image[5]; // Thêm mảng các overlay cho cooldown

    private void OnEnable()
    {
        skillSystem.OnSkillAssigned += UpdateSkillSlots;
        skillSystem.OnSkillUsed += OnSkillUsed;

        // Cập nhật tất cả các slot khi bật UI
        for (int i = 0; i < skillSlots.Length; i++)
        {
            UpdateSkillSlots(i, skillSystem.GetAssignedSkill(i));
        }

        // Đăng ký sự kiện cho các nút
        for (int i = 0; i < skillSlots.Length; i++)
        {
            int index = i; 
            skillSlots[i].onClick.AddListener(() => OnSkillSlotClicked(index));
        }
    }


    void OnDisable()
    {
        skillSystem.OnSkillUsed -= OnSkillUsed;
    }

    private void OnSkillUsed(SkillID skillID)
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSystem.GetAssignedSkill(i) == skillID)
            {
                float cooldown = skillSystem.GetSkillData(skillID).cooldown;
                StartCooldown(i, cooldown);
                break;
            }
        }
    }

    private void OnSkillSlotClicked(int slotIndex)
    {
        // Kiểm tra xem ô này có kỹ năng nào đã gán không
        SkillID skillID = skillSystem.GetAssignedSkill(slotIndex);
        if (skillID != SkillID.None)
        {
            // Nếu có kỹ năng gán vào ô, sử dụng kỹ năng đó
            skillSystem.UseSkill(skillID);
        }
        else
        {
            Debug.Log("Không có kỹ năng nào gán vào ô này.");
        }
    }

    private void StartCooldown(int slotIndex, float cooldownTime)
    {
        if (cooldownOverlays[slotIndex].fillAmount > 0) return; // Nếu đang cooldown rồi thì không làm gì cả

        StartCoroutine(CooldownRoutine(slotIndex, cooldownTime));
    }

    private IEnumerator CooldownRoutine(int slotIndex, float cooldownTime)
    {
        cooldownOverlays[slotIndex].fillAmount = 1; // Đặt lại thời gian cooldown
        float timeElapsed = 0f;

        while (timeElapsed < cooldownTime)
        {
            timeElapsed += Time.deltaTime;
            cooldownOverlays[slotIndex].fillAmount = 1 - (timeElapsed / cooldownTime);
            yield return null;
        }

        cooldownOverlays[slotIndex].fillAmount = 0; // Reset khi hết cooldown
    }

    private void UpdateSkillSlots(int slotIndex, SkillID skillID)
    {
        // Cập nhật UI slot theo kỹ năng đã gán
        if (skillID == SkillID.None)
        {
            skillSlots[slotIndex].GetComponentInChildren<Text>().text = "Lock";
            skillSlots[slotIndex].image.sprite = null;
            cooldownOverlays[slotIndex].fillAmount = 0; // Reset overlay khi không có skill
        }
        else
        {
            SkillData skillData = skillSystem.skillList.Find(s => s.skillID == skillID);
            skillSlots[slotIndex].GetComponentInChildren<Text>().text = skillData.skillName;
            skillSlots[slotIndex].image.sprite = skillData.icon;
        }
    }
}
