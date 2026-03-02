using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float acceleration = 70f;
    [SerializeField] private float deceleration = 80f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;
    [SerializeField] private float fallGravityMultiplier = 2.4f;
    [SerializeField] private float lowJumpGravityMultiplier = 2f;

    [Header("Wall")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float wallSlideSpeed = 1.6f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(10f, 13f);
    [SerializeField] private float wallJumpLockTime = 0.18f;

    [Header("Checks")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.1f, 0.9f);

    private Rigidbody2D rb;
    private float horizontalInput;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private int facingDirection = 1;
    private float wallJumpLockCounter;

    private Vector3 respawnPosition;

    public Vector3 RespawnPosition => respawnPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        respawnPosition = transform.position;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        bool jumpDown = Input.GetKeyDown(KeyCode.Space);
        bool jumpHeld = Input.GetKey(KeyCode.Space);

        UpdateChecks();
        UpdateTimers(jumpDown);
        HandleWallSlide();

        if (jumpBufferCounter > 0f)
        {
            if (CanNormalJump())
            {
                Jump(Vector2.up * jumpForce);
            }
            else if (CanWallJump())
            {
                int wallDir = facingDirection;
                Vector2 force = new Vector2(-wallDir * wallJumpForce.x, wallJumpForce.y);
                Jump(force, true);
            }
        }

        if (!jumpHeld && rb.velocity.y > 0f)
        {
            rb.gravityScale = lowJumpGravityMultiplier;
        }
        else if (rb.velocity.y < 0f)
        {
            rb.gravityScale = fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = 1f;
        }

        if (horizontalInput != 0 && Mathf.Sign(horizontalInput) != facingDirection)
        {
            Flip();
        }
    }

    private void FixedUpdate()
    {
        float targetSpeed = horizontalInput * moveSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        float rate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;

        if (wallJumpLockCounter > 0f)
        {
            wallJumpLockCounter -= Time.fixedDeltaTime;
            return;
        }

        float movement = speedDiff * rate * Time.fixedDeltaTime;
        rb.velocity = new Vector2(rb.velocity.x + movement, rb.velocity.y);
    }

    private void UpdateChecks()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        isTouchingWall = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, groundLayer);

        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
        }
    }

    private void UpdateTimers(bool jumpDown)
    {
        if (!isGrounded)
        {
            coyoteCounter -= Time.deltaTime;
        }

        if (jumpDown)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void HandleWallSlide()
    {
        isWallSliding = isTouchingWall && !isGrounded && horizontalInput == facingDirection && rb.velocity.y < 0f;
        if (isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
        }
    }

    private bool CanNormalJump()
    {
        return coyoteCounter > 0f;
    }

    private bool CanWallJump()
    {
        return isWallSliding || (isTouchingWall && !isGrounded);
    }

    private void Jump(Vector2 force, bool isWallJump = false)
    {
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(force, ForceMode2D.Impulse);

        if (isWallJump)
        {
            wallJumpLockCounter = wallJumpLockTime;
            facingDirection *= -1;
            transform.localScale = new Vector3(facingDirection, 1f, 1f);
        }
    }

    private void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(facingDirection, 1f, 1f);
    }

    public void SetCheckpoint(Vector3 checkpoint)
    {
        respawnPosition = checkpoint;
    }

    public void Respawn()
    {
        rb.velocity = Vector2.zero;
        transform.position = respawnPosition;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
        }
    }
}
