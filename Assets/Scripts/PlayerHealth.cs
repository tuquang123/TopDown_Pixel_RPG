using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private static readonly int Property = Animator.StringToHash("4_Death");
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    private Animator anim;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player died!");
        anim.SetTrigger(Property);
        Destroy(gameObject, .5f);
    }
   
}