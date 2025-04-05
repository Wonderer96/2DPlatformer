using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public abstract class CameraZone : MonoBehaviour
{
    [Header("��������")]
    [Range(1, 20)] public float cameraSize = 10;
    [Range(0, 100)] public int priority = 0;

    [Header("��������")]
    public float transitionSpeed = 5f;
    public float switchDelay = 0.1f;

    protected BoxCollider2D zoneCollider;

    protected virtual void Awake()
    {
        InitializeCollider();
    }

    protected void InitializeCollider()
    {
        zoneCollider = GetComponent<BoxCollider2D>();
        if (zoneCollider == null)
        {
            zoneCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        zoneCollider.isTrigger = true;

        // �Զ�������ײ��ߴ�
        if (TryGetComponent(out SpriteRenderer sr))
        {
            zoneCollider.size = sr.size;
        }
    }

    public abstract Vector2 GetTargetPosition();

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (CameraController.Instance != null &&
            CameraController.Instance.playerTransform != null &&
            other.transform == CameraController.Instance.playerTransform)
        {
            CameraController.Instance.RequestZoneSwitch(this);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (CameraController.Instance != null &&
            CameraController.Instance.playerTransform != null &&
            other.transform == CameraController.Instance.playerTransform)
        {
            CameraController.Instance.RemoveZone(this);
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        // ȷ���ڱ༭ģʽ��Ҳ�ܻ�ȡcollider
        if (zoneCollider == null)
        {
            zoneCollider = GetComponent<BoxCollider2D>();
            if (zoneCollider == null) return;
        }

        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        Gizmos.DrawCube(transform.position, new Vector3(zoneCollider.size.x, zoneCollider.size.y, 1));
    }
}