using UnityEngine;

public class ShurikenThrowSkill : ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skill)
    {
        GameObject shuriken = Object.Instantiate(skill.prefab, playerStats.transform.position, Quaternion.identity);
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
        return playerStats.Mana >= skill.manaCost;
    }
}
