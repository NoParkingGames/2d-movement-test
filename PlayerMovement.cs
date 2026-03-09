using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 70f;
    public float airAcceleration = 35f;

    [Header("Jump")]
    public float jumpForce = 12f;
    public int maxJumps = 2;
    private int jumpsLeft;
    [Range(0, 1)] public float jumpCutMultiplier = 0.5f;
    public float gravityScale = 3.5f;
    public float fallGravityMultiplier = 1.5f;

    [Header("Dash")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool canDash = true;
    private bool isDashing;
    private float dashCooldownTimer;

    [Header("Slide")]
    public float slideSpeedMultiplier = 1.5f;
    private bool isSliding;
    private Vector3 originalVisualPos;

    [Header("Juice (Squash & Stretch)")]
    public Transform visualTransform;
    public float squashAmount = 0.7f;
    public float stretchAmount = 1.3f;
    public float slideSquishAmount = 0.3f; 
    public float effectDuration = 0.1f;

    [Header("UI")]
    public Image dashCooldownFill;

    [Header("Camera Juice")]
    private CameraShake camShake;

    [Header("Detection & Respawn")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public float deathYLevel = -10f;
    private Vector2 checkpoint;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool wasGrounded;
    private float coyoteTimer;
    private float jumpBufferTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        checkpoint = transform.position;
        if (visualTransform == null) visualTransform = transform;
        originalVisualPos = visualTransform.localPosition;
        
        if (Camera.main != null) camShake = Camera.main.GetComponent<CameraShake>();
    }

    public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<float>();

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started) jumpBufferTimer = 0.15f;
        if (context.canceled && rb.velocity.y > 0)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && canDash && !isDashing && Mathf.Abs(moveInput) > 0.1f) 
            StartCoroutine(Dash());
    }

    public void OnSlide(InputAction.CallbackContext context)
    {
        if (context.started && isGrounded) StartSlide();
        if (context.canceled) StopSlide();
    }

    private void Update()
    {
        UpdateDashUI();

        if (isDashing) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // --- LANDING LOGIC ---
        if (isGrounded && !wasGrounded)
        {
            jumpsLeft = maxJumps;
            
            // 1. Play Land Sound
            if (AudioManager.instance != null && rb.velocity.y < -2f)
                AudioManager.instance.PlaySFX(AudioManager.instance.landSound);

            // 2. Visual Juice
            if (!isSliding) StartCoroutine(ApplyVisualEffect(new Vector3(stretchAmount, squashAmount, 1f)));
            
            // 3. Camera Shake
            if (rb.velocity.y < -12f && camShake != null) camShake.Shake(0.1f, 0.1f);
        }

        coyoteTimer = isGrounded ? 0.15f : coyoteTimer - Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;

        if (jumpBufferTimer > 0 && (coyoteTimer > 0 || jumpsLeft > 1 || (jumpsLeft == 1 && !isGrounded)))
            ExecuteJump();

        if (transform.position.y < deathYLevel) Die();
        
        wasGrounded = isGrounded;
        rb.gravityScale = (rb.velocity.y < 0) ? gravityScale * fallGravityMultiplier : gravityScale;
    }

    private void UpdateDashUI()
    {
        if (dashCooldownFill != null)
        {
            if (!canDash)
            {
                dashCooldownTimer += Time.deltaTime;
                dashCooldownFill.fillAmount = dashCooldownTimer / dashCooldown;
            }
            else
            {
                dashCooldownFill.fillAmount = 1;
                dashCooldownTimer = 0;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        float targetSpeed = moveInput * moveSpeed;
        if (isSliding) targetSpeed *= slideSpeedMultiplier;

        float accel = isGrounded ? acceleration : airAcceleration;
        float newX = Mathf.MoveTowards(rb.velocity.x, targetSpeed, accel * Time.fixedDeltaTime);
        rb.velocity = new Vector2(newX, rb.velocity.y);
    }

    private void ExecuteJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpsLeft--;
        jumpBufferTimer = 0;
        coyoteTimer = 0;
        
        // --- JUMP SOUND ---
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(AudioManager.instance.jumpSound);

        if (isSliding) StopSlide();
        StartCoroutine(ApplyVisualEffect(new Vector3(squashAmount, stretchAmount, 1f)));
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        
        rb.velocity = new Vector2(Mathf.Sign(moveInput) * dashForce, 0);

        // --- DASH SOUND & SHAKE ---
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(AudioManager.instance.dashSound);

        if (camShake != null) camShake.ShakeDefault();

        yield return new WaitForSeconds(dashDuration);
        
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void StartSlide()
    {
        isSliding = true;
        visualTransform.localScale = new Vector3(1.5f, slideSquishAmount, 1f);
        float yOffset = (1f - slideSquishAmount) / 2f;
        visualTransform.localPosition = new Vector3(originalVisualPos.x, originalVisualPos.y - yOffset, originalVisualPos.z);
    }

    private void StopSlide()
    {
        isSliding = false;
        visualTransform.localScale = Vector3.one;
        visualTransform.localPosition = originalVisualPos;
    }

    public void Die()
    {
        transform.position = checkpoint;
        rb.velocity = Vector2.zero;
        StopSlide();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Spikes")) Die();
        if (other.CompareTag("Checkpoint")) checkpoint = other.transform.position;
    }

    private IEnumerator ApplyVisualEffect(Vector3 targetScale)
    {
        if (isSliding) yield break;
        float elapsed = 0;
        while (elapsed < effectDuration)
        {
            elapsed += Time.deltaTime;
            visualTransform.localScale = Vector3.Lerp(targetScale, Vector3.one, elapsed / effectDuration);
            yield return null;
        }
        visualTransform.localScale = Vector3.one;
    }
}using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 70f;
    public float airAcceleration = 35f;

    [Header("Jump")]
    public float jumpForce = 12f;
    public int maxJumps = 2;
    private int jumpsLeft;
    [Range(0, 1)] public float jumpCutMultiplier = 0.5f;
    public float gravityScale = 3.5f;
    public float fallGravityMultiplier = 1.5f;

    [Header("Dash")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool canDash = true;
    private bool isDashing;
    private float dashCooldownTimer;

    [Header("Slide")]
    public float slideSpeedMultiplier = 1.5f;
    private bool isSliding;
    private Vector3 originalVisualPos;

    [Header("Juice (Squash & Stretch)")]
    public Transform visualTransform;
    public float squashAmount = 0.7f;
    public float stretchAmount = 1.3f;
    public float slideSquishAmount = 0.3f; 
    public float effectDuration = 0.1f;

    [Header("UI")]
    public Image dashCooldownFill;

    [Header("Camera Juice")]
    private CameraShake camShake;

    [Header("Detection & Respawn")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public float deathYLevel = -10f;
    private Vector2 checkpoint;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool wasGrounded;
    private float coyoteTimer;
    private float jumpBufferTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        checkpoint = transform.position;
        if (visualTransform == null) visualTransform = transform;
        originalVisualPos = visualTransform.localPosition;
        
        if (Camera.main != null) camShake = Camera.main.GetComponent<CameraShake>();
    }

    public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<float>();

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started) jumpBufferTimer = 0.15f;
        if (context.canceled && rb.velocity.y > 0)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && canDash && !isDashing && Mathf.Abs(moveInput) > 0.1f) 
            StartCoroutine(Dash());
    }

    public void OnSlide(InputAction.CallbackContext context)
    {
        if (context.started && isGrounded) StartSlide();
        if (context.canceled) StopSlide();
    }

    private void Update()
    {
        UpdateDashUI();

        if (isDashing) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // --- LANDING LOGIC ---
        if (isGrounded && !wasGrounded)
        {
            jumpsLeft = maxJumps;
            
            // 1. Play Land Sound
            if (AudioManager.instance != null && rb.velocity.y < -2f)
                AudioManager.instance.PlaySFX(AudioManager.instance.landSound);

            // 2. Visual Juice
            if (!isSliding) StartCoroutine(ApplyVisualEffect(new Vector3(stretchAmount, squashAmount, 1f)));
            
            // 3. Camera Shake
            if (rb.velocity.y < -12f && camShake != null) camShake.Shake(0.1f, 0.1f);
        }

        coyoteTimer = isGrounded ? 0.15f : coyoteTimer - Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;

        if (jumpBufferTimer > 0 && (coyoteTimer > 0 || jumpsLeft > 1 || (jumpsLeft == 1 && !isGrounded)))
            ExecuteJump();

        if (transform.position.y < deathYLevel) Die();
        
        wasGrounded = isGrounded;
        rb.gravityScale = (rb.velocity.y < 0) ? gravityScale * fallGravityMultiplier : gravityScale;
    }

    private void UpdateDashUI()
    {
        if (dashCooldownFill != null)
        {
            if (!canDash)
            {
                dashCooldownTimer += Time.deltaTime;
                dashCooldownFill.fillAmount = dashCooldownTimer / dashCooldown;
            }
            else
            {
                dashCooldownFill.fillAmount = 1;
                dashCooldownTimer = 0;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        float targetSpeed = moveInput * moveSpeed;
        if (isSliding) targetSpeed *= slideSpeedMultiplier;

        float accel = isGrounded ? acceleration : airAcceleration;
        float newX = Mathf.MoveTowards(rb.velocity.x, targetSpeed, accel * Time.fixedDeltaTime);
        rb.velocity = new Vector2(newX, rb.velocity.y);
    }

    private void ExecuteJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpsLeft--;
        jumpBufferTimer = 0;
        coyoteTimer = 0;
        
        // --- JUMP SOUND ---
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(AudioManager.instance.jumpSound);

        if (isSliding) StopSlide();
        StartCoroutine(ApplyVisualEffect(new Vector3(squashAmount, stretchAmount, 1f)));
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        
        rb.velocity = new Vector2(Mathf.Sign(moveInput) * dashForce, 0);

        // --- DASH SOUND & SHAKE ---
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(AudioManager.instance.dashSound);

        if (camShake != null) camShake.ShakeDefault();

        yield return new WaitForSeconds(dashDuration);
        
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void StartSlide()
    {
        isSliding = true;
        visualTransform.localScale = new Vector3(1.5f, slideSquishAmount, 1f);
        float yOffset = (1f - slideSquishAmount) / 2f;
        visualTransform.localPosition = new Vector3(originalVisualPos.x, originalVisualPos.y - yOffset, originalVisualPos.z);
    }

    private void StopSlide()
    {
        isSliding = false;
        visualTransform.localScale = Vector3.one;
        visualTransform.localPosition = originalVisualPos;
    }

    public void Die()
    {
        transform.position = checkpoint;
        rb.velocity = Vector2.zero;
        StopSlide();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Spikes")) Die();
        if (other.CompareTag("Checkpoint")) checkpoint = other.transform.position;
    }

    private IEnumerator ApplyVisualEffect(Vector3 targetScale)
    {
        if (isSliding) yield break;
        float elapsed = 0;
        while (elapsed < effectDuration)
        {
            elapsed += Time.deltaTime;
            visualTransform.localScale = Vector3.Lerp(targetScale, Vector3.one, elapsed / effectDuration);
            yield return null;
        }
        visualTransform.localScale = Vector3.one;
    }
}
