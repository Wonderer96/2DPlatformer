using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [Header("玩家配置")]
    [Tooltip("指定玩家对象的Transform")]
    public Transform playerTransform;

    [Header("全局设置")]
    [SerializeField] private Vector2 mapMinBounds = new Vector2(-100, -100);
    [SerializeField] private Vector2 mapMaxBounds = new Vector2(100, 100);
    [SerializeField] private float defaultSize = 10f;

    [Header("平滑过渡")]
    [Range(0.1f, 2f)] public float positionSmoothTime = 0.3f;
    [Range(0.1f, 2f)] public float sizeSmoothTime = 0.3f;

    public Camera mainCamera;
    public List<CameraZone> activeZones = new List<CameraZone>();
    private Vector3 targetPosition;
    private float targetSize;
    private Vector3 velocity = Vector3.zero;
    private Coroutine activeTransition;
    private CameraZoneFollow currentFollowZone;
    public CameraZone currentZone;

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

    public void SetCurrentFollowZone(CameraZoneFollow zone)
    {
        currentFollowZone = zone;
    }

    public void ClearCurrentFollowZone()
    {
        currentFollowZone = null;
    }

    public CameraZoneFollow GetCurrentFollowZone()
    {
        return currentFollowZone;
    }

    public void RequestZoneSwitch(CameraZone newZone)
    {
        // 如果已经在处理更高优先级的区域则忽略
        if (activeZones.Count > 0 && newZone.priority < activeZones[0].priority)
            return;

        // 取消之前的延迟调用
        if (activeTransition != null)
            StopCoroutine(activeTransition);

        // 立即添加新区域（不再延迟）
        AddZone(newZone);
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

            // 新增：立即应用位置变化
            if (highestPriority is CameraZoneStatic)
            {
                transform.position = targetPosition;
                mainCamera.orthographicSize = targetSize;
            }
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

        target.z = transform.position.z;

        float currentSize = mainCamera.orthographicSize;
        float aspect = mainCamera.aspect;
        float effectiveHeight = currentSize * 2;
        float effectiveWidth = effectiveHeight * aspect;

        Bounds activeBounds = new Bounds();
        if (currentFollowZone != null && currentFollowZone.boundaryTrigger != null)
        {
            // 使用跟随区域的边界触发器
            activeBounds = currentFollowZone.boundaryTrigger.bounds;
        }
        else
        {
            // 使用全局边界
            activeBounds.SetMinMax(
                new Vector3(mapMinBounds.x, mapMinBounds.y, -Mathf.Infinity),
                new Vector3(mapMaxBounds.x, mapMaxBounds.y, Mathf.Infinity)
            );
        }

        float minX = activeBounds.min.x + effectiveWidth / 2;
        float maxX = activeBounds.max.x - effectiveWidth / 2;
        float minY = activeBounds.min.y + effectiveHeight / 2;
        float maxY = activeBounds.max.y - effectiveHeight / 2;

        return new Vector3(
            Mathf.Clamp(target.x, minX, maxX),
            Mathf.Clamp(target.y, minY, maxY),
            target.z
        );
    }

    private void Update()
    {
        if(activeZones.Count !=1 || !activeZones.Contains(currentZone))
        {
            RequestZoneSwitch(currentZone);
        }
    }
    void LateUpdate()
    {
        if (activeZones.Count > 0 && activeZones[0] is CameraZoneFollow)
        {
            UpdateCameraTarget();
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            positionSmoothTime
        );

        mainCamera.orthographicSize = Mathf.Lerp(
            mainCamera.orthographicSize,
            targetSize,
            Time.deltaTime * (activeZones.Count > 0 ?
                activeZones[0].transitionSpeed :
                sizeSmoothTime)
        );
    }

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
    // 在CameraController类中添加
    public void ForceSwitchToStaticZone(CameraZoneStatic staticZone)
    {
        // 清除所有区域
        activeZones.Clear();

        // 强制添加新区域
        AddZone(staticZone);

        // 立即更新目标位置
        UpdateCameraTarget();

        // 强制完成位置过渡
        transform.position = targetPosition;
        mainCamera.orthographicSize = targetSize;

        // 清除所有跟随区域相关状态
        ClearCurrentFollowZone();
    }


    // 修改原有UpdateCameraTarget方法

}
