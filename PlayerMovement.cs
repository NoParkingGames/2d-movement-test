using UnityEngine;
using UnityEngine.InputSystem; // new Input System namespace

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 70f;
    public float deceleration = 70f;
    public float airAcceleration = 35f;

    [Header("Jump")]
    public float jumpForce = 12f;
    public float jumpCutMultiplier = 0.5f;

    [Header("Coyote Time")]
    public float coyoteTime = 0.1f;
    private float coyoteTimer;

    [Header("Jump Buffer")]
    public float jumpBufferTime = 0.1f;
    private float jumpBufferTimer;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;

    private float moveInput;
    private bool jumpPressed;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Called by the Input System
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        moveInput = input.x; // horizontal movement only
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jumpPressed = true;
            jumpBufferTimer = jumpBufferTime;
        }
        if (context.canceled)
        {
            // Variable jump height
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
            }
        }
    }

    private void Update()
    {
        // Ground check
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Coyote time
        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        // Jump buffer timer countdown
        if (jumpBufferTimer > 0)
            jumpBufferTimer -= Time.deltaTime;

        // Perform jump
        if (jumpBufferTimer > 0 && coyoteTimer > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferTimer = 0;
            coyoteTimer = 0;
        }
    }

    private void FixedUpdate()
    {
        float targetSpeed = moveInput * moveSpeed;
        float accelRate = isGrounded ? acceleration : airAcceleration;

        float speedDiff = targetSpeed - rb.velocity.x;
        float movement = speedDiff * accelRate * Time.fixedDeltaTime;

        rb.velocity = new Vector2(rb.velocity.x + movement, rb.velocity.y);

        // Deceleration when no input
        if (moveInput == 0 && isGrounded)
        {
            float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), deceleration * Time.fixedDeltaTime);
            rb.velocity -= new Vector2(Mathf.Sign(rb.velocity.x) * amount, 0);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
