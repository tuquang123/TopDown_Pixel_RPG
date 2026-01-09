using Photon.Pun;
using UnityEngine;

public class PlayerCombatPUN : MonoBehaviourPunCallbacks
{
    [Header("Attack Settings")]
    public float attackDamage = 20f;
    public float attackRange = 2f;
    public float attackCooldown = 0.5f;
    
    [Header("Animation")]
    public Animator anim;
    private static readonly int AttackTrigger = Animator.StringToHash("2_Attack");
    
    private float lastAttackTime;
    private PlayerHealthPUN playerHealth;

    void Awake()
    {
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
        
        playerHealth = GetComponent<PlayerHealthPUN>();
    }

    void Update()
    {
        if (!this.photonView.IsMine) return;
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
        
        // Gọi RPC để xử lý damage và sync animation
        this.photonView.RPC("AttackRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void AttackRPC()
    {
        // Remote players: play animation
        if (!this.photonView.IsMine && anim != null)
        {
            anim.SetTrigger(AttackTrigger);
        }
        
        // Chỉ Master Client xử lý damage để tránh conflict
        if (PhotonNetwork.IsMasterClient)
        {
            ProcessAttackDamage();
        }
    }

    void ProcessAttackDamage()
    {
        // Tìm enemies trong range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        
        foreach (var hit in hits)
        {
            // Bỏ qua chính mình
            if (hit.transform == transform) continue;
            
            // Tìm PlayerHealth của enemy
            if (hit.TryGetComponent<PlayerHealthPUN>(out var enemyHealth))
            {
                // Check nếu là PhotonView khác
                var enemyPhotonView = hit.GetComponent<PhotonView>();
                if (enemyPhotonView != null && enemyPhotonView.ViewID != this.photonView.ViewID)
                {
                    Debug.Log($"Player {this.photonView.Owner.NickName} hit Player {enemyPhotonView.Owner.NickName}!");
                    
                    // Gọi TakeDamage trên enemy - Lấy PhotonView từ GameObject
                    enemyPhotonView.RPC("TakeDamageRPC", RpcTarget.AllBuffered, attackDamage, this.photonView.ViewID);
                }
            }
        }
    }

    // Debug: Vẽ attack range
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}