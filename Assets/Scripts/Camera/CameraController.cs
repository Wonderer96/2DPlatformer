using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [Header("�������")]
    [Tooltip("ָ����Ҷ����Transform")]
    public Transform playerTransform;

    [Header("ȫ������")]
    [SerializeField] private Vector2 mapMinBounds = new Vector2(-100, -100);
    [SerializeField] private Vector2 mapMaxBounds = new Vector2(100, 100);
    [SerializeField] private float defaultSize = 10f;

    [Header("ƽ������")]
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
        // ����Ѿ��ڴ���������ȼ������������
        if (activeZones.Count > 0 && newZone.priority < activeZones[0].priority)
            return;

        // ȡ��֮ǰ���ӳٵ���
        if (activeTransition != null)
            StopCoroutine(activeTransition);

        // ������������򣨲����ӳ٣�
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

            // ����������Ӧ��λ�ñ仯
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
            // ʹ�ø�������ı߽紥����
            activeBounds = currentFollowZone.boundaryTrigger.bounds;
        }
        else
        {
            // ʹ��ȫ�ֱ߽�
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
    // ��CameraController�������
    public void ForceSwitchToStaticZone(CameraZoneStatic staticZone)
    {
        // �����������
        activeZones.Clear();

        // ǿ�����������
        AddZone(staticZone);

        // ��������Ŀ��λ��
        UpdateCameraTarget();

        // ǿ�����λ�ù���
        transform.position = targetPosition;
        mainCamera.orthographicSize = targetSize;

        // ������и����������״̬
        ClearCurrentFollowZone();
    }


    // �޸�ԭ��UpdateCameraTarget����

}
