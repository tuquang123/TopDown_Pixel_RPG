using UnityEngine;

public class BossAI : EnemyAI
{
    [Header("Boss Special Settings")]
    [SerializeField] private float specialAttackCooldown = 5f;
    //[SerializeField] private GameObject specialAttackPrefab;
    
    [SerializeField] BossHealthUI bossHealthUI;
    private float _lastSpecialAttackTime;
    
    protected override void Start()
    {
        base.Start();
        // = FindObjectOfType<BossHealthUI>(); // UI chỉ có 1
        bossHealthUI?.SetMaxHealth(maxHealth);
    }

    protected override void AttackPlayer()
    {
        base.AttackPlayer(); // Gọi tấn công thường

        // Tấn công đặc biệt nếu đủ thời gian
        if (Time.time - _lastSpecialAttackTime >= specialAttackCooldown)
        {
            PerformSpecialAttack();
            _lastSpecialAttackTime = Time.time;
        }
    }

    private void PerformSpecialAttack()
    {
        /*if (specialAttackPrefab != null)
        {
            Instantiate(specialAttackPrefab, transform.position, Quaternion.identity);
            Debug.Log("Boss dùng skill đặc biệt!");
        }*/
    }
    public override void TakeDamage(int damage, bool isCrit = false)
    {
        base.TakeDamage(damage, isCrit);
        bossHealthUI?.UpdateHealth(currentHealth);
    }

    protected override void Die()
    {
        base.Die();
        bossHealthUI?.Hide();
    }

}
