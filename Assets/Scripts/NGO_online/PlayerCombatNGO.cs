using Unity.Netcode;
using UnityEngine;

public class PlayerCombatNGO : NetworkBehaviour
{
    [Header("Attack Settings")]
    public float attackDamage = 20f;
    public float attackRange = 2f;
    public float attackCooldown = 0.5f;
    
    [Header("Animation")]
    public Animator anim;
    private static readonly int AttackTrigger = Animator.StringToHash("2_Attack");
    
    private float lastAttackTime;
    private PlayerHealthNGO playerHealth;

    void Awake()
    {
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
        
        playerHealth = GetComponent<PlayerHealthNGO>();
    }

    void Update()
    {
        if (!IsOwner) return;
        if (playerHealth != null && playerHealth.IsDead()) return;
        
        // Nhấn SPACE hoặc click chuột để tấn công
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) 
            && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        
        // Play animation local
        if (anim != null)
            anim.SetTrigger(AttackTrigger);
        
        // Gọi server xử lý damage
        AttackServerRpc();
    }

    [ServerRpc]
    void AttackServerRpc()
    {
        // Tìm enemies trong range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        
        foreach (var hit in hits)
        {
            // Bỏ qua chính mình
            if (hit.transform == transform) continue;
            
            // Tìm PlayerHealth của enemy
            if (hit.TryGetComponent<PlayerHealthNGO>(out var enemyHealth))
            {
                // Check nếu là NetworkObject khác
                var enemyNetObj = hit.GetComponent<NetworkObject>();
                if (enemyNetObj != null && enemyNetObj.OwnerClientId != OwnerClientId)
                {
                    Debug.Log($"Player {OwnerClientId} hit Player {enemyNetObj.OwnerClientId}!");
                    enemyHealth.TakeDamageServerRpc(attackDamage, OwnerClientId);
                }
            }
        }
        
        // Sync animation cho tất cả clients
        AttackClientRpc();
    }

    [ClientRpc]
    void AttackClientRpc()
    {
        if (IsOwner) return; // Owner đã play rồi
        
        if (anim != null)
            anim.SetTrigger(AttackTrigger);
    }

    // Debug: Vẽ attack range
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
