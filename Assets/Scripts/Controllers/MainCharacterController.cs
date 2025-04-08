using UnityEngine;
using System.Collections;

public class MainCharacterController : MonoBehaviour
{
    [Header("Basic Info")]
    public int maxHP = 1;
    public int hP = 1;
    public Transform respawnPoint;

    [Header("Movement Settings")]
    public float walkSpeed = 8f;
    public float runSpeed = 12f;
    public float jumpForce = 13f;
    public float groundCheckDistance = 0.2f;
    public float bottomDistance = 0.2f;
    public float jumpBufferTime = 0.15f;
    public float coyoteTime = 0.1f;
    public float scale = 1; // ��ʼˮƽ����ֵ
    public float maxFallSpeed = 20f; // ����������½��ٶ�
    public LayerMask groundLayer;

    [Header("Air Control")]
    public float airAcceleration = 25f;
    public float groundAcceleration = 40f;

    [Header("Jump Control")]
    public int maxJumps = 2;
    public float jumpCutMultiplier = 0.5f;
    private bool hasJumpedFromGround;


    [Header("Jump Tweaks")]
    // ����ɫ�½�ʱ����ʩ�ӵ��������ʣ���ֵԽ���½�Խ�죩
    public float fallMultiplier = 2.5f;
    // ����ɫ������δ������ס��Ծ��ʱ��ʹ�õ��������ʣ�ʹ���������ݣ�
    public float lowJumpMultiplier = 2.0f;
    public float upJumpMultiplier = 1.2f; // �������׶γ�����ס��Ծ��ʱ������������䣨ʹ������ߵ���죩��Ĭ��ֵ�ɸ����������


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
    public float nearWallFallingSpeed = -10f;
    public float wallJumpForce = 10f;
    public float wallCheckDistance = 0.5f;
    public LayerMask wallLayer; // ����������� groundLayer ����һ���µ� Layer
    private bool isNearWall;
    public Collider2D test;

    [Header("Ladder Settings")]
    public float ladderSpeed = 5f;
    public bool isOnLadder;
    private bool wasOnLadderLastFrame;
    private float originalGravityScale;

    private Rigidbody2D _platformRb;

    // �����������ɫ��ʼ����������ֵ�����ڻ�ԭ
    private Vector3 originalScale;

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
        originalGravityScale = rb.gravityScale;

        // ��¼��ʼ����ֵ
        originalScale = transform.localScale;
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
        if (!isOnLadder && wasOnLadderLastFrame)
        {
            rb.gravityScale = originalGravityScale;
            gravityController.enabled = true;
        }

        wasOnLadderLastFrame = isOnLadder;
        CheckGrounded();
        CheckWall();
        HandleMovement();
        HandleJump();

        // Ӧ�ø��������Ծ����Ч�������������½���������
        ApplyBetterJumpGravity();

        UpdatePhysicsMaterial();
        LimitFallSpeed(); // ������������ٶ�
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }

        // ������Ȼ�����ɿ���Ծ���󲿷ּ����ٶȵ��߼�
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
        // ʹ�� Collider2D �� bounds ��������ȡ��ɫ��ײ�еı߽�
        Bounds bounds = col.bounds;
        // ����ײ�еĵײ�ƫ���������봦��⣨�ɸ���ʵ��������� offset��
        Vector2 leftRayOrigin = new Vector2(bounds.min.x, bounds.min.y + 0.1f);
        Vector2 rightRayOrigin = new Vector2(bounds.max.x, bounds.min.y + 0.1f);

        // �ֱ�������Ҳ����·�������
        RaycastHit2D leftHit = Physics2D.Raycast(leftRayOrigin, Vector2.down, groundCheckDistance, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(rightRayOrigin, Vector2.down, groundCheckDistance, groundLayer);

        // ��������һ�����߼�⵽��ײ�ж��Ƿ�ӵ�
        bool wasGrounded = isGrounded;
        isGrounded = (leftHit.collider != null || rightHit.collider != null);

        // ����ԭ���߼������綯��������
        if (isGrounded && !wasGrounded)
        {
            jumpsRemaining = maxJumps - 1; // ������Ծռ��һ��
            hasJumpedFromGround = false;
            coyoteTimeCounter = coyoteTime;
            animator.SetTrigger("Land");
            StartCoroutine(LandScaleEffect());
        }
        else if (!isGrounded && wasGrounded)
        {
            // ��������֮ǰû���������Ǿ�ռ��һ����Ծ����
            if (!hasJumpedFromGround)
            {
                jumpsRemaining--;
                hasJumpedFromGround = true; // ��ֹ���ο۳�
            }
        }

        coyoteTimeCounter = isGrounded ? coyoteTime : Mathf.Max(coyoteTimeCounter - Time.fixedDeltaTime, 0);

        // �����Ҫʹ��ƽ̨���壬�ɼ������һ�����ߵ� collider
        if (isGrounded)
        {
            _platformRb = (leftHit.collider != null ? leftHit.collider.attachedRigidbody : rightHit.collider.attachedRigidbody);
        }
        else
        {
            _platformRb = null;
        }

        // Ҳ���Ի��Ƶ�������
        Debug.DrawRay(leftRayOrigin, Vector2.down * groundCheckDistance, Color.red);
        Debug.DrawRay(rightRayOrigin, Vector2.down * groundCheckDistance, Color.red);
    }


    public void HandleMovement()
    {
        if (isOnBoostPlatform) return;

        // �����ƶ����ȴ���
        if (isOnLadder)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            rb.velocity = new Vector2(moveX * ladderSpeed, moveY * ladderSpeed);
            return;
        }

        float moveInput = Input.GetAxisRaw("Horizontal");
        float platformSpeed = _platformRb ? _platformRb.velocity.x : 0f;
        bool isGrappling = grapplingGun != null && grapplingGun.grappleRope.enabled;

        float targetSpeed = moveInput * (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed) + platformSpeed;
        float acceleration = isGrounded ? groundAcceleration : airAcceleration;

        if (isGrappling)
        {
            acceleration *= 0.2f;
            targetSpeed *= 0.5f;
        }

        float speedIncrement = acceleration * Time.fixedDeltaTime * Mathf.Sign(targetSpeed - rb.velocity.x);

        if (Mathf.Abs(rb.velocity.x - targetSpeed) <= Mathf.Abs(speedIncrement))
        {
            rb.velocity = new Vector2(targetSpeed, rb.velocity.y);
        }
        else
        {
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
        if (isOnLadder)
            return;

        bool canJump = (coyoteTimeCounter > 0 || jumpsRemaining > 0);

        if (jumpBufferCounter > 0 && canJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            Vector2 jumpDirection = (gravityController.gravityDirection == 1) ? Vector2.up : Vector2.down;
            rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);

            // ����ǵ�����Ծ������ coyoteTime���������ñ�־
            if (coyoteTimeCounter > 0)
            {
                hasJumpedFromGround = true;
            }
            else
            {
                jumpsRemaining--;
            }

            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
            animator.SetTrigger("Jump");
            StartCoroutine(JumpScaleEffect());
        }
    }


    // �������������ݡ���������Ծ��ԭ���������Ч��
    private void ApplyBetterJumpGravity()
    {
        if (isGrounded || isOnLadder)
            return;

        float gravityStrength = Mathf.Abs(Physics2D.gravity.y);

        if (gravityController.gravityDirection == 1)
        {
            if (rb.velocity.y > 0) // �����׶�
            {
                // Ӧ�� upJumpMultiplier ���˵���Ч����ȷ�����������ס��Ծ����Ҳ�ܽϿ�ﵽ��ߵ�
                float multiplier = Input.GetKey(KeyCode.Space) ? upJumpMultiplier : lowJumpMultiplier;
                rb.velocity += Vector2.up * (-gravityStrength) * (multiplier - 1) * Time.fixedDeltaTime;
            }
            else if (rb.velocity.y < 0) // ����׶�
            {
                rb.velocity += Vector2.up * (-gravityStrength) * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
        }
        else // gravityController.gravityDirection == -1 (��������)
        {
            if (rb.velocity.y < 0) // �����׶Σ�����
            {
                float multiplier = Input.GetKey(KeyCode.Space) ? upJumpMultiplier : lowJumpMultiplier;
                rb.velocity += Vector2.down * (-gravityStrength) * (multiplier - 1) * Time.fixedDeltaTime;
            }
            else if (rb.velocity.y > 0) // ����׶Σ�����
            {
                rb.velocity += Vector2.down * (-gravityStrength) * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
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
        Vector2 rayStart = (Vector2)transform.position + new Vector2(transform.localScale.x * 0.5f, 0);
        Vector2 checkDirection = Vector2.right * transform.localScale.x;
        RaycastHit2D hit = Physics2D.Raycast(rayStart, checkDirection, wallCheckDistance, wallLayer);

        Debug.DrawRay(rayStart, checkDirection * wallCheckDistance, Color.blue);

        isNearWall = hit.collider != null;
        test = hit.collider;
    }

    void HandleWallJump()
    {
        if (isNearWall && Input.GetKeyDown(KeyCode.Space) && !isGrounded)
        {
            Vector2 wallJumpDirection = new Vector2(-transform.localScale.x, 1).normalized;
            rb.velocity = Vector2.zero;
            rb.AddForce(wallJumpDirection * wallJumpForce, ForceMode2D.Impulse);
        }
    }

    public void EnterLadder()
    {
        isOnLadder = true;
        rb.gravityScale = 0;
        gravityController.enabled = false;
        jumpsRemaining = maxJumps;
    }

    public void ExitLadder()
    {
        isOnLadder = false;
        rb.gravityScale = originalGravityScale;
        gravityController.enabled = true;
    }

    private void LimitFallSpeed()
    {
        if (!isOnLadder)
        {
            // ����ǽʱ�Ҵ����½�״̬���������������������������ֱ�������½��ٶ�Ϊ nearWallFallingSpeed
            if (isNearWall)
            {
                if (gravityController.gravityDirection == 1 && rb.velocity.y < 0)
                {
                    rb.velocity = new Vector2(rb.velocity.x, nearWallFallingSpeed);
                    return;
                }
                else if (gravityController.gravityDirection == -1 && rb.velocity.y > 0)
                {
                    rb.velocity = new Vector2(rb.velocity.x, nearWallFallingSpeed);
                    return;
                }
            }

            // ���е���������ٶ������߼�
            float currentVerticalSpeed = rb.velocity.y;
            float maxAllowedSpeed = -maxFallSpeed * Mathf.Sign(gravityController.gravityDirection);
            if ((gravityController.gravityDirection == 1 && currentVerticalSpeed < -maxFallSpeed) ||
                (gravityController.gravityDirection == -1 && currentVerticalSpeed > maxFallSpeed))
            {
                rb.velocity = new Vector2(rb.velocity.x, maxAllowedSpeed);
            }
        }
    }


    // Э�̣�����ʱ������Ч��
    private IEnumerator JumpScaleEffect()
    {
        Vector3 tempScale = transform.localScale;
        transform.localScale = new Vector3(Mathf.Sign(transform.localScale.x) * scale * 0.9f, tempScale.y * 1.1f, tempScale.z);
        yield return new WaitForSeconds(0.2f);
        transform.localScale = originalScale;
    }

    // Э�̣����ʱ������Ч��
    private IEnumerator LandScaleEffect()
    {
        Vector3 tempScale = transform.localScale;
        transform.localScale = new Vector3(Mathf.Sign(tempScale.x) * Mathf.Abs(tempScale.x) * 1.1f, tempScale.y * 0.9f, tempScale.z);
        yield return new WaitForSeconds(0.2f);
        transform.localScale = originalScale;
    }
}

