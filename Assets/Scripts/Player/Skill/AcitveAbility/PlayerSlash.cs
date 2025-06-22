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
        int currentLevel = 1; 
        SkillLevelStat currentLevelStat = skill.GetLevelStat(currentLevel);

        if (currentLevelStat == null)
        {
            Debug.LogError($"Không tìm thấy dữ liệu cấp độ {currentLevel} cho kỹ năng {skill.skillName}");
        }
        
        if (currentLevelStat != null && stats.currentMana < currentLevelStat.manaCost) return;

        if (currentLevelStat != null) stats.UseMana(currentLevelStat.manaCost);

        anim.SetTrigger("9_Slash"); // Gọi animation Skill (cần đảm bảo đã setup trên Animator)

        // Gọi coroutine để delay ra đòn
        StartCoroutine(PerformSlashAfterDelay(skill));
    }

    private IEnumerator PerformSlashAfterDelay(SkillData skill)
    {
        yield return new WaitForSeconds(0.2f); // Delay để chờ animation swing tới thời điểm ra đòn

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                int baseDamage = (int)stats.attack.Value;
                bool isCrit = Random.Range(0f, 100f) < stats.GetCritChance();
                int finalDamage = isCrit ? Mathf.RoundToInt(baseDamage * 1.5f) : baseDamage;

                enemy.GetComponent<EnemyAI>()?.TakeDamage(finalDamage, isCrit);
                stats.HealFromLifeSteal(finalDamage);

                // Có thể tạo hiệu ứng slash ở đây nếu có prefab
                // Instantiate(slashVFX, attackPoint.position, Quaternion.identity);
            }
        }
    }
}

