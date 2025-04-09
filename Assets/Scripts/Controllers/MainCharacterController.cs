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
    public float maxFallSpeed = 20f; // ����½��ٶ�
    public LayerMask groundLayer;

    [Header("Air Control")]
    public float airAcceleration = 25f;
    public float groundAcceleration = 40f;

    [Header("Jump Control")]
    public int maxJumps = 2;
    public float jumpCutMultiplier = 0.5f;
    private bool hasJumpedFromGround;

    [Header("Jump Tweaks")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2.0f;
    public float upJumpMultiplier = 1.2f;

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
    // �Ѳ���ʹ�� nearWallFallingSpeed ����
    public float nearWallFallingSpeed = -10f;
    public float wallJumpForce = 10f;
    public float wallCheckDistance = 0.5f;
    public LayerMask wallLayer;
    private bool isNearWall;
    public Collider2D test;

    [Header("Ladder Settings")]
    public float ladderSpeed = 5f;
    public bool isOnLadder;
    private bool wasOnLadderLastFrame;
    private float originalGravityScale;

    private Rigidbody2D _platformRb;
    private Vector3 originalScale;

    // ���������������ܿ��Ʊ���
    [Header("Climb Settings")]
    public bool canClimb = false;  // ������Ϊ true ʱ��������
    private bool isClimbing = false; // ��¼�Ƿ���������״̬
    private Vector3 lastClimbObjectPos; // ���ڱ�������������һ֡��λ��
    private Transform originalParent;

    // ���ڼ�¼��ǰ��ɫ�Ƿ�������ࣨ��ת״̬��
    private bool isFacingLeft = false;


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

        ApplyBetterJumpGravity();
        UpdatePhysicsMaterial();

        // ���µ� LimitFallSpeed ����ȡ��ԭ�й���
        LimitFallSpeed();
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
        Bounds bounds = col.bounds;
        Vector2 leftRayOrigin = new Vector2(bounds.min.x, bounds.min.y + 0.1f);
        Vector2 rightRayOrigin = new Vector2(bounds.max.x, bounds.min.y + 0.1f);

        RaycastHit2D leftHit = Physics2D.Raycast(leftRayOrigin, Vector2.down, groundCheckDistance, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(rightRayOrigin, Vector2.down, groundCheckDistance, groundLayer);

        bool wasGrounded = isGrounded;
        isGrounded = (leftHit.collider != null || rightHit.collider != null);

        if (isGrounded && !wasGrounded)
        {
            jumpsRemaining = maxJumps - 1;
            hasJumpedFromGround = false;
            coyoteTimeCounter = coyoteTime;
            animator.SetTrigger("Land");
            StartCoroutine(LandScaleEffect());
        }
        else if (!isGrounded && wasGrounded)
        {
            if (!hasJumpedFromGround)
            {
                jumpsRemaining--;
                hasJumpedFromGround = true;
            }
        }

        coyoteTimeCounter = isGrounded ? coyoteTime : Mathf.Max(coyoteTimeCounter - Time.fixedDeltaTime, 0);

        if (isGrounded)
        {
            _platformRb = (leftHit.collider != null ? leftHit.collider.attachedRigidbody : rightHit.collider.attachedRigidbody);
        }
        else
        {
            _platformRb = null;
        }

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

        // ����ˮƽ����ʱ���� isFacingLeft ������ˮƽ scale
        if (moveInput != 0)
        {
            isFacingLeft = moveInput < 0;
            transform.localScale = new Vector3((isFacingLeft ? -scale : scale), transform.localScale.y, 1);
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

    private void ApplyBetterJumpGravity()
    {
        if (isGrounded || isOnLadder)
            return;

        float gravityStrength = Mathf.Abs(Physics2D.gravity.y);

        if (gravityController.gravityDirection == 1)
        {
            if (rb.velocity.y > 0) // �����׶�
            {
                float multiplier = Input.GetKey(KeyCode.Space) ? upJumpMultiplier : lowJumpMultiplier;
                rb.velocity += Vector2.up * (-gravityStrength) * (multiplier - 1) * Time.fixedDeltaTime;
            }
            else if (rb.velocity.y < 0) // ����׶�
            {
                rb.velocity += Vector2.up * (-gravityStrength) * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
        }
        else // ��������
        {
            if (rb.velocity.y < 0)
            {
                float multiplier = Input.GetKey(KeyCode.Space) ? upJumpMultiplier : lowJumpMultiplier;
                rb.velocity += Vector2.down * (-gravityStrength) * (multiplier - 1) * Time.fixedDeltaTime;
            }
            else if (rb.velocity.y > 0)
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

    // ���ǽ�壬�����߼����ǽ����ײ��
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
            // ��ȡǽ������Զ��ǽ�ڣ�
            isFacingLeft = transform.localScale.x > 0; // �����ǰ�����ң���ǽ����Ӧ��������

            // ���½�ɫ����
            transform.localScale = new Vector3((isFacingLeft ? -scale : scale), transform.localScale.y, 1);
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

    // �µ� LimitFallSpeed �������滻ԭ�н�ǽ�½��ٶȹ��ܣ�ʵ�ֵ� canClimb Ϊ true �Ұ�ס J ʱ�����⵽��ǽ��λ�ƣ���������
    // �޸ĺ�� LimitFallSpeed �����������������ø�����ķ�ʽʵ����ȫͬ���ƶ�
    private void LimitFallSpeed()
    {
        if (!isOnLadder)
        {
            // ����ģʽ���� canClimb Ϊ true���Ҽ�⵽ǽ�壬������ҳ�����ס J ��ִ��
            if (canClimb && isNearWall && Input.GetKey(KeyCode.J))
            {
                // �ս�������״̬ʱ�����ø�����Ϊ��⵽��ǽ����󣬲���������
                if (!isClimbing)
                {
                    isClimbing = true;
                    originalParent = transform.parent; // ����ԭʼ������
                    if (test != null)
                        transform.parent = test.transform; // �������ø�����
                    rb.gravityScale = 0;
                    gravityController.enabled = false;
                    rb.velocity = Vector2.zero; // ��������ٶ�
                }
                return; // �����ڼ��������ִ�������߼�
            }
            else
            {
                // ���֮ǰ�Ѿ���������״̬������������������ָ�ԭ״
                if (isClimbing)
                {
                    isClimbing = false;
                    transform.parent = originalParent; // ��ԭԭʼ������
                    rb.gravityScale = originalGravityScale;
                    gravityController.enabled = true;
                }
            }

            // �����������ԭ�е���������ٶ������߼�
            float currentVerticalSpeed = rb.velocity.y;
            float maxAllowedSpeed = -maxFallSpeed * Mathf.Sign(gravityController.gravityDirection);
            if ((gravityController.gravityDirection == 1 && currentVerticalSpeed < -maxFallSpeed) ||
                (gravityController.gravityDirection == -1 && currentVerticalSpeed > maxFallSpeed))
            {
                rb.velocity = new Vector2(rb.velocity.x, maxAllowedSpeed);
            }
        }
    }


    // ע�⣺���� originalScale ���� Start �м�¼�ĳ�ʼ transform.localScale ��ֵ
    // �� scale ���㹫������Ŀ���ˮƽ�����С����ֵ

    private IEnumerator JumpScaleEffect()
    {
        // �������� scale �� scale �����������жϽ�ɫ��ת״̬
        float baseX = isFacingLeft ? -scale : scale;
        // ʹ�� originalScale �� y��z ������Ϊ����
        float baseY = originalScale.y;
        float baseZ = originalScale.z;

        // Ӧ����Ծʱ������Ч������������� 0.9 �������������� 1.1 ��
        transform.localScale = new Vector3(baseX * 0.9f, baseY * 1.1f, baseZ);

        yield return new WaitForSeconds(0.2f);

        // Ч�������󣬻ָ�Ϊ��׼��ֵ
        transform.localScale = new Vector3(baseX, baseY, baseZ);
    }

    private IEnumerator LandScaleEffect()
    {
        float baseX = isFacingLeft ? -scale : scale;
        float baseY = originalScale.y;
        float baseZ = originalScale.z;

        // Ӧ����½ʱ������Ч��������Ŵ��� 1.1 ����������С�� 0.9 ��
        transform.localScale = new Vector3(baseX * 1.1f, baseY * 0.9f, baseZ);

        yield return new WaitForSeconds(0.2f);

        transform.localScale = new Vector3(baseX, baseY, baseZ);
    }


}



