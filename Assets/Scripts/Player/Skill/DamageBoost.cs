using UnityEngine;

public class DamageBoost : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skillData)
    {
        // Lấy giá trị từ SkillData (ví dụ: value có thể là tỷ lệ % tăng sát thương)
        float damageIncrease = playerStats.attack.Value * skillData.value / 100;  // Tăng sát thương theo value (ví dụ: value là 10 cho 10% tăng sát thương)
        playerStats.attack.AddModifier(new StatModifier(StatType.Attack, (int)damageIncrease));  // Thêm modifier vào attack
        Debug.Log($"Kỹ năng DamageBoost đã được kích hoạt! Tăng {damageIncrease} sát thương.");
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        // Kiểm tra các điều kiện để sử dụng kỹ năng, nếu cần thiết
        return true;  // Đối với kỹ năng thụ động, có thể không cần kiểm tra mana hay cooldown
    }
}