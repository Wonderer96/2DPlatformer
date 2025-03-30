using UnityEngine;

public class GravityController : MonoBehaviour
{
    [Header("Gravity Settings")]
    public int gravityDirection = 1;
    public KeyCode reverseGravityKey = KeyCode.L;
    public float gravityMultiplier = 1.5f;

    // References
    public MainCharacterController mainController;
    public Rigidbody2D rb;
    public float originalGravityScale;

    void Start()
    {
        mainController = GetComponent<MainCharacterController>();
        rb = GetComponent<Rigidbody2D>();
        originalGravityScale = rb.gravityScale;
        UpdateLocalGravity();
    }

    void Update()
    {
        if (Input.GetKeyDown(reverseGravityKey))
        {
            ToggleGravity();
        }
    }

    void FixedUpdate()
    {
        ApplyExtraGravity();
    }

    public void ToggleGravity()
    {
        gravityDirection *= -1;
        UpdateLocalGravity();

        Vector3 newScale = mainController.transform.localScale;
        newScale.y = Mathf.Sign(newScale.y) * gravityDirection * mainController.scale;
        mainController.transform.localScale = newScale;
    }

    private void UpdateLocalGravity()
    {
        rb.gravityScale = Mathf.Abs(originalGravityScale) * gravityDirection;

        // 如果存在抓钩引用且正在使用
        if (mainController.grapplingGun != null &&
            mainController.grapplingGun.grappleRope.enabled)
        {
            mainController.grapplingGun.ballRigidbody.gravityScale =
                originalGravityScale * gravityDirection;
        }
    }

    private void ApplyExtraGravity()
    {
        if ((gravityDirection == 1 && rb.velocity.y < 0) ||
            (gravityDirection == -1 && rb.velocity.y > 0))
        {
            rb.AddForce(new Vector2(0, Physics2D.gravity.y * (gravityMultiplier - 1) * rb.mass * gravityDirection));
        }
    }
}
