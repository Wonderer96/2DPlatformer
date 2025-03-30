using UnityEngine;

public class RopeController : MonoBehaviour
{
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
    public KeyCode ropeKey = KeyCode.K;

    // References
    public MainCharacterController mainController;
    private Rigidbody2D rb;

    // Rope State
    [HideInInspector] public bool isRopeActive;
    private GameObject hookedObject;
    private Vector2 ropeEndPoint;
    private float currentRopeLength;
    private Vector2 hitLocalPoint;
    private Rigidbody2D hookedRb;
    private bool isPulling;

    void Start()
    {
        mainController = GetComponent<MainCharacterController>();
        rb = GetComponent<Rigidbody2D>();
        ropeLine.enabled = false;
    }

    void Update()
    {
        HandleRopeInput();
        UpdateRopeVisual();
    }

    void FixedUpdate()
    {
        HandleRopePhysics();
    }

    private void HandleRopeInput()
    {
        if (Input.GetKeyDown(ropeKey)) TryShootRope();
        if (Input.GetKey(ropeKey)) isPulling = isRopeActive;
        if (Input.GetKeyUp(ropeKey)) ReleaseRope();
    }

    private void TryShootRope()
    {
        if (isRopeActive) return;

        Vector2 inputDir = GetRopeDirection();
        RaycastHit2D hit = Physics2D.Raycast(
            mainController.transform.position,
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
            currentRopeLength = Vector2.Distance(rb.position, ropeEndPoint);
            isRopeActive = true;
            ropeLine.enabled = true;
        }
    }

    private Vector2 GetRopeDirection()
    {
        Vector2 dir = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) dir += Vector2.up;
        if (Input.GetKey(KeyCode.S)) dir += Vector2.down;
        if (Input.GetKey(KeyCode.A)) dir += Vector2.left;
        if (Input.GetKey(KeyCode.D)) dir += Vector2.right;

        return dir != Vector2.zero ? dir.normalized :
            (mainController.transform.localScale.x > 0 ? Vector2.right : Vector2.left);
    }

    private void HandleRopePhysics()
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

    public void ReleaseRope()
    {
        if (!isRopeActive) return;
        ResetRope();
    }

    private void ResetRope()
    {
        isRopeActive = false;
        hookedObject = null;
        hookedRb = null;
        ropeLine.enabled = false;
    }

    private void UpdateRopeVisual()
    {
        if (!isRopeActive) return;
        ropeLine.SetPosition(0, rb.position);
        ropeLine.SetPosition(1, GetActualEndPoint());
    }

    private Vector2 GetActualEndPoint()
    {
        if (hookedObject == null) return Vector2.zero;
        if (hookedObject.CompareTag("InteractiveObject") && hookedRb != null)
        {
            return hookedRb.position;
        }
        return hookedObject.transform.TransformPoint(hitLocalPoint);
    }
}