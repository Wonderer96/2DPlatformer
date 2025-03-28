// CameraZone.cs
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraZone : MonoBehaviour
{
    [Header("��ͷ����")]
    public Vector2 cameraPosition = Vector2.zero;  // ��ͷ���ĵ�
    [Range(5, 20)] public float cameraSize = 10;   // ������ͷ�ߴ�
    [Range(0, 100)] public int priority = 0;       // �������ȼ���Խ��Խ���ȣ�

    [Header("��������")]
    public float transitionSpeed = 5f;             // ������ľ�ͷ�����ٶ�
    public float switchDelay = 0.1f;               // �����ӳ�ʱ��

    private BoxCollider2D zoneCollider;

    void Awake()
    {
        zoneCollider = GetComponent<BoxCollider2D>();
        zoneCollider.isTrigger = true;

        // �Զ�������ײ��ߴ�
        if (TryGetComponent(out SpriteRenderer sr))
        {
            zoneCollider.size = sr.size;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // ͨ���ӳٵ��ñ����Ե����
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

    // �ڱ༭���п��ӻ�����Χ
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        Gizmos.DrawCube(transform.position + (Vector3)cameraPosition,
            new Vector3(zoneCollider.size.x, zoneCollider.size.y, 1));
    }
}