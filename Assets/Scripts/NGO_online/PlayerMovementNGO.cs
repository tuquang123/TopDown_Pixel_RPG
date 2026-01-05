using Unity.Netcode;
using UnityEngine;
using Pattern;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementNGO : NetworkBehaviour
{
    public float moveSpeed = 5f;

    [Header("Animation")]
    public Animator anim; // gán runtime hoặc prefab có animator
    private static readonly int MoveBool = Animator.StringToHash("1_Move");

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private UltimateJoystick joystick; // joystick trên scene

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (anim == null)
            anim = GetComponentInChildren<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        // chỉ Player local mới lấy joystick
        joystick = FindObjectOfType<UltimateJoystick>();
        if (joystick == null)
            Debug.LogWarning("Joystick not found in scene!");

        CameraFollow.Instance.target = transform;
    }

    void Update()
    {
        if (!IsOwner) return;

        // joystick + keyboard
        float joyX = UltimateJoystick.GetHorizontalAxis("Move");
        float joyY = UltimateJoystick.GetVerticalAxis("Move");

        float keyX = Input.GetKey(KeyCode.A) ? -1 : Input.GetKey(KeyCode.D) ? 1 : 0;
        float keyY = Input.GetKey(KeyCode.S) ? -1 : Input.GetKey(KeyCode.W) ? 1 : 0;

        moveInput = new Vector2(joyX + keyX, joyY + keyY);
        if (moveInput.magnitude > 1f)
            moveInput.Normalize();

        // set animation move
        bool isMoving = moveInput.magnitude > 0.01f;
        if (anim != null)
            anim.SetBool(MoveBool, isMoving);

        // rotate nhân vật theo hướng di chuyển
        if (isMoving)
            RotateCharacter(moveInput.x);
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void RotateCharacter(float direction)
    {
        if (direction < 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction > 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }
}
