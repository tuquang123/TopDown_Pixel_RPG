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
                GameObject obj = new GameObject("ShurikenThrowSkill");
                return obj.AddComponent<ShurikenThrow>();  // ✅ Đúng cách
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
    private PlayerStats playerStats;
    private Dictionary<SkillID, bool> unlockedSkills = new Dictionary<SkillID, bool>(); // Quản lý trạng thái mở khóa
    private Dictionary<SkillID, float> skillCooldownTimes = new Dictionary<SkillID, float>(); // Quản lý thời gian hồi chiêu

    public Button buttonSkill1;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        UnlockSkill(SkillID.ShurikenThrow);  // Example skill unlock
        UnlockSkill(SkillID.DamageBoost); 
        UseSkill(SkillID.DamageBoost);
        buttonSkill1.onClick.AddListener(() => UseSkill(SkillID.ShurikenThrow));  // Example button action
    }

    public void UnlockSkill(SkillID skillID)
    {
        SkillData skill = skillList.Find(s => s.skillID == skillID);
        if (skill != null && !unlockedSkills.ContainsKey(skillID) && playerStats.level >= skill.requiredLevel)
        {
            unlockedSkills[skillID] = true;  // Cập nhật trạng thái mở khóa
            Debug.Log($"Đã học kỹ năng: {skill.skillName}");
        }
    }
    public void UseSkill(SkillID skillID)
    {
        SkillData skill = skillList.Find(s => s.skillID == skillID);
        if (skill == null || !unlockedSkills.ContainsKey(skillID) || !unlockedSkills[skillID])  // Kiểm tra trong unlockedSkills
        {
            Debug.Log("Skill not unlocked!");
            return;
        }

        // Kiểm tra cooldown từ Dictionary
        if (Time.time < skillCooldownTimes.GetValueOrDefault(skillID, -Mathf.Infinity) + skill.cooldown)
        {
            Debug.Log("Skill is on cooldown!");
            return;
        }

        // Kiểm tra mana thông qua skillData
        ISkill skillInstance = SkillFactory.CreateSkill(skillID);

        if (skillInstance.CanUse(playerStats, skill))  // Kiểm tra có thể sử dụng kỹ năng
        {
            skillCooldownTimes[skillID] = Time.time;  // Lưu thời gian sử dụng skill
            skillInstance.ExecuteSkill(playerStats,skill);  // Thực thi kỹ năng
        }
    }

}

