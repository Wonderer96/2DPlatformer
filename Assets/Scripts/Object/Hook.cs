using UnityEngine;

public class Hook : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask grappleLayer;
    public LayerMask obstacleLayer;

    private GrapplingHook grapplingHook;
    private Rigidbody2D rb;
    private Transform shooter;
    private Vector2 shootDirection;
    private float maxDistance;
    private Vector2 startPosition;
    private bool hasAttached;
    private Transform attachedTransform;

    public void Initialize(GrapplingHook grapplingHook, Vector2 shootDirection, float maxDistance)
    {
        this.grapplingHook = grapplingHook;
        this.shootDirection = shootDirection;
        this.maxDistance = maxDistance;
        this.shooter = grapplingHook.transform;
        this.startPosition = transform.position;

        rb = GetComponent<Rigidbody2D>();
        rb.velocity = shootDirection * grapplingHook.shootSpeed;
    }

    void Update()
    {
        CheckMaxDistance();
    }

    void CheckMaxDistance()
    {
        if (hasAttached) return;

        float traveledDistance = Vector2.Distance(startPosition, transform.position);
        if (traveledDistance >= maxDistance)
        {
            DestroyHook();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasAttached) return;

        if (IsInLayerMask(other.gameObject.layer, obstacleLayer))
        {
            DestroyHook();
            return;
        }

        if (IsInLayerMask(other.gameObject.layer, grappleLayer))
        {
            AttachHook(other.transform);
        }
    }

    bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    void AttachHook(Transform target)
    {
        hasAttached = true;
        attachedTransform = target;

        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        transform.SetParent(target);

        bool isStatic = CheckIfStatic(target);
        grapplingHook.OnHookAttached(transform, isStatic);
    }

    bool CheckIfStatic(Transform target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        return targetRb == null || targetRb.bodyType == RigidbodyType2D.Static;
    }

    void DestroyHook()
    {
        grapplingHook.ReleaseHook();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (grapplingHook != null && grapplingHook.currentHook == gameObject)
        {
            grapplingHook.currentHook = null;
        }
    }
}

