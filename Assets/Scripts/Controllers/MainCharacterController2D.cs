using System.Collections;
using UnityEngine;

public class MainCharacterController2D : MonoBehaviour
{
    
    [Header("Basic Infro")]
    public int hP = 3;
    public Transform respawnPoint;

    [Header("Movement Settings")]
    public float walkSpeed = 8f;
    public float runSpeed = 12f;
    public float jumpForce = 13f;
    public float gravityMultiplier = 1.5f;
    public float groundCheckDistance = 0.2f;
    public float jumpBufferTime = 0.15f;
    public float coyoteTime = 0.1f;
    public int scale = 1;
    public LayerMask groundLayer;

    [Header("Air Control")]
    public float airAcceleration = 25f;
    public float groundAcceleration = 40f;

    [Header("Jump Control")]
    public int maxJumps = 2;
    public float jumpCutMultiplier = 0.5f;

    [Header("Physics Materials")]
    public PhysicsMaterial2D highFriction;
    public PhysicsMaterial2D zeroFriction;

    [Header("Rope Settings")]
    public float ropeMaxLength = 10f;
    public float ropeShootSpeed = 20f;
    public float ropePositionalCorrection = 15f;
    public LineRenderer ropeLine;
    public LayerMask ropeLayerMask;
    public float pullForce = 15f;
    public float snapForce = 30f;
    public float pullSpeed = 8f;
    public float ropeStiffness = 50f;
    public float ropeDamping = 5f;

    [Header("Gravity Settings")]
    public int gravityDirection = 1; // 1=Down, -1=Up
    public KeyCode ropeKey = KeyCode.K;
    public KeyCode reverseGravityKey = KeyCode.L;

    // Private variables
    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D col;
    private bool isGrounded;
    private float jumpBufferCounter;
    private float coyoteTimeCounter;
    private int jumpsRemaining;
    private bool isRopeActive;
    private GameObject collidedObject;
    private Vector2 ropeEndPoint;
    private GameObject hookedObject;
    private float currentRopeLength;
    private Vector2 hitLocalPoint;
    private Rigidbody2D hookedRb;
    private bool isPulling;
    private Vector2 ropeVelocity;
    private float originalGravityScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        jumpsRemaining = maxJumps;
        ropeLine.enabled = false;
        originalGravityScale = rb.gravityScale;
        UpdateLocalGravity();
    }

    void Update()
    {
        // 输入处理
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }

        // 跳跃释放检测
        if (Input.GetKeyUp(KeyCode.Space))
        {
            bool isAscending = (gravityDirection == 1) ?
                (rb.velocity.y > 0) :
                (rb.velocity.y < 0);

            if (isAscending)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
            }
        }

        // 计时器更新
        jumpBufferCounter -= Time.deltaTime;
        coyoteTimeCounter = isGrounded ? coyoteTime : coyoteTimeCounter - Time.deltaTime;

        // 重力反转输入
        if (Input.GetKeyDown(reverseGravityKey))
        {
            ToggleGravity();
        }

        UpdateAnimations();
        HandleRopeInput();
        UpdateRopeVisual();
    }
    void FixedUpdate()
    {
        CheckGrounded();
        HandleMovement();
        HandleJump();
        ApplyExtraGravity();
        UpdatePhysicsMaterial();
        HandleRopePhysics();
    }

    private void ToggleGravity()
    {
        gravityDirection *= -1;
        UpdateLocalGravity();

        // 视觉翻转
        Vector3 newScale = transform.localScale;
        newScale.y = Mathf.Sign(newScale.y) * gravityDirection * scale;
        transform.localScale = newScale;
    }

    private void UpdateLocalGravity()
    {
        // 只修改玩家自身的重力缩放
        rb.gravityScale = Mathf.Abs(originalGravityScale) * gravityDirection;
    }

    void CheckGrounded()
    {
        Vector2 rayStart = transform.position;
        Vector2 checkDirection = (gravityDirection == 1) ? Vector2.down : Vector2.up;

        RaycastHit2D hit = Physics2D.Raycast(
            rayStart,
            checkDirection,
            groundCheckDistance,
            groundLayer
        );

        Debug.DrawRay(rayStart, checkDirection * groundCheckDistance, Color.red);

        bool wasGrounded = isGrounded;
        isGrounded = hit.collider != null;

        if (isGrounded)
        {
            if (!wasGrounded)
            {
                jumpsRemaining = maxJumps;
                coyoteTimeCounter = coyoteTime;
            }
        }
        else
        {
            coyoteTimeCounter = Mathf.Max(coyoteTimeCounter - Time.fixedDeltaTime, 0);
        }
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (isGrounded || CanAirControl())
        {
            float targetSpeed = moveInput * (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed);
            float acceleration = isGrounded ? groundAcceleration : airAcceleration;
            float newSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);
            rb.velocity = new Vector2(newSpeed, rb.velocity.y);
        }

        if (moveInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput) * scale, transform.localScale.y, 1);
        }
    }

    bool CanAirControl()
    {
        float rayLength = 1.1f;
        Vector2 sideRayOrigin = transform.position;
        bool hitSide = Physics2D.Raycast(
            sideRayOrigin,
            Vector2.right * transform.localScale.x,
            rayLength,
            groundLayer
        );
        return !hitSide;
    }

    void HandleJump()
    {
        bool canJump = (coyoteTimeCounter > 0 || jumpsRemaining > 0);
        if (jumpBufferCounter > 0 && canJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            Vector2 jumpDirection = (gravityDirection == 1) ? Vector2.up : Vector2.down;
            rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);

            if (collidedObject != null && collidedObject.CompareTag("Trampoline"))
            {
                Trampoline trampoline = collidedObject.GetComponent<Trampoline>();
                if (trampoline != null)
                {
                    rb.AddForce(jumpDirection * jumpForce * 1.5f, ForceMode2D.Impulse);
                }
            }

            jumpsRemaining--;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
            animator.SetTrigger("Jump");
        }
    }
    void ApplyExtraGravity()
    {
        // 根据当前重力方向应用额外重力
        if ((gravityDirection == 1 && rb.velocity.y < 0) ||
            (gravityDirection == -1 && rb.velocity.y > 0))
        {
            rb.AddForce(new Vector2(0, Physics2D.gravity.y * (gravityMultiplier - 1) * rb.mass * gravityDirection));
        }
    }

    void UpdatePhysicsMaterial()
    {
        bool isMoving = Mathf.Abs(rb.velocity.x) > 0.1f;
        col.sharedMaterial = (isGrounded && !isMoving) ? highFriction : zeroFriction;
    }

    void UpdateAnimations()
    {
        animator.SetFloat("MoveSpeed", Mathf.Abs(rb.velocity.x));
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rb.velocity.y * gravityDirection);
    }

    // 以下为绳索系统（保持原有逻辑）
    void HandleRopeInput()
    {
        if (Input.GetKeyDown(ropeKey)) TryShootRope();
        if (Input.GetKey(ropeKey)) isPulling = isRopeActive;
        if (Input.GetKeyUp(ropeKey)) ReleaseRope();
    }

    void TryShootRope()
    {
        if (isRopeActive) return;

        Vector2 inputDir = GetRopeDirection();
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            inputDir,
            ropeMaxLength,
            ropeLayerMask
        );

        if (hit)
        {
            hitLocalPoint = hit.transform.InverseTransformPoint(hit.point);
            hookedObject = hit.collider.gameObject;
            hookedRb = hookedObject.GetComponent<Rigidbody2D>();
            ropeEndPoint = hit.point;
            currentRopeLength = Vector2.Distance(transform.position, ropeEndPoint);
            isRopeActive = true;
            ropeLine.enabled = true;
        }
    }

    Vector2 GetRopeDirection()
    {
        Vector2 dir = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) dir += Vector2.up;
        if (Input.GetKey(KeyCode.S)) dir += Vector2.down;
        if (Input.GetKey(KeyCode.A)) dir += Vector2.left;
        if (Input.GetKey(KeyCode.D)) dir += Vector2.right;

        return dir != Vector2.zero ? dir.normalized :
            (transform.localScale.x > 0 ? Vector2.right : Vector2.left);
    }

    void HandleRopePhysics()
    {
        if (!isRopeActive || hookedObject == null) return;

        Vector2 currentEnd = GetActualEndPoint();
        Vector2 playerPos = rb.position;
        float currentDistance = Vector2.Distance(playerPos, currentEnd);
        bool shouldConstrain = currentDistance >= ropeMaxLength;

        if (Input.GetKey(ropeKey))
        {
            if (hookedObject.CompareTag("InteractiveObject") && hookedRb != null)
            {
                if (shouldConstrain)
                {
                    Vector2 pullDirection = (playerPos - currentEnd).normalized;
                    hookedRb.MovePosition(Vector2.MoveTowards(currentEnd,
                        playerPos,
                        pullSpeed * Time.fixedDeltaTime));

                    Vector2 targetPos = currentEnd + (playerPos - currentEnd).normalized * ropeMaxLength;
                    rb.MovePosition(Vector2.Lerp(playerPos, targetPos,
                        ropePositionalCorrection * Time.fixedDeltaTime));
                }
                return;
            }
        }

        if (shouldConstrain)
        {
            Vector2 dirToEnd = (currentEnd - playerPos).normalized;
            Vector2 targetPos = currentEnd - dirToEnd * ropeMaxLength;

            if (hookedRb != null)
            {
                float massRatio = rb.mass / (rb.mass + hookedRb.mass);
                rb.MovePosition(Vector2.Lerp(playerPos, targetPos,
                    massRatio * ropePositionalCorrection * Time.fixedDeltaTime));

                Vector2 objTargetPos = playerPos + dirToEnd * ropeMaxLength;
                hookedRb.MovePosition(Vector2.Lerp(currentEnd, objTargetPos,
                    (1 - massRatio) * ropePositionalCorrection * Time.fixedDeltaTime));
            }
            else
            {
                rb.MovePosition(Vector2.Lerp(playerPos, targetPos,
                    ropePositionalCorrection * Time.fixedDeltaTime));
            }
        }
    }

    void ReleaseRope()
    {
        if (!isRopeActive) return;
        ResetRope();
    }

    void ResetRope()
    {
        isRopeActive = false;
        hookedObject = null;
        hookedRb = null;
        ropeLine.enabled = false;
    }

    void UpdateRopeVisual()
    {
        if (!isRopeActive) return;
        ropeLine.SetPosition(0, transform.position);
        ropeLine.SetPosition(1, GetActualEndPoint());
    }

    Vector2 GetActualEndPoint()
    {
        if (hookedObject == null) return Vector2.zero;
        if (hookedObject.CompareTag("InteractiveObject") && hookedRb != null)
        {
            return hookedRb.position;
        }
        return hookedObject.transform.TransformPoint(hitLocalPoint);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector2 checkDirection = (gravityDirection == 1) ? Vector2.down : Vector2.up;
        Gizmos.DrawLine(transform.position,
            transform.position + (Vector3)checkDirection * (groundCheckDistance + 0.1f));
    }
    void Respawn()
    {
        hP = 3;
        this.transform.position = respawnPoint.position;
    }
}