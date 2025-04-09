// GrapplingHook.cs
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    [Header("Hook Settings")]
    public GameObject hookPrefab;
    public KeyCode hookKey = KeyCode.Mouse0;
    public float shootSpeed = 20f;
    public float hookForce = 10f;
    public float detectionRadius = 5f;

    [Header("Layers & Tags")]
    public string priorityTag = "GrapplePoint";

    [Header("References")]
    public Transform shootPoint;
    public LineRenderer lineRenderer;

    public GameObject currentHook;
    private bool isGrappling;
    private Transform hookedTransform;
    private Rigidbody2D playerRb;
    private Vector2 shootDirection;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        lineRenderer.enabled = false;
    }

    void Update()
    {
        HandleInput();
        UpdateLineRenderer();
    }

    void FixedUpdate()
    {
        ApplyGrappleForce();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(hookKey))
        {
            if (currentHook == null)
            {
                ShootHook();
            }
        }

        if (Input.GetKeyUp(hookKey))
        {
            ReleaseHook();
        }
    }

    void ShootHook()
    {
        // 优先检测附近有特殊tag的物体
        Collider2D[] priorityTargets = Physics2D.OverlapCircleAll(transform.position, detectionRadius, -1);
        Transform target = FindNearestPriorityTarget(priorityTargets);

        if (target != null)
        {
            shootDirection = (target.position - shootPoint.position).normalized;
        }
        else
        {
            shootDirection = new Vector2(transform.localScale.x, 0);
        }

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

    public void OnHookAttached(Transform hookedTransform, Rigidbody2D hookedRb)
    {
        isGrappling = true;
        this.hookedTransform = hookedTransform;
    }

    void ApplyGrappleForce()
    {
        if (!isGrappling || hookedTransform == null) return;

        Vector2 hookPosition = hookedTransform.position;
        Vector2 directionToHook = (hookPosition - (Vector2)transform.position).normalized;

        Rigidbody2D targetRb = hookedTransform.GetComponent<Rigidbody2D>();
        bool isStatic = targetRb == null || targetRb.bodyType == RigidbodyType2D.Static;

        if (isStatic)
        {
            playerRb.AddForce(directionToHook * hookForce, ForceMode2D.Force);
        }
        else
        {
            Vector2 directionToPlayer = ((Vector2)transform.position - hookPosition).normalized;
            targetRb.AddForce(directionToPlayer * hookForce, ForceMode2D.Force);
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
        if (currentHook != null)
        {
            Destroy(currentHook);
            currentHook = null;
        }
        hookedTransform = null;
    }
}
