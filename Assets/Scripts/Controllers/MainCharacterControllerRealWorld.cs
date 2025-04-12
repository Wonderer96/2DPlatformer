using UnityEngine;

public class BasicMovementControllerRealWorld : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 8f;
    public float runSpeed = 12f;
    public float acceleration = 40f;
    public float scale = 1;
    public LayerMask groundLayer;

    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D col;

    // State
    private bool isGrounded;
    private bool isFacingLeft;
    private Vector3 originalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        UpdateAnimations();
        HandleFlipping();
    }

    void FixedUpdate()
    {
        CheckGrounded();
        HandleMovement();
    }

    private void CheckGrounded()
    {
        Bounds bounds = col.bounds;
        Vector2 leftOrigin = new Vector2(bounds.min.x, bounds.min.y + 0.1f);
        Vector2 rightOrigin = new Vector2(bounds.max.x, bounds.min.y + 0.1f);

        isGrounded = Physics2D.Raycast(leftOrigin, Vector2.down, 0.2f, groundLayer) ||
                     Physics2D.Raycast(rightOrigin, Vector2.down, 0.2f, groundLayer);
    }

    private void HandleMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        float targetSpeed = moveInput * (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed);
        float speedDifference = targetSpeed - rb.velocity.x;

        rb.AddForce(Vector2.right * speedDifference * acceleration);
    }

    private void HandleFlipping()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        if (moveInput != 0)
        {
            isFacingLeft = moveInput < 0;
            transform.localScale = new Vector3((isFacingLeft ? -scale : scale), originalScale.y, 1);
        }
    }

    private void UpdateAnimations()
    {
        animator.SetFloat("MoveSpeed", Mathf.Abs(rb.velocity.x));
        animator.SetBool("isGrounded", isGrounded);
    }
}