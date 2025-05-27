using UnityEngine;

public class CommonAnimationEvents : MonoBehaviour
{
    private PlayerController playerController;
    private EnemyAI enemyAI;

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
        enemyAI = GetComponentInParent<EnemyAI>();
    }

    // Gọi từ animation event
    public void OnAttackHit()
    {
        if (playerController != null)
        {
            playerController.ApplyAttackDamage(); // Gây damage cho enemy
        }
        else if (enemyAI != null)
        {
            enemyAI.DealDamageToPlayer(); // Gây damage cho player
        }
    }
}