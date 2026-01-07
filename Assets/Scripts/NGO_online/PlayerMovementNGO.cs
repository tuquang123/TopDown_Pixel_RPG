using Unity.Netcode;
using UnityEngine;
using Pattern;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementNGO : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Animation")]
    public Animator anim;
    private static readonly int MoveBool = Animator.StringToHash("1_Move");

    private Rigidbody2D rb;
    private Vector2 moveInput;

    // Network state - Owner có quyền ghi
    private NetworkVariable<bool> netIsMoving = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private NetworkVariable<bool> netFacingLeft = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    // Local joystick (chỉ Owner có)
    private UltimateJoystick joystick;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (anim == null)
            anim = GetComponentInChildren<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Gán camera + joystick CHỈ cho player local
        if (IsOwner)
        {
            joystick = FindObjectOfType<UltimateJoystick>();
            
            if (CameraFollow.Instance != null)
                CameraFollow.Instance.target = transform;
        }
        else
        {
            // Client áp dụng hướng ban đầu ngay khi spawn
            ApplyScale(netFacingLeft.Value);
        }

        // Lắng nghe thay đổi network state
        netIsMoving.OnValueChanged += OnMoveStateChanged;
        netFacingLeft.OnValueChanged += OnFacingChanged;

        // Áp dụng animation ban đầu
        if (anim != null)
            anim.SetBool(MoveBool, netIsMoving.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        netIsMoving.OnValueChanged -= OnMoveStateChanged;
        netFacingLeft.OnValueChanged -= OnFacingChanged;
    }

    void Update()
    {
        if (!IsOwner) return;

        ReadInput();
        UpdateAnimationAndVisual();
        UpdateNetworkState();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        rb.linearVelocity = moveInput * moveSpeed;
    }

    // =========================
    // INPUT
    // =========================
    void ReadInput()
    {
        float joyX = joystick != null ? UltimateJoystick.GetHorizontalAxis("Move") : 0f;
        float joyY = joystick != null ? UltimateJoystick.GetVerticalAxis("Move") : 0f;

        float keyX = Input.GetKey(KeyCode.A) ? -1 : Input.GetKey(KeyCode.D) ? 1 : 0;
        float keyY = Input.GetKey(KeyCode.S) ? -1 : Input.GetKey(KeyCode.W) ? 1 : 0;

        moveInput = new Vector2(joyX + keyX, joyY + keyY);

        if (moveInput.magnitude > 1f)
            moveInput.Normalize();
    }

    // =========================
    // LOCAL ANIMATION & VISUAL
    // =========================
    void UpdateAnimationAndVisual()
    {
        bool isMoving = moveInput.magnitude > 0.01f;

        if (anim != null)
            anim.SetBool(MoveBool, isMoving);

        // Owner tự flip ngay lập tức (không đợi network)
        if (isMoving && moveInput.x != 0)
        {
            bool shouldFaceLeft = moveInput.x < 0;
            ApplyScale(shouldFaceLeft);
        }
    }

    void ApplyScale(bool faceLeft)
    {
        transform.localScale = faceLeft 
            ? new Vector3(-1, 1, 1) 
            : new Vector3(1, 1, 1);
    }

    // =========================
    // NETWORK SYNC
    // =========================
    void UpdateNetworkState()
    {
        bool isMoving = moveInput.magnitude > 0.01f;

        // Cập nhật trạng thái di chuyển
        if (netIsMoving.Value != isMoving)
            netIsMoving.Value = isMoving;

        // Cập nhật hướng khi đang di chuyển
        if (isMoving && moveInput.x != 0)
        {
            bool faceLeft = moveInput.x < 0;
            if (netFacingLeft.Value != faceLeft)
                netFacingLeft.Value = faceLeft;
        }
    }

    // =========================
    // NETWORK CALLBACKS
    // =========================
    void OnMoveStateChanged(bool oldValue, bool newValue)
    {
        // Owner đã tự update animation local rồi
        if (IsOwner) return;

        if (anim != null)
            anim.SetBool(MoveBool, newValue);
    }

    void OnFacingChanged(bool oldValue, bool newValue)
    {
        // Owner đã tự flip local rồi
        if (IsOwner) return;

        // Client áp dụng hướng mới NGAY LẬP TỨC
        ApplyScale(newValue);
    }
}