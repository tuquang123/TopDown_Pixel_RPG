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

    // Gọi từ animation event: melee hit
    public void OnAttackHit()
    {
        if (playerController != null)
        {
            playerController.ApplyAttackDamage();
        }
        else if (enemyAI is EnemyRangedAI rangedAI)
        {
            rangedAI.FireProjectile();
        }
        else if (enemyAI != null)
        {
            enemyAI.DealDamageToTarget();
        }
    }
}