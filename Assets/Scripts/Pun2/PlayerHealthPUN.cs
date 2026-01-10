using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthPUN : MonoBehaviourPun, IPunObservable
{
    private static readonly int DeathTrigger = Animator.StringToHash("4_Death");
    private static readonly int MoveBool = Animator.StringToHash("1_Move");
    private static readonly int Idle = Animator.StringToHash("Idle");

    [Header("Health Settings")]
    public float maxHealth = 100f;
    
    [Header("UI")]
    public Image healthBarFill;
    public GameObject healthBarCanvas;
    public TMPro.TextMeshProUGUI healthText; // Text hiển thị HP số
    
    // Network variables
    private float health = 100f;
    private bool isDead = false;

    void Start()
    {
        // Khởi tạo health
        if (this.photonView.IsMine)
        {
            health = maxHealth;
            isDead = false;
        }
        
        UpdateHealthBar();
        
        // ✅ Setup health bar
        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(true);
            
            // Nếu health bar là child của player, unparent nó và thêm follow script
            if (healthBarCanvas.transform.parent == transform)
            {
                // Unparent
                healthBarCanvas.transform.SetParent(null);
                
                // Thêm follow script nếu chưa có
                if (!healthBarCanvas.GetComponent<HealthBarFollowPlayer>())
                {
                    var followScript = healthBarCanvas.AddComponent<HealthBarFollowPlayer>();
                    followScript.SetTarget(transform);
                    followScript.offset = new Vector3(0, 1.5f, 0); // Adjust này theo ý bạn
                }
            }
        }
    }

    void Update()
    {
        if (!this.photonView.IsMine) return;
        
        // Test damage bằng phím K
        if (Input.GetKeyDown(KeyCode.K))
        {
            this.photonView.RPC("TakeDamageRPC", RpcTarget.AllBuffered, 20f, this.photonView.ViewID);
        }
    }

    [PunRPC]
    public void TakeDamageRPC(float damage, int attackerViewID)
    {
        Debug.Log($"[TakeDamage] START - Damage: {damage}, Health BEFORE: {health}, IsDead: {isDead}, IsMine: {this.photonView.IsMine}, IsMaster: {PhotonNetwork.IsMasterClient}");
        
        if (isDead)
        {
            Debug.Log($"[TakeDamage] Already dead, ignoring");
            return;
        }
        
        // ✅ CHỈ MasterClient được thay đổi health (authority)
        if (PhotonNetwork.IsMasterClient)
        {
            float oldHealth = health;
            health -= damage;
            
            if (health < 0) health = 0;
            
            Debug.Log($"[TakeDamage] MasterClient: Health changed: {oldHealth} → {health}");
            
            // ✅ Force sync UI cho victim ngay lập tức
            this.photonView.RPC("ForceUpdateUIRPC", RpcTarget.All, health, isDead);
            
            // Check death - ĐẢM BẢO chỉ chết 1 lần
            if (health <= 0 && !isDead)
            {
                isDead = true; // Set ngay để tránh gọi lại
                Debug.Log($"[TakeDamage] Player died! Setting isDead = true");
                
                // Sync isDead ngay
                this.photonView.RPC("ForceUpdateUIRPC", RpcTarget.All, health, isDead);
                
                this.photonView.RPC("OnPlayerDiedRPC", RpcTarget.AllBuffered, attackerViewID);
            }
        }
        
        Debug.Log($"[TakeDamage] END - Health AFTER: {health}, IsDead: {isDead}");
    }
    
    // ✅ RPC để force update UI cho tất cả
    [PunRPC]
    void ForceUpdateUIRPC(float newHealth, bool newIsDead)
    {
        Debug.Log($"[ForceUpdateUI] Received - Health: {newHealth}, IsDead: {newIsDead}, IsMine: {this.photonView.IsMine}");
        
        health = newHealth;
        isDead = newIsDead;
        
        UpdateHealthBarImmediate();
    }
    
    // Update UI ngay lập tức không cần check gì
    void UpdateHealthBarImmediate()
    {
        if (healthBarFill == null)
        {
            Debug.LogError($"[UpdateUI] healthBarFill is NULL on {gameObject.name}!");
            return;
        }
        
        if (healthText == null)
        {
            Debug.LogError($"[UpdateUI] healthText is NULL on {gameObject.name}!");
        }
        
        float fillAmount = health / maxHealth;
        healthBarFill.fillAmount = fillAmount;
        
        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(health)}/{maxHealth}";
        }
        
        Debug.Log($"[UpdateUI] ✅ UI Updated! Fill: {fillAmount}, Text: {Mathf.Ceil(health)}/{maxHealth}, GameObject: {gameObject.name}");
    }

    [PunRPC]
    void OnPlayerDiedRPC(int attackerViewID)
    {
        PhotonView attackerView = PhotonView.Find(attackerViewID);
        string attackerName = attackerView != null ? attackerView.Owner.NickName : "Unknown";
        
        Debug.Log($"[OnPlayerDied] Player {this.photonView.Owner.NickName} was killed by {attackerName}!");
        
        // Disable movement
        if (TryGetComponent<PlayerMovementPUN>(out var movement))
            movement.enabled = false;
        
        // Play death animation
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
            anim.SetTrigger(DeathTrigger);
        
        // ẨN health bar khi chết
        if (healthBarCanvas != null)
            healthBarCanvas.SetActive(false);
        
        // ✅ CHỈ MasterClient schedule respawn (không cần RPC)
        if (PhotonNetwork.IsMasterClient)
        {
            // Cancel any existing respawn
            CancelInvoke(nameof(Respawn));
            
            Debug.Log($"[OnPlayerDied] MasterClient scheduling respawn in 3 seconds...");
            Invoke(nameof(Respawn), 3f);
        }
    }

    void Respawn()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        Debug.Log($"[Respawn] MasterClient starting respawn... Health BEFORE: {health}, IsDead: {isDead}");
        
        // ✅ RESET state
        health = maxHealth;
        isDead = false;
        
        Debug.Log($"[Respawn] State reset - Health: {health}, IsDead: {isDead}");
        
        Vector3 spawnPos = GetRandomSpawnPoint();
        
        Debug.Log($"[Respawn] Calling RespawnRPC for all clients at position: {spawnPos}");
        
        // Sync respawn cho tất cả
        this.photonView.RPC("RespawnRPC", RpcTarget.AllBuffered, spawnPos);
    }

    [PunRPC]
    void RespawnRPC(Vector3 spawnPosition)
    {
        Debug.Log($"[Respawn] RespawnRPC called! Position: {spawnPosition}, IsMine: {this.photonView.IsMine}");
        
        // ✅ RESET health và isDead cho TẤT CẢ
        health = maxHealth;
        isDead = false;
        
        Debug.Log($"[Respawn] Health reset to: {health}");
        
        // Set position
        transform.position = spawnPosition;
        
        // Reset animator
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.ResetTrigger(DeathTrigger);
            anim.SetBool(MoveBool, false);
            anim.SetTrigger(Idle);
        }
        
        // Enable movement
        if (TryGetComponent<PlayerMovementPUN>(out var movement))
            movement.enabled = true;
        
        // ✅ HIỆN lại health bar khi respawn
        if (healthBarCanvas != null)
            healthBarCanvas.SetActive(true);
        
        // ✅ FORCE update UI ngay lập tức
        UpdateHealthBarImmediate();
        
        Debug.Log($"[Respawn] ✅ Player {this.photonView.Owner.NickName} respawned! Health: {health}/{maxHealth}");
    }

    Vector3 GetRandomSpawnPoint()
    {
        return new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0);
    }

    void UpdateHealthBar()
    {
        UpdateHealthBarImmediate();
    }

    public bool IsDead() => isDead;
    public float GetHealth() => health;

    // =========================
    // PHOTON SERIALIZATION
    // =========================
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Owner gửi data
            stream.SendNext(health);
            stream.SendNext(isDead);
            Debug.Log($"[Serialize] SENDING - Health: {health}, IsDead: {isDead}");
        }
        else
        {
            // Remote players nhận data
            float receivedHealth = (float)stream.ReceiveNext();
            bool receivedIsDead = (bool)stream.ReceiveNext();
            
            Debug.Log($"[Serialize] RECEIVING - Health: {receivedHealth}, IsDead: {receivedIsDead}, Current Health: {health}");
            
            // Update local state
            if (health != receivedHealth)
            {
                float oldHealth = health;
                health = receivedHealth;
                Debug.Log($"[Serialize] Health updated: {oldHealth} → {health}");
                UpdateHealthBar();
            }
            
            if (isDead != receivedIsDead)
            {
                isDead = receivedIsDead;
                
                // Update health bar visibility
                if (healthBarCanvas != null)
                    healthBarCanvas.SetActive(!isDead);
                    
                Debug.Log($"[Serialize] IsDead updated to: {isDead}");
            }
        }
    }
}