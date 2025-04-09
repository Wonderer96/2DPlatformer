// GrapplingHook.cs
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    [Header("Hook Settings")]
    public GameObject hookPrefab;
    public KeyCode hookKey = KeyCode.Mouse0;
    public float shootSpeed = 20f;
    public float pullAcceleration = 15f;
    public float detectionRadius = 5f;
    public float minLockDistance = 0.5f;
    public float staticLockRadius = 0.2f;

    [Header("Layers & Tags")]
    public string priorityTag = "GrapplePoint";

    [Header("References")]
    public Transform shootPoint;
    public LineRenderer lineRenderer;

    public GameObject currentHook;
    private bool isGrappling;
    private Transform hookTransform;
    private Rigidbody2D playerRb;
    private Vector2 shootDirection;
    private bool isStaticTarget;
    private bool isPositionLocked;
    private Vector2 lockPosition;
    private float originalGravityScale;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        originalGravityScale = playerRb.gravityScale;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        HandleInput();
        UpdateLineRenderer();
    }

    void FixedUpdate()
    {
        if (isPositionLocked)
        {
            MaintainLockPosition();
        }
        else
        {
            ApplyGrapplePhysics();
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(hookKey))
        {
            TryShootHook();
        }

        if (Input.GetKeyUp(hookKey))
        {
            ReleaseHook();
        }
    }

    void TryShootHook()
    {
        if (currentHook != null) return;

        Collider2D[] priorityTargets = Physics2D.OverlapCircleAll(transform.position, detectionRadius, -1);
        Transform target = FindNearestPriorityTarget(priorityTargets);

        shootDirection = target != null ?
            (target.position - shootPoint.position).normalized :
            new Vector2(transform.localScale.x, 0);

        currentHook = Instantiate(hookPrefab, shootPoint.position, Quaternion.identity);
        Hook hookComponent = currentHook.GetComponent<Hook>();
        hookComponent.Initialize(this, shootDirection, detectionRadius);

        lineRenderer.enabled = true;
    }

    Transform FindNearestPriorityTarget(Collider2D[] targets)
    {
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D target in targets)
        {
            if (target.CompareTag(priorityTag))
            {
                float distance = Vector2.Distance(transform.position, target.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = target.transform;
                }
            }
        }
        return nearest;
    }

    public void OnHookAttached(Transform hookTransformRef, bool isStatic)
    {
        isGrappling = true;
        hookTransform = hookTransformRef;
        isStaticTarget = isStatic;
    }

    void ApplyGrapplePhysics()
    {
        if (!isGrappling || hookTransform == null) return;

        Vector2 hookPos = hookTransform.position;
        Vector2 playerPos = playerRb.position;
        float currentDistance = Vector2.Distance(playerPos, hookPos);

        if (isStaticTarget)
        {
            HandleStaticPull(hookPos, playerPos, currentDistance);
        }
        else
        {
            HandleDynamicPull(hookPos, currentDistance);
        }
    }

    void HandleStaticPull(Vector2 hookPos, Vector2 playerPos, float currentDistance)
    {
        if (currentDistance > minLockDistance)
        {
            Vector2 pullDirection = (hookPos - playerPos).normalized;
            playerRb.AddForce(pullDirection * pullAcceleration, ForceMode2D.Force);

            if (playerRb.velocity.magnitude < 3f)
            {
                playerRb.velocity = pullDirection * 3f;
            }
        }
        else
        {
            EnterLockState(hookPos);
        }
    }

    void HandleDynamicPull(Vector2 hookPos, float currentDistance)
    {
        Rigidbody2D targetRb = hookTransform.parent.GetComponent<Rigidbody2D>();
        if (targetRb == null) return;

        Vector2 pullDirection = ((Vector2)transform.position - hookPos).normalized;
        targetRb.AddForce(pullDirection * pullAcceleration, ForceMode2D.Force);

        if (currentDistance < minLockDistance)
        {
            ReleaseHook();
        }
    }

    void EnterLockState(Vector2 targetPosition)
    {
        isPositionLocked = true;
        playerRb.gravityScale = 0;
        playerRb.velocity = Vector2.zero;
        lockPosition = targetPosition;
        playerRb.MovePosition(lockPosition);
    }

    void MaintainLockPosition()
    {
        if (!isGrappling || hookTransform == null) return;

        lockPosition = hookTransform.position;

        Vector2 newPos = Vector2.Lerp(playerRb.position, lockPosition, 20 * Time.fixedDeltaTime);
        playerRb.MovePosition(newPos);

        if (Vector2.Distance(playerRb.position, lockPosition) < staticLockRadius)
        {
            playerRb.velocity = Vector2.zero;
        }
    }

    void UpdateLineRenderer()
    {
        if (currentHook == null)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.SetPosition(0, shootPoint.position);
        lineRenderer.SetPosition(1, currentHook.transform.position);
    }

    public void ReleaseHook()
    {
        isGrappling = false;
        isPositionLocked = false;
        playerRb.gravityScale = originalGravityScale;

        if (currentHook != null)
        {
            Destroy(currentHook);
            currentHook = null;
        }
        hookTransform = null;
    }
}
