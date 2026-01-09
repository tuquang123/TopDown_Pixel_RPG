using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthPUN : NetworkBehaviour
{
    private static readonly int DeathTrigger = Animator.StringToHash("4_Death");
    private static readonly int MoveBool = Animator.StringToHash("1_Move");
    private static readonly int Idle = Animator.StringToHash("Idle");

    [Header("Health Settings")]
    public float maxHealth = 100f;
    
    [Header("UI")]
    public Image healthBarFill;
    public GameObject healthBarCanvas;
    
    private NetworkVariable<float> health = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    private NetworkVariable<bool> isDead = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            health.Value = maxHealth;
            isDead.Value = false;
        }
        
        health.OnValueChanged += OnHealthChanged;
        isDead.OnValueChanged += OnDeadStateChanged;
        
        UpdateHealthBar();
        
        // ✅ HIỆN health bar cho tất cả khi spawn
        if (healthBarCanvas != null)
            healthBarCanvas.SetActive(true);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        health.OnValueChanged -= OnHealthChanged;
        isDead.OnValueChanged -= OnDeadStateChanged;
    }

    void Update()
    {
        if (!IsOwner) return;
        
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamageServerRpc(20f, OwnerClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage, ulong attackerId)
    {
        if (isDead.Value) return;
        
        health.Value -= damage;
        
        Debug.Log($"Player {OwnerClientId} took {damage} damage. HP: {health.Value}");
        
        if (health.Value <= 0)
        {
            health.Value = 0;
            isDead.Value = true;
            OnPlayerDiedClientRpc(attackerId);
        }
    }

    [ClientRpc]
    void OnPlayerDiedClientRpc(ulong attackerId)
    {
        Debug.Log($"Player {OwnerClientId} was killed by Player {attackerId}!");
        
        if (TryGetComponent<PlayerMovementNGO>(out var movement))
            movement.enabled = false;
        
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
            anim.SetTrigger(DeathTrigger);
        
        if (IsServer)
            Invoke(nameof(Respawn), 3f);
    }

    void Respawn()
    {
        if (!IsServer) return;
        
        health.Value = maxHealth;
        isDead.Value = false;
        
        transform.position = GetRandomSpawnPoint();
        
        RespawnClientRpc();
    }

    [ClientRpc]
    void RespawnClientRpc()
    {
        // Reset animator
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.ResetTrigger(DeathTrigger);
            anim.SetBool(MoveBool, false);
            anim.SetTrigger(Idle);
        }
        
        // Enable movement
        if (TryGetComponent<PlayerMovementNGO>(out var movement))
            movement.enabled = true;
        
        // ✅ HIỆN lại health bar khi respawn
        if (healthBarCanvas != null)
            healthBarCanvas.SetActive(true);
        
        Debug.Log($"Player {OwnerClientId} respawned!");
    }

    Vector3 GetRandomSpawnPoint()
    {
        return new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0);
    }

    void OnHealthChanged(float oldValue, float newValue)
    {
        UpdateHealthBar();
    }

    void OnDeadStateChanged(bool oldValue, bool newValue)
    {
        if (healthBarCanvas == null) return;
        
        // ✅ Chết = ẨN, Sống = HIỆN
        healthBarCanvas.SetActive(!newValue);
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = health.Value / maxHealth;
        }
    }

    public bool IsDead() => isDead.Value;
    public float GetHealth() => health.Value;
}