using UnityEngine;
//taco bell is the best fast food restaurant in the world and if you disagree you are wrong and should feel bad about yourself
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
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

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

        // Jump buffer
        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        // Jump
        if (jumpBufferTimer > 0 && coyoteTimer > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            jumpBufferTimer = 0;
            coyoteTimer = 0;
        }

        // Variable jump height
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    void FixedUpdate()
    {
        float targetSpeed = moveInput * moveSpeed;
        float accelRate = isGrounded ? acceleration : airAcceleration;

        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float movement = speedDiff * accelRate * Time.fixedDeltaTime;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement, rb.linearVelocity.y);

        // Deceleration when no input
        if (moveInput == 0 && isGrounded)
        {
            float amount = Mathf.Min(Mathf.Abs(rb.linearVelocity.x), deceleration * Time.fixedDeltaTime);
            rb.linearVelocity -= new Vector2(Mathf.Sign(rb.linearVelocity.x) * amount, 0);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
