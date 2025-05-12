using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private static readonly int DeathHash = Animator.StringToHash("4_Death");
    private Animator anim;
    private PlayerStats playerStats;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        playerStats = GetComponent<PlayerStats>();
    }

    public void TakeDamage(int damage)
    {
        playerStats.TakeDamage(damage);

        if (playerStats.maxHealth.Value <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        //Debug.Log("Player died!");
        anim.SetTrigger(DeathHash);
        //Destroy(gameObject, .5f);
    }
}