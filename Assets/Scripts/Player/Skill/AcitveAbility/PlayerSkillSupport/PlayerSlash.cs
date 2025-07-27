using System.Collections;
using UnityEngine;


public class PlayerSlash : MonoBehaviour, IGameEventListener
{
    private Animator anim;
    private PlayerStats stats;
    [SerializeField] Transform attackPoint;
    public float attackRadius = 1.2f;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        stats = GetComponent<PlayerStats>();
    }
    public void OnEventRaised()
    {
        Debug.Log("Người chơi đã dùng ngựa.");
        anim = GetComponentInChildren<Animator>();
    }

    private void OnEnable() => GameEvents.OnUpdateAnimation.RegisterListener(this);
    private void OnDisable() => GameEvents.OnUpdateAnimation.UnregisterListener(this);

    public void Slash(SkillData skill)
    {
        int currentLevel = stats.GetSkillLevel(skill.skillID);
        SkillLevelStat currentLevelStat = skill.GetLevelStat(currentLevel);

        if (currentLevelStat == null)
        {
            Debug.LogError($"Không tìm thấy dữ liệu cấp độ {currentLevel} cho kỹ năng {skill.skillName}");
            return;
        }

        // Kiểm tra đủ mana
        if (stats.currentMana < currentLevelStat.manaCost)
            return;

        // Kiểm tra có ít nhất 1 enemy trong vùng chém
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);
        bool hasTarget = false;
        foreach (var enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                hasTarget = true;
                break;
            }
        }

        if (!hasTarget)
        {
            Debug.Log("Không có enemy trong phạm vi -> hủy Slash");
            return;
        }

        // Trừ mana, bật animation và đánh
        stats.UseMana(currentLevelStat.manaCost);
        anim.SetTrigger("9_Slash");
        stats.isUsingSkill = true;

        StartCoroutine(SlashDelayAndDamage(skill));
    }
    private IEnumerator SlashDelayAndDamage(SkillData skill)
    {
        yield return new WaitForSeconds(0.2f);

        // Gọi hàm đánh đã gộp, không gây damage lên destructible
        PlayerController.Instance.ApplyAttackDamage(true, skill);

        stats.isUsingSkill = false;
    }


}

