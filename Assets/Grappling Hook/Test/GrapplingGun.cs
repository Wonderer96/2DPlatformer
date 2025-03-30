using UnityEngine;

public class GrapplingGun : MonoBehaviour
{
    // 新增的DragLayer变量
    [Header("Drag Layer Settings:")]
    public LayerMask DragLayer;

    // 其他原有变量保持不变...
    [Header("Scripts:")]
    public GrappleRope grappleRope;
    [Header("Layer Settings:")]
    [SerializeField] private bool grappleToAll = false;
    [SerializeField] private int grappableLayerNumber = 9;

    [Header("Main Camera")]
    public Camera m_camera;
    public GravityController gravityController;

    [Header("Transform Refrences:")]
    public Transform gunHolder;
    public Transform gunPivot;
    public Transform firePoint;
    public Transform targetTransform;
    private Vector2 hitOffset;

    [Header("Rotation:")]
    [SerializeField] private bool rotateOverTime = true;
    [Range(0, 80)][SerializeField] private float rotationSpeed = 4;

    [Header("Distance:")]
    [SerializeField] private bool hasMaxDistance = true;
    [SerializeField] private float maxDistance = 4;

    [Header("Launching")]
    [SerializeField] private bool launchToPoint = true;
    [SerializeField] private LaunchType Launch_Type = LaunchType.Transform_Launch;
    [Range(0, 5)][SerializeField] private float launchSpeed = 5;

    [Header("No Launch To Point")]
    [SerializeField] private bool autoCongifureDistance = false;
    [SerializeField] private float targetDistance = 3;
    [SerializeField] private float targetFrequency = 3;

    private enum LaunchType
    {
        Transform_Launch,
        Physics_Launch,
    }

    [Header("Component Refrences:")]
    public SpringJoint2D m_springJoint2D;

    [HideInInspector] public Vector2 grapplePoint;
    [HideInInspector] public Vector2 DistanceVector;
    Vector2 Mouse_FirePoint_DistanceVector;

    public Rigidbody2D ballRigidbody;

    private void Start()
    {
        grappleRope.enabled = false;
        m_springJoint2D.enabled = false;
        ballRigidbody.gravityScale = 1;
    }

    private void Update()
    {
        Mouse_FirePoint_DistanceVector = m_camera.ScreenToWorldPoint(Input.mousePosition) - gunPivot.position;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            SetGrapplePoint();
        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            if (grappleRope.enabled)
            {
                RotateGun(grapplePoint, false);
            }
            else
            {
                RotateGun(m_camera.ScreenToWorldPoint(Input.mousePosition), false);
            }

            if (launchToPoint && grappleRope.isGrappling)
            {
                if (Launch_Type == LaunchType.Physics_Launch)
                {
                    // 修改后的移动逻辑
                    if (IsTargetDragLayer()&& gravityController.gravityDirection!= -1)
                    {
                        // 将目标物体拉向玩家
                        targetTransform.position = Vector3.Lerp(targetTransform.position, gunHolder.position, Time.deltaTime * launchSpeed);
                    }
                    else
                    {
                        // 原有逻辑：将玩家拉向目标点
                        gunHolder.position = Vector3.Lerp(gunHolder.position, grapplePoint, Time.deltaTime * launchSpeed);
                    }
                }
            }
            if (targetTransform != null)
            {
                grapplePoint = (Vector2)targetTransform.position + hitOffset;
            }

        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            grappleRope.enabled = false;
            m_springJoint2D.enabled = false;
            // 改为使用重力控制器的当前值
            ballRigidbody.gravityScale = gravityController.gravityDirection * gravityController.originalGravityScale;
            targetTransform = null;
            hitOffset = Vector2.zero;
        }
        else
        {
            RotateGun(m_camera.ScreenToWorldPoint(Input.mousePosition), true);
        }
    }

    // 新增的判断方法
    private bool IsTargetDragLayer()
    {
        return targetTransform != null &&
              (DragLayer.value & (1 << targetTransform.gameObject.layer)) != 0;
    }

    // 其他原有方法保持不变...
    void RotateGun(Vector3 lookPoint, bool allowRotationOverTime)
    {
        Vector3 distanceVector = lookPoint - gunPivot.position;

        float angle = Mathf.Atan2(distanceVector.y, distanceVector.x) * Mathf.Rad2Deg;
        if (rotateOverTime && allowRotationOverTime)
        {
            Quaternion startRotation = gunPivot.rotation;
            gunPivot.rotation = Quaternion.Lerp(startRotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.deltaTime * rotationSpeed);
        }
        else
            gunPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void SetGrapplePoint()
    {
        if (Physics2D.Raycast(firePoint.position, Mouse_FirePoint_DistanceVector.normalized))
        {
            RaycastHit2D _hit = Physics2D.Raycast(firePoint.position, Mouse_FirePoint_DistanceVector.normalized);
            if ((_hit.transform.gameObject.layer == grappableLayerNumber || grappleToAll) && ((Vector2.Distance(_hit.point, firePoint.position) <= maxDistance) || !hasMaxDistance))
            {
                grapplePoint = _hit.point;
                DistanceVector = grapplePoint - (Vector2)gunPivot.position;
                grappleRope.enabled = true;
                targetTransform = _hit.transform;
                hitOffset = (Vector2)_hit.point - (Vector2)targetTransform.position;
            }
        }
    }

    public void Grapple()
    {
        if (!launchToPoint && !autoCongifureDistance)
        {
            m_springJoint2D.distance = targetDistance;
            m_springJoint2D.frequency = targetFrequency;
        }

        if (!launchToPoint)
        {
            if (autoCongifureDistance)
            {
                m_springJoint2D.autoConfigureDistance = true;
                m_springJoint2D.frequency = 0;
            }
            m_springJoint2D.connectedAnchor = grapplePoint;
            m_springJoint2D.enabled = true;
        }
        else
        {
            if (Launch_Type == LaunchType.Transform_Launch)
            {
                ballRigidbody.gravityScale = gravityController.gravityDirection * gravityController.originalGravityScale;
                ballRigidbody.velocity = Vector2.zero;
            }
            if (Launch_Type == LaunchType.Physics_Launch)
            {
                // 修改物理连接逻辑
                if (IsTargetDragLayer() && gravityController.gravityDirection != -1)
                {
                    // 当目标是DragLayer时，将连接点设为玩家当前位置
                    m_springJoint2D.connectedAnchor = gunHolder.position;
                }
                else
                {
                    m_springJoint2D.connectedAnchor = grapplePoint;
                }
                m_springJoint2D.distance = 0;
                m_springJoint2D.frequency = launchSpeed;
                m_springJoint2D.enabled = true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (hasMaxDistance)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, maxDistance);
        }
    }
}

