using UnityEngine;
using System.Collections;

public class InvincibleSkill : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skillData)
    {
        playerStats.StartCoroutine(ActivateInvincibility(playerStats, skillData));
    }

    private IEnumerator ActivateInvincibility(PlayerStats playerStats, SkillData skillData)
    {
        int defenseBoostAmount = (int)skillData.value;

        // Tăng giáp tạm thời
        var modifier = new StatModifier(StatType.Defense, defenseBoostAmount);
        playerStats.defense.AddModifier(modifier);
        playerStats.isInvincible = true;
        Debug.Log("Kích hoạt bất tử!");

        // ✅ Spawn VFX shield
        Vector3 spawnPosition = playerStats.transform.position + Vector3.up;
        var vfx = Object.Instantiate(skillData.prefab, spawnPosition, Quaternion.identity, playerStats.transform);

        vfx.SetActive(true);

        yield return new WaitForSeconds(skillData.duration);

        // Kết thúc hiệu ứng
        playerStats.defense.RemoveModifier(modifier);
        playerStats.isInvincible = false;
        Debug.Log("Hết hiệu ứng bất tử!");

        // ✅ Hủy VFX
        Object.Destroy(vfx);
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        return playerStats.currentMana >= skillData.manaCost;
    }
}