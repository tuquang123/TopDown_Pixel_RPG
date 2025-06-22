using UnityEngine;
using System.Collections;

public class InvincibleSkill : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skillData)
    {
        // Cập nhật thời gian cooldown và mana thông qua SkillSystem
        SkillSystem skillSystem = playerStats.GetComponent<SkillSystem>();
        if (skillSystem != null)
        {
            skillSystem.UseSkill(skillData.skillID); // Gọi UseSkill để xử lý mana và cooldown
        }

        playerStats.StartCoroutine(ActivateInvincibility(playerStats, skillData));
    }

    private IEnumerator ActivateInvincibility(PlayerStats playerStats, SkillData skillData)
    {
        // Lấy cấp độ hiện tại từ PlayerStats
        int currentLevel = playerStats.GetSkillLevel(skillData.skillID);
        SkillLevelStat currentLevelStat = skillData.GetLevelStat(currentLevel);

        if (currentLevelStat == null)
        {
            Debug.LogError($"Không tìm thấy dữ liệu cấp độ {currentLevel} cho kỹ năng {skillData.skillName}");
            yield break;
        }

        int defenseBoostAmount = (int)currentLevelStat.value;

        // Tăng giáp tạm thời
        var modifier = new StatModifier(StatType.Defense, defenseBoostAmount);
        playerStats.defense.AddModifier(modifier);
        playerStats.isInvincible = true;
        Debug.Log("Kích hoạt bất tử!");

        // Spawn VFX shield
        Vector3 spawnPosition = playerStats.transform.position + Vector3.up;
        var prefab = skillData.GetPrefabAtLevel(playerStats.GetSkillLevel(skillData.skillID));
        var vfx = Object.Instantiate(prefab, spawnPosition, Quaternion.identity, playerStats.transform);
        vfx.SetActive(true);

        yield return new WaitForSeconds(currentLevelStat.duration);

        // Kết thúc hiệu ứng
        playerStats.defense.RemoveModifier(modifier);
        playerStats.isInvincible = false;
        Debug.Log("Hết hiệu ứng bất tử!");

        // Hủy VFX
        Object.Destroy(vfx);
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        // Lấy cấp độ hiện tại từ PlayerStats
        int currentLevel = playerStats.GetSkillLevel(skillData.skillID);
        SkillLevelStat currentLevelStat = skillData.GetLevelStat(currentLevel);

        if (currentLevelStat == null)
        {
            Debug.LogError($"Không tìm thấy dữ liệu cấp độ {currentLevel} cho kỹ năng {skillData.skillName}");
            return false;
        }

        // Kiểm tra mana
        if (playerStats.currentMana < currentLevelStat.manaCost)
            return false;

        // Kiểm tra cooldown thông qua SkillSystem
        SkillSystem skillSystem = playerStats.GetComponent<SkillSystem>();
        if (skillSystem == null || !skillSystem.CanUseSkill(skillData.skillID))
            return false;

        return true;
    }
}