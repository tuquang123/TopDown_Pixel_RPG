using Pattern;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementNGO : NetworkBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        CameraFollow.Instance.target = transform;
    }
    
    void Update()
    {
        if (!IsOwner || !IsSpawned) return;

        // Input giống PlayerController
        float x = Input.GetKey(KeyCode.A) ? -1 :
            Input.GetKey(KeyCode.D) ? 1 : 0;

        float y = Input.GetKey(KeyCode.S) ? -1 :
            Input.GetKey(KeyCode.W) ? 1 : 0;

        moveInput = new Vector2(x, y);

        if (moveInput.magnitude > 1f)
            moveInput.Normalize();
    }

    void FixedUpdate()
    {
        if (!IsOwner || !IsSpawned) return;

        rb.linearVelocity = moveInput * moveSpeed;
    }
}