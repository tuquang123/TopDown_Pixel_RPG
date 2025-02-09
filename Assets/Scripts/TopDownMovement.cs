using UnityEngine;

public class TopDownMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

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
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Nhấn Space để attack
        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetTrigger("2_Attack");
        }

        UpdateAnimation();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput.normalized * moveSpeed;

        // Chỉ xoay trái/phải theo hướng di chuyển
        if (moveInput.x > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0); // Hướng phải
        }
        else if (moveInput.x < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0); // Hướng trái
        }
    }

    void UpdateAnimation()
    {
        bool isMoving = moveInput.magnitude > 0.01f;
        anim.SetBool("1_Move", isMoving);
    }
}