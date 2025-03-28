// CameraZone.cs
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraZone : MonoBehaviour
{
    [Header("镜头配置")]
    public Vector2 cameraPosition = Vector2.zero;  // 镜头中心点
    [Range(5, 20)] public float cameraSize = 10;   // 正交镜头尺寸
    [Range(0, 100)] public int priority = 0;       // 区域优先级（越大越优先）

    [Header("过渡设置")]
    public float transitionSpeed = 5f;             // 本区域的镜头过渡速度
    public float switchDelay = 0.1f;               // 防抖延迟时间

    private BoxCollider2D zoneCollider;

    void Awake()
    {
        zoneCollider = GetComponent<BoxCollider2D>();
        zoneCollider.isTrigger = true;

        // 自动适配碰撞体尺寸
        if (TryGetComponent(out SpriteRenderer sr))
        {
            zoneCollider.size = sr.size;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 通过延迟调用避免边缘抖动
            CameraController.Instance.RequestZoneSwitch(this);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CameraController.Instance.RemoveZone(this);
        }
    }

    // 在编辑器中可视化区域范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        Gizmos.DrawCube(transform.position + (Vector3)cameraPosition,
            new Vector3(zoneCollider.size.x, zoneCollider.size.y, 1));
    }
}