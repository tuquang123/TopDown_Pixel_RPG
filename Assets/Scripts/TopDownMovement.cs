using UnityEngine;

public class TopDownMovement : MonoBehaviour
{
    private static readonly int Property = Animator.StringToHash("2_Attack");
    private static readonly int Property1 = Animator.StringToHash("1_Move");
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float attackSpeed = 1f; // Tốc độ đánh (1 đòn/giây)
    private float lastAttackTime;

    [SerializeField] private GameObject slashVFX;

    private Vector2 moveInput;
    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        moveInput.x = UltimateJoystick.GetHorizontalAxis( "Move" );
        moveInput.y = UltimateJoystick.GetVerticalAxis( "Move" );

        if (Input.GetKeyDown(KeyCode.Space) && Time.time - lastAttackTime >= 1f / attackSpeed)
        {
            lastAttackTime = Time.time; // Cập nhật thời gian đánh cuối cùng
            anim.SetTrigger(Property);
            slashVFX.SetActive(true);
            Invoke(nameof(DisableVFX), 0.3f); // Tắt VFX nhanh hơn nếu cần
        }
        
        UpdateAnimation();
    }
    void DisableVFX()
    {
        slashVFX.SetActive(false);
    }
    void FixedUpdate()
    {
        rb.linearVelocity = moveInput.normalized * moveSpeed;
        
        if (moveInput.x > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0); // Quay về hướng phải
        }
        else if (moveInput.x < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0); // Quay về hướng trái
        }
    }


    void UpdateAnimation()
    {
        bool isMoving = moveInput.magnitude > 0.01f;
        anim.SetBool(Property1, isMoving);
    }
}