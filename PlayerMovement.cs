using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 70f;
    public float deceleration = 70f;
    public float airAcceleration = 35f;

    [Header("Jump")]
    public float jumpForce = 12f;
    [Range(0, 1)] public float jumpCutMultiplier = 0.5f;
    public float gravityScale = 3f; // Higher gravity feels "snappier"
    public float fallGravityMultiplier = 1.5f;

    [Header("Timers")]
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    private float coyoteTimer;
    private float jumpBufferTimer;

    [Header("Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float moveInput; 
    private bool isGrounded;
    private bool isJumping;

    private void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
    }

    public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<float>();

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started) jumpBufferTimer = jumpBufferTime;

        if (context.canceled && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
            coyoteTimer = 0; // Prevent double-jumping via coyote time
        }
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Timer Management
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        jumpBufferTimer -= Time.deltaTime;

        // Jump Trigger
        if (jumpBufferTimer > 0 && coyoteTimer > 0 && !isJumping)
        {
            ExecuteJump();
        }

        // Reset jumping state when falling or grounded
        if (isGrounded && rb.velocity.y <= 0) isJumping = false;
        
        // Better Falling: Apply more gravity when falling down
        ModifyGravity();
    }

    private void ExecuteJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpBufferTimer = 0;
        coyoteTimer = 0;
        isJumping = true;
    }

    private void ModifyGravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = gravityScale * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }
    }

    private void FixedUpdate()
    {
        float targetSpeed = moveInput * moveSpeed;
        float accelRate = isGrounded ? (Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration) : airAcceleration;

        float newX = Mathf.MoveTowards(rb.velocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);
        rb.velocity = new Vector2(newX, rb.velocity.y);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
