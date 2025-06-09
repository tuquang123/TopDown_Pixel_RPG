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
        int defenseBoostAmount = (int)skillData.value; // ví dụ: 1000

        // Tăng giáp tạm thời
        var modifier = new StatModifier(StatType.Defense, defenseBoostAmount);
        playerStats.defense.AddModifier(modifier);

        playerStats.isInvincible = true;
        Debug.Log("Kích hoạt bất tử!");

        yield return new WaitForSeconds(skillData.duration); // ví dụ: 5 giây

        playerStats.defense.RemoveModifier(modifier);
        playerStats.isInvincible = false;
        Debug.Log("Hết hiệu ứng bất tử!");
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        return playerStats.currentMana >= skillData.manaCost;
    }
}