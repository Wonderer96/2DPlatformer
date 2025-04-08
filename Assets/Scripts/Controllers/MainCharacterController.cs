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

        // ��¼��ʼ����ֵ������� scale �����������ˮƽ����ĳ�ʼ��С
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
        UpdatePhysicsMaterial();
        LimitFallSpeed(); // �����������½��ٶ�
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

            // �������ʱ�����ű仯Ч����X������10%��Y�����10%��0.2���ԭ
            StartCoroutine(LandScaleEffect());
        }
        else if (!isGrounded && wasGrounded)
        {
            if (jumpsRemaining > 0)
            {
                jumpsRemaining--;
            }
        }

        coyoteTimeCounter = isGrounded ? coyoteTime : Mathf.Max(coyoteTimeCounter - Time.fixedDeltaTime, 0);
        if (isGrounded)
        {
            _platformRb = hit.collider.attachedRigidbody;
        }
        else
        {
            _platformRb = null;
        }
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

        // ����Ŀ���ٶȣ���������͵�ǰ״̬���ܲ������ߣ�
        float targetSpeed = moveInput * (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed) + platformSpeed;
        float acceleration = isGrounded ? groundAcceleration : airAcceleration;

        // ץ��״̬�½��Ϳ���Ȩ
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
            // ���ֳ�ʼˮƽ��������ţ�ע�Ᵽ�������ԣ�Mathf.Sign��
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

            jumpsRemaining--;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
            animator.SetTrigger("Jump");

            // ��������ʱ�����ű仯Ч����X�����10%�����ڳ�ʼ���ţ���Y������10%��0.2���ԭ
            StartCoroutine(JumpScaleEffect());
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
        // ���浱ǰ״̬�����ڻ�ԭ���˶��п��ܻᱻ HandleMovement ���� x ���������Բ��� originalScale ��y��
        Vector3 tempScale = transform.localScale;
        // ����ʱ X ����� 10%������ԭʼˮƽ���� scale����Y ������ 10%
        transform.localScale = new Vector3(Mathf.Sign(transform.localScale.x) * scale * 0.9f, tempScale.y * 1.1f, tempScale.z);
        yield return new WaitForSeconds(0.2f);
        // ��ԭΪ��ʼ���ţ���ˮƽ���򱣳ַ����ԣ��� movement �и��£�
        transform.localScale = originalScale;
    }

    // Э�̣����ʱ������Ч��
    private IEnumerator LandScaleEffect()
    {
        // ���浱ǰ״̬
        Vector3 tempScale = transform.localScale;
        // ���ʱ X ������ 10%��Y ����� 10%
        transform.localScale = new Vector3(Mathf.Sign(tempScale.x) * Mathf.Abs(tempScale.x) * 1.1f, tempScale.y * 0.9f, tempScale.z);
        yield return new WaitForSeconds(0.2f);
        // ��ԭΪ��ʼ����
        transform.localScale = originalScale;
    }
}

