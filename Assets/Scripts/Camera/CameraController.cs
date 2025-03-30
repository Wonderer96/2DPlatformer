// CameraController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [Header("全局设置")]
    [SerializeField] private Vector2 mapMinBounds = new Vector2(-100, -100);
    [SerializeField] private Vector2 mapMaxBounds = new Vector2(100, 100);
    [SerializeField] private float defaultSize = 10f;

    [Header("平滑过渡")]
    [Range(0.1f, 2f)] public float positionSmoothTime = 0.3f;
    [Range(0.1f, 2f)] public float sizeSmoothTime = 0.3f;

    public Camera mainCamera;
    private List<CameraZone> activeZones = new List<CameraZone>();
    private Vector3 targetPosition;
    private float targetSize;
    private Vector3 velocity = Vector3.zero;
    private Coroutine activeTransition;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        mainCamera = Camera.main;
        targetPosition = transform.position;
        targetSize = defaultSize;
    }

    public void RequestZoneSwitch(CameraZone newZone)
    {
        // 如果已经在处理更高优先级的区域则忽略
        if (activeZones.Count > 0 &&
            newZone.priority < activeZones[0].priority) return;

        // 取消之前的延迟调用
        if (activeTransition != null) StopCoroutine(activeTransition);
        activeTransition = StartCoroutine(SwitchWithDelay(newZone));
    }

    private IEnumerator SwitchWithDelay(CameraZone zone)
    {
        yield return new WaitForSeconds(zone.switchDelay);
        AddZone(zone);
    }

    public void AddZone(CameraZone zone)
    {
        if (!activeZones.Contains(zone))
        {
            activeZones.Add(zone);
            activeZones.Sort((a, b) => b.priority.CompareTo(a.priority));
            UpdateCameraTarget();
        }
    }

    public void RemoveZone(CameraZone zone)
    {
        if (activeZones.Contains(zone))
        {
            activeZones.Remove(zone);
            UpdateCameraTarget();
        }
    }

    private void UpdateCameraTarget()
    {
        if (activeZones.Count > 0)
        {
            CameraZone highestPriority = activeZones[0];
            targetPosition = CalculateClampedPosition(highestPriority);
            targetSize = highestPriority.cameraSize;
        }
        else
        {
            targetPosition = CalculateClampedPosition(null);
            targetSize = defaultSize;
        }
    }

    private Vector3 CalculateClampedPosition(CameraZone zone)
    {
        Vector3 target = zone != null ?
            (Vector3)zone.GetTargetPosition() :
            transform.position;

        // 保持Z轴不变
        target.z = transform.position.z;

        // 边界计算（保持原有逻辑）
        float currentSize = mainCamera.orthographicSize;
        float aspect = mainCamera.aspect;
        float effectiveHeight = currentSize * 2;
        float effectiveWidth = effectiveHeight * aspect;

        float minX = mapMinBounds.x + effectiveWidth / 2;
        float maxX = mapMaxBounds.x - effectiveWidth / 2;
        float minY = mapMinBounds.y + effectiveHeight / 2;
        float maxY = mapMaxBounds.y - effectiveHeight / 2;

        return new Vector3(
            Mathf.Clamp(target.x, minX, maxX),
            Mathf.Clamp(target.y, minY, maxY),
            target.z
        );
    }

    void LateUpdate()
    {
        // 每帧检查是否需要更新目标位置（针对跟随区域）
        if (activeZones.Count > 0 && activeZones[0] is CameraZoneFollow)
        {
            UpdateCameraTarget(); // 持续更新目标位置
        }

        // 位置平滑过渡
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            positionSmoothTime
        );

        // 尺寸平滑过渡（保持原有逻辑）
        mainCamera.orthographicSize = Mathf.Lerp(
            mainCamera.orthographicSize,
            targetSize,
            Time.deltaTime * (activeZones.Count > 0 ?
                activeZones[0].transitionSpeed :
                sizeSmoothTime)
        );
    }

    // 镜头震动扩展功能
    public void ShakeCamera(float duration, float magnitude)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = originalPos.x + Random.Range(-1f, 1f) * magnitude;
            float y = originalPos.y + Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }
}
