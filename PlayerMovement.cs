using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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
    public int maxJumps = 2; 
    private int jumpsLeft;
    [Range(0, 1)] public float jumpCutMultiplier = 0.5f;
    public float gravityScale = 3.5f; 
    public float fallGravityMultiplier = 1.5f;

    [Header("Timers")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.1f;
    private float coyoteTimer;
    private float jumpBufferTimer;

    [Header("Juice (Squash & Stretch)")]
    public Transform visualTransform; 
    public float squashAmount = 0.7f; 
    public float stretchAmount = 1.3f;
    public float effectDuration = 0.1f;

    [Header("Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float moveInput; 
    private bool isGrounded;
    private bool wasGrounded; 

    private void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        if (visualTransform == null) visualTransform = transform;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<float>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jumpBufferTimer = jumpBufferTime;
        }

        if (context.canceled && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
        }
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Landing Logic
        if (isGrounded && !wasGrounded)
        {
            StopAllCoroutines(); // Stop any current squash/stretch before starting a new one
            StartCoroutine(ApplyVisualEffect(new Vector3(stretchAmount, squashAmount, 1f)));
            jumpsLeft = maxJumps; 
        }

        // Coyote Time
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        jumpBufferTimer -= Time.deltaTime;

        // Jump Logic
        if (jumpBufferTimer > 0)
        {
            // Can jump if grounded (Coyote Time) OR if we have double jumps left
            if (coyoteTimer > 0 || (jumpsLeft > 1))
            {
                ExecuteJump();
            }
            // Special case: if we are in the air and have exactly 1 jump left (the double jump)
            else if (jumpsLeft == 1 && !isGrounded)
            {
                ExecuteJump();
            }
        }

        wasGrounded = isGrounded;
        ModifyGravity();
    }

    private void ExecuteJump()
    {
        // Reset Y velocity for consistent double jump height
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        
        StopAllCoroutines();
        StartCoroutine(ApplyVisualEffect(new Vector3(squashAmount, stretchAmount, 1f)));

        jumpsLeft--;
        jumpBufferTimer = 0;
        coyoteTimer = 0;
    }

    private IEnumerator ApplyVisualEffect(Vector3 targetScale)
    {
        float elapsed = 0;
        Vector3 initialScale = visualTransform.localScale;

        while (elapsed < effectDuration)
        {
            elapsed += Time.deltaTime;
            visualTransform.localScale = Vector3.Lerp(targetScale, Vector3.one, elapsed / effectDuration);
            yield return null;
        }
        visualTransform.localScale = Vector3.one;
    }

    private void ModifyGravity()
    {
        rb.gravityScale = (rb.velocity.y < 0) ? gravityScale * fallGravityMultiplier : gravityScale;
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
