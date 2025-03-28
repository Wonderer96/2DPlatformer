using System.Collections;
using UnityEngine;

public class MainCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpForce = 7f;
    public float gravityMultiplier = 2f;
    public float groundCheckDistance = 0.2f;
    public float jumpBufferTime = 0.2f; // 跳跃输入缓冲时间

    [Header("Physics Materials")]
    public PhysicMaterial highFriction; // 地面物理材质
    public PhysicMaterial zeroFriction; // 空中物理材质

    private Rigidbody rb;
    private Animator animator;
    private Collider col;
    private bool isGrounded;
    private bool jumpRequest;
    private float jumpBufferCounter;
    private float originalGravity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        col = GetComponent<Collider>();
        originalGravity = Physics.gravity.y;
    }

    void Update()
    {
        // 跳跃输入缓冲
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        UpdateAnimations();
    }

    void FixedUpdate()
    {
        CheckGrounded();
        HandleMovement();
        HandleJump();
        ApplyExtraGravity();
        UpdatePhysicsMaterial();
    }

    void CheckGrounded()
    {
        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f,
                      Vector3.down, out hit, groundCheckDistance + 0.1f);
    }

    void HandleMovement()
    {
        // 仅在地面时接受输入
        if (isGrounded)
        {
            float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            float moveInput = Input.GetAxisRaw("Horizontal") * targetSpeed;

            // 直接控制速度（保持物理响应）
            rb.velocity = new Vector3(moveInput, rb.velocity.y, 0);

            // 角色转向
            if (moveInput != 0)
            {
                transform.rotation = Quaternion.Euler(0, moveInput > 0 ? 0 : 180, 0);
            }
        }
    }

    void HandleJump()
    {
        // 跳跃缓冲检测
        if (jumpBufferCounter > 0 && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, 0);
            animator.SetTrigger("Jump");
            jumpBufferCounter = 0;
        }
    }

    void ApplyExtraGravity()
    {
        if (!isGrounded && rb.velocity.y < 0)
        {
            rb.AddForce(Vector3.up * originalGravity * (gravityMultiplier - 1) * rb.mass);
        }
    }

    void UpdatePhysicsMaterial()
    {
        // 根据状态切换物理材质
        col.material = isGrounded ? highFriction : zeroFriction;
    }

    void UpdateAnimations()
    {
        animator.SetFloat("MoveSpeed", Mathf.Abs(rb.velocity.x));
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rb.velocity.y);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            animator.SetTrigger("Land");
        }
    }

    IEnumerator LandingRoutine()
    {
        animator.SetTrigger("Land");
        yield return new WaitForSeconds(0.1f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position,
            transform.position + Vector3.down * (groundCheckDistance + 0.1f));
    }
}
