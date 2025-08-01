using System.Collections;
using UnityEngine;

public class InvincibleSkill : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skillData)
    {
        if (playerStats == null || skillData == null)
        {
            Debug.LogError("InvincibleSkill.ExecuteSkill: playerStats hoặc skillData null.");
            return;
        }

        SkillSystem skillSystem = playerStats.GetComponent<SkillSystem>();
        if (skillSystem == null)
        {
            Debug.LogError("InvincibleSkill.ExecuteSkill: Không tìm thấy SkillSystem trên playerStats.");
            return;
        }

        skillSystem.UseSkill(skillData.skillID); // xử lý cooldown & mana

        playerStats.StartCoroutine(ActivateInvincibility(playerStats, skillData));
    }

    private IEnumerator ActivateInvincibility(PlayerStats playerStats, SkillData skillData)
    {
        int level = playerStats.GetSkillLevel(skillData.skillID);
        SkillLevelStat levelStat = skillData.GetLevelStat(level);

        if (levelStat == null)
        {
            Debug.LogError($"Không tìm thấy level stat cho kỹ năng {skillData.skillName} cấp {level}");
            yield break;
        }

        int defenseBoost = (int)levelStat.value;
        var modifier = new StatModifier(StatType.Defense, defenseBoost);

        playerStats.defense.AddModifier(modifier);
        playerStats.isInvincible = true;
        Debug.Log($"[InvincibleSkill] Bất tử kích hoạt! +{defenseBoost} DEF");

        // Spawn VFX nếu có
        GameObject prefab = skillData.GetPrefabAtLevel(level);
        GameObject vfx = null;
        if (prefab != null)
        {
            vfx = Object.Instantiate(prefab, playerStats.transform.position + Vector3.up, Quaternion.identity, playerStats.transform);
            vfx.SetActive(true);
        }

        yield return new WaitForSeconds(levelStat.duration);

        playerStats.defense.RemoveModifier(modifier);
        playerStats.isInvincible = false;
        Debug.Log("[InvincibleSkill] Hiệu ứng bất tử kết thúc.");

        if (vfx != null)
            Object.Destroy(vfx);
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        int currentLevel = playerStats.GetSkillLevel(skillData.skillID);
        SkillLevelStat levelStat = skillData.GetLevelStat(currentLevel);

        if (levelStat == null) return false;

        if (playerStats.currentMana < levelStat.manaCost)
            return false;

        return true;
    }

}
