using UnityEngine;

public class ShurikenThrowSkill : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skill)
    {
        var prefab = skill.GetPrefabAtLevel(playerStats.GetSkillLevel(skill.skillID));
        GameObject shuriken = Object.Instantiate(prefab, playerStats.transform.position, Quaternion.identity);
        var bouncingShuriken = shuriken.GetComponent<BouncingShuriken>();
        if (bouncingShuriken != null)
        {
            float randomAngle = Random.Range(0f, 360f);
            Vector2 randomDirection = Quaternion.Euler(0, 0, randomAngle) * Vector2.right;
            bouncingShuriken.SetDirection(randomDirection);
        }
    }

    public bool CanUse(PlayerStats playerStats, SkillData skill)
    {
        // Lấy cấp độ hiện tại từ PlayerStats
        int currentLevel = playerStats.GetSkillLevel(skill.skillID);
        SkillLevelStat currentLevelStat = skill.GetLevelStat(currentLevel);
        
        if (currentLevelStat == null)
        {
            Debug.LogError($"Không tìm thấy dữ liệu cấp độ {currentLevel} cho kỹ năng {skill.skillName}");
            return false;
        }
        
        // Kiểm tra mana
        if (playerStats.currentMana < currentLevelStat.manaCost)
            return false;
        
        return playerStats.currentMana >= currentLevelStat.manaCost;
    }
    
}
