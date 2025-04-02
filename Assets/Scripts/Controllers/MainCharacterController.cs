using UnityEngine;

public class MainCharacterController : MonoBehaviour
{
    [Header("Basic Info")]
    public int hP = 3;
    public Transform respawnPoint;

    [Header("Movement Settings")]
    public float walkSpeed = 8f;
    public float runSpeed = 12f;
    public float jumpForce = 13f;
    public float groundCheckDistance = 0.2f;
    public float bottomDistance = 0.2f;
    public float jumpBufferTime = 0.15f;
    public float coyoteTime = 0.1f;
    public float scale = 1;
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

    // Components
    public Rigidbody2D rb;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Collider2D col;

    // Controllers
    [HideInInspector] public RopeController ropeController;
    [HideInInspector] public GravityController gravityController;

    // State variables
    public bool isGrounded;
    public bool isOnBoostPlatform;
    private float jumpBufferCounter;
    private float coyoteTimeCounter;
    public int jumpsRemaining;

    [Header("Grappling Reference")]
    public GrapplingGun grapplingGun;

    [Header("Wall Jump Settings")]
    public float wallJumpForce = 10f;
    public float wallCheckDistance = 0.5f;
    public LayerMask wallLayer; // ����������� groundLayer ����һ���µ� Layer
    private bool isNearWall;
    public Collider2D test; 


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        jumpsRemaining = maxJumps;

        ropeController = GetComponent<RopeController>();
        gravityController = GetComponent<GravityController>();
    }

    void Update()
    {
        HandleInput();
        HandleWallJump();
        UpdateTimers();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        CheckGrounded();
        CheckWall();
        HandleMovement();
        HandleJump();
        UpdatePhysicsMaterial();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            bool isAscending = (gravityController.gravityDirection == 1) ?
                (rb.velocity.y > 0) :
                (rb.velocity.y < 0);

            if (isAscending)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
            }
        }
    }

    private void UpdateTimers()
    {
        jumpBufferCounter -= Time.deltaTime;
        coyoteTimeCounter = isGrounded ? coyoteTime : coyoteTimeCounter - Time.deltaTime;
    }

    public void CheckGrounded()
    {
        Vector2 rayStart = (Vector2)transform.position + new Vector2(0, -bottomDistance);
        Vector2 checkDirection = (gravityController.gravityDirection == 1) ? Vector2.down : Vector2.up;
        RaycastHit2D hit = Physics2D.Raycast(
            rayStart,
            checkDirection,
            groundCheckDistance,
            groundLayer
        );

        Debug.DrawRay(rayStart, checkDirection * groundCheckDistance, Color.red);

        bool wasGrounded = isGrounded;
        isGrounded = hit.collider != null;

        if (isGrounded && !wasGrounded)
        {
            jumpsRemaining = maxJumps;
            coyoteTimeCounter = coyoteTime;
            animator.SetTrigger("Land");
        }
        else if (!isGrounded && wasGrounded)
        {
            if (jumpsRemaining > 0)
            {
                jumpsRemaining--;
            }
        }

        coyoteTimeCounter = isGrounded ? coyoteTime : Mathf.Max(coyoteTimeCounter - Time.fixedDeltaTime, 0);
    }



    public void HandleMovement()
    {
        if (isOnBoostPlatform) return;
        float moveInput = Input.GetAxisRaw("Horizontal");
        bool isGrappling = grapplingGun != null && grapplingGun.grappleRope.enabled;

            // ����Ŀ���ٶȣ���������͵�ǰ״̬���ܲ������ߣ�
            float targetSpeed = moveInput * (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed);
            float acceleration = isGrounded ? groundAcceleration : airAcceleration;

            // ץ��״̬�½��Ϳ���Ȩ
            if (isGrappling)
            {
                // ���ͼ��ٶȺ�Ŀ���ٶȣ��˴������߼���
                acceleration *= 0.2f;
                targetSpeed *= 0.5f;
            }

            // ����������Ӱ���ٶȣ������ٶ���������ֱ�Ӹ���
            float speedIncrement = acceleration * Time.fixedDeltaTime * Mathf.Sign(targetSpeed - rb.velocity.x);

            // �������Ŀ���ٶ��㹻�ӽ�����ֱ������ΪĿ���ٶȣ����⡰�񵴡���
            if (Mathf.Abs(rb.velocity.x - targetSpeed) <= Mathf.Abs(speedIncrement))
            {
                rb.velocity = new Vector2(targetSpeed, rb.velocity.y);
            }
            else
            {
                // ���ӻ�����ٶȣ�ʹ���𽥽ӽ�Ŀ���ٶ�
                rb.velocity = new Vector2(rb.velocity.x + speedIncrement, rb.velocity.y);
            }



        if (moveInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput) * scale, transform.localScale.y, 1);
        }
    }

    private bool CanAirControl()
    {
        float rayLength = 1.3f;
        Vector2 sideRayOrigin = transform.position;
        bool hitSide = Physics2D.Raycast(
            sideRayOrigin,
            Vector2.right * transform.localScale.x,
            rayLength,
            groundLayer
        );
        return !hitSide;
    }

    public void HandleJump()
    {
        bool canJump = (coyoteTimeCounter > 0 || jumpsRemaining > 0);
        if (jumpBufferCounter > 0 && canJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            Vector2 jumpDirection = (gravityController.gravityDirection == 1) ? Vector2.up : Vector2.down;
            rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);

            jumpsRemaining--;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
            animator.SetTrigger("Jump");
        }
    }

    public void UpdatePhysicsMaterial()
    {
        bool isMoving = Mathf.Abs(rb.velocity.x) > 0.1f;
        col.sharedMaterial = (isGrounded && !isMoving) ? highFriction : zeroFriction;
    }

    private void UpdateAnimations()
    {
        animator.SetFloat("MoveSpeed", Mathf.Abs(rb.velocity.x));
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rb.velocity.y * gravityController.gravityDirection);
    }

    public void Respawn()
    {
        hP = 3;
        transform.position = respawnPoint.position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector2 checkDirection = (gravityController != null && gravityController.gravityDirection == 1) ?
            Vector2.down :
            Vector2.up;
        Gizmos.DrawLine(transform.position,
            transform.position + (Vector3)checkDirection * (groundCheckDistance + 0.1f));
    }
    void CheckWall()
    {
        // ���ƫ�ƣ�ʹ���ߴӽ�ɫ�ı�Ե��ʼ���
        Vector2 rayStart = (Vector2)transform.position + new Vector2(transform.localScale.x * 0.5f, 0);
        Vector2 checkDirection = Vector2.right * transform.localScale.x; // ���ݽ�ɫ�泯���������ⷽ��
        RaycastHit2D hit = Physics2D.Raycast(rayStart, checkDirection, wallCheckDistance, wallLayer);

        Debug.DrawRay(rayStart, checkDirection * wallCheckDistance, Color.blue);

        isNearWall = hit.collider != null;
        test = hit.collider;

    }


    void HandleWallJump()
    {
        if (isNearWall && Input.GetKeyDown(KeyCode.Space)&& !isGrounded)
        {
            Vector2 wallJumpDirection = new Vector2(-transform.localScale.x, 1).normalized;
            rb.velocity = Vector2.zero; // �����ǰ�ٶ�
            rb.AddForce(wallJumpDirection * wallJumpForce, ForceMode2D.Impulse);
        }
    }

}
