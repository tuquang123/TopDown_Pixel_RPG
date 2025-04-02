using UnityEngine;

public class ShurikenThrow : MonoBehaviour, ISkill
{
    public void ExecuteSkill(PlayerStats playerStats, SkillData skill)
    {
        GameObject shuriken = Instantiate(skill.prefab, playerStats.transform.position, Quaternion.identity);
        BouncingShuriken bouncingShuriken = shuriken.GetComponent<BouncingShuriken>();
        if (bouncingShuriken != null)
        {
            float randomAngle = Random.Range(0f, 360f);
            Vector2 randomDirection = Quaternion.Euler(0, 0, randomAngle) * Vector2.right;
            bouncingShuriken.SetDirection(randomDirection);
        }
        Debug.Log("Ném phi tiêu!");
    }

    public bool CanUse(PlayerStats playerStats, SkillData skillData)
    {
        if (playerStats.Mana < skillData.manaCost)
        {
            Debug.Log("Không đủ mana!");
            return false;
        }
        return true;
    }
}