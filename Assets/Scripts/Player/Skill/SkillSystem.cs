using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public interface ISkill
{
    void ExecuteSkill(PlayerStats playerStats,SkillData data);
    bool CanUse(PlayerStats playerStats,SkillData data);
}
public class SkillFactory
{
    public static ISkill CreateSkill(SkillID skillID)
    {
        switch (skillID)
        {
            case SkillID.ShurikenThrow:
                return new ShurikenThrowSkill();  // class logic, không phải MonoBehaviour
            case SkillID.DamageBoost:
                return new DamageBoost();
            default:
                throw new ArgumentException("Không tìm thấy kỹ năng với ID này.");
        }
    }
}

public class SkillSystem : MonoBehaviour
{
    public List<SkillData> skillList = new List<SkillData>();
    private PlayerStats _playerStats;
    private Dictionary<SkillID, bool> unlockedSkills = new Dictionary<SkillID, bool>();
    private Dictionary<SkillID, float> skillCooldownTimes = new Dictionary<SkillID, float>();
    private Dictionary<int, SkillID> assignedSkills = new Dictionary<int, SkillID>();
    public event Action<int, SkillID> OnSkillAssigned;

    private void Start()
    {
        _playerStats = GetComponent<PlayerStats>();
    }

    public void UnlockSkill(SkillID skillID)
    {
        SkillData skill = skillList.Find(s => s.skillID == skillID);
        if (skill != null && !unlockedSkills.ContainsKey(skillID) && _playerStats.level >= skill.requiredLevel)
        {
            unlockedSkills[skillID] = true;
            Debug.Log($"Đã học kỹ năng: {skill.skillName}");
        }
    }

    public SkillData GetSkillData(SkillID skillID)
    {
        return skillList.Find(s => s.skillID == skillID);
    }

    public void AssignSkillToSlot(int slotIndex, SkillID skillID)
    {
        if (slotIndex >= 0 && slotIndex < 5 && unlockedSkills.ContainsKey(skillID) && unlockedSkills[skillID])
        {
            // Kiểm tra nếu kỹ năng đã được gán vào bất kỳ ô nào
            if (assignedSkills.ContainsValue(skillID))
            {
                Debug.Log($"Kỹ năng {skillID} đã được gán vào một ô khác, không thể gán thêm.");
                return; // Nếu kỹ năng đã được gán vào một ô, không thể gán vào ô này nữa
            }

            // Kiểm tra xem ô này đã có kỹ năng chưa
            if (assignedSkills.ContainsKey(slotIndex) && assignedSkills[slotIndex] != SkillID.None)
            {
                Debug.Log($"Ô {slotIndex + 1} đã có kỹ năng, không thể gán thêm.");
                return; // Nếu ô đã có kỹ năng, không cho gán thêm
            }

            // Nếu chưa có kỹ năng nào, gán kỹ năng vào ô
            assignedSkills[slotIndex] = skillID;
            OnSkillAssigned?.Invoke(slotIndex, skillID);
            Debug.Log($"Đã gán kỹ năng: {skillID} vào ô {slotIndex + 1}");
        }
        else
        {
            Debug.Log("Kỹ năng không hợp lệ hoặc không thể gán vào ô này.");
        }
    }
    public void DecrementSkillPoint()
    {
        if (_playerStats.skillPoints > 0)
        {
            _playerStats.skillPoints--;
            Debug.Log($"Điểm kỹ năng còn lại: {_playerStats.skillPoints}");
        }
        else
        {
            Debug.Log("Không còn điểm kỹ năng.");
        }
    }
    
    public bool CanUnlockSkill(SkillID skillID)
    {
        // Kiểm tra nếu kỹ năng đã được học hay chưa và có đủ điểm kỹ năng hay không
        return !IsSkillUnlocked(skillID) && _playerStats.skillPoints > 0;
    }


    public SkillID GetAssignedSkill(int slotIndex)
    {
        return assignedSkills.ContainsKey(slotIndex) ? assignedSkills[slotIndex] : SkillID.None;
    }

    // Thực thi một kỹ năng
    public void UseSkill(SkillID skillID)
    {
        SkillData skill = skillList.Find(s => s.skillID == skillID);
        if (skill == null || !unlockedSkills.ContainsKey(skillID) || !unlockedSkills[skillID]) // Kiểm tra xem kỹ năng có mở khóa không
        {
            Debug.Log("Kỹ năng chưa được mở khóa!");
            return;
        }

        // Kiểm tra cooldown từ Dictionary
        if (Time.time < skillCooldownTimes.GetValueOrDefault(skillID, -Mathf.Infinity) + skill.cooldown)
        {
            Debug.Log("Kỹ năng đang trong thời gian hồi chiêu!");
            return;
        }

        // Kiểm tra mana thông qua skillData
        ISkill skillInstance = SkillFactory.CreateSkill(skillID);

        if (skillInstance.CanUse(_playerStats, skill)) // Kiểm tra có thể sử dụng kỹ năng
        {
            skillCooldownTimes[skillID] = Time.time; // Lưu thời gian sử dụng kỹ năng
            skillInstance.ExecuteSkill(_playerStats, skill); // Thực thi kỹ năng
        }
    }
    public int GetFirstAvailableSlot()
    {
        // Kiểm tra các slot từ 0 đến 4 để tìm ô trống
        for (int i = 0; i < 5; i++)
        {
            if (!assignedSkills.ContainsKey(i) || assignedSkills[i] == SkillID.None)
            {
                return i; // Trả về chỉ số của ô slot trống đầu tiên
            }
        }
        return -1; // Nếu không có ô trống, trả về -1
    }
    public bool IsSkillUnlocked(SkillID skillID)
    {
        return unlockedSkills.ContainsKey(skillID) && unlockedSkills[skillID];
    }

}


