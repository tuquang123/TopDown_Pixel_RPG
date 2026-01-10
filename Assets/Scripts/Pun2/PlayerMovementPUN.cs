using Photon.Pun;
using UnityEngine;
using Pattern;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementPUN : MonoBehaviourPun, IPunObservable
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Animation")]
    public Animator anim;
    private static readonly int MoveBool = Animator.StringToHash("1_Move");

    private Rigidbody2D rb;
    private Vector2 moveInput;

    // Network state
    private bool netIsMoving = false;
    private bool netFacingLeft = false;

    // Local joystick (chỉ Owner có)
    private UltimateJoystick joystick;

    // Smooth network movement
    private Vector2 networkPosition;
    private float smoothing = 10f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (anim == null)
            anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        // Gán camera + joystick CHỈ cho player local
        if (photonView.IsMine)
        {
            joystick = FindObjectOfType<UltimateJoystick>();
            
            if (CameraFollow.Instance != null)
                CameraFollow.Instance.target = transform;
        }
        else
        {
            // Disable Rigidbody2D cho remote players (vật lý chỉ Owner xử lý)
            rb.isKinematic = true;
        }

        // Khởi tạo network position
        networkPosition = rb.position;
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            // Owner: xử lý input và di chuyển
            ReadInput();
            UpdateAnimationAndVisual();
        }
        else
        {
            // Remote players: smooth interpolation
            SmoothNetworkMovement();
        }
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        // Owner: di chuyển bằng physics
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

        // Update animation
        if (anim != null)
            anim.SetBool(MoveBool, isMoving);

        // Update network state
        netIsMoving = isMoving;

        // Owner tự flip ngay lập tức
        if (isMoving && moveInput.x != 0)
        {
            bool shouldFaceLeft = moveInput.x < 0;
            netFacingLeft = shouldFaceLeft;
            ApplyScale(shouldFaceLeft);
        }
    }

    void ApplyScale(bool faceLeft)
    {
        transform.localScale = faceLeft 
            ? new Vector3(1, 1, 1) 
            : new Vector3(-1, 1, 1);
    }

    // =========================
    // REMOTE PLAYER MOVEMENT
    // =========================
    void SmoothNetworkMovement()
    {
        // Smooth interpolation cho remote players
        rb.position = Vector2.Lerp(rb.position, networkPosition, Time.deltaTime * smoothing);
    }

    // =========================
    // PHOTON SERIALIZATION
    // =========================
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Owner gửi data
            stream.SendNext(rb.position);
            stream.SendNext(netIsMoving);
            stream.SendNext(netFacingLeft);
        }
        else
        {
            // Remote players nhận data
            networkPosition = (Vector2)stream.ReceiveNext();
            bool receivedIsMoving = (bool)stream.ReceiveNext();
            bool receivedFacingLeft = (bool)stream.ReceiveNext();

            // Update animation
            if (anim != null && netIsMoving != receivedIsMoving)
            {
                anim.SetBool(MoveBool, receivedIsMoving);
                netIsMoving = receivedIsMoving;
            }

            // Update facing direction
            if (netFacingLeft != receivedFacingLeft)
            {
                netFacingLeft = receivedFacingLeft;
                ApplyScale(netFacingLeft);
            }
        }
    }
}