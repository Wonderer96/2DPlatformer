// CameraZoneFollow.cs
using UnityEngine;

public class CameraZoneFollow : CameraZone
{
    private Transform playerTransform;

    [Header("��������")]
    [Range(0.1f, 5f)] public float followSmoothing = 1f;
    [Tooltip("��������λ�õ�X��ƫ��")] public float xOffset = 0f;
    [Tooltip("��������λ�õ�Y��ƫ��")] public float yOffset = 0f;

    private Vector2 currentFollowPosition;

    public override Vector2 GetTargetPosition()
    {
        if (playerTransform != null)
        {
            // �����ƫ�Ƶ�Ŀ��λ��
            Vector2 targetWithOffset = new Vector2(
                playerTransform.position.x + xOffset,
                playerTransform.position.y + yOffset
            );

            // Ӧ��ƽ��
            currentFollowPosition = Vector2.Lerp(
                currentFollowPosition,
                targetWithOffset,
                Time.deltaTime * followSmoothing
            );
        }
        return currentFollowPosition;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = other.transform;
            currentFollowPosition = GetRawTargetPosition(); // ��ʼ��λ��
            base.OnTriggerEnter2D(other);
        }
    }

    // ��ȡԭʼĿ��λ�ã�������ƽ����
    private Vector2 GetRawTargetPosition()
    {
        return playerTransform != null ?
            new Vector2(
                playerTransform.position.x + xOffset,
                playerTransform.position.y + yOffset
            ) :
            Vector2.zero;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (playerTransform != null)
        {
            // ����ƫ�Ƹ�����
            Gizmos.color = Color.cyan;
            Vector3 rawPos = GetRawTargetPosition();
            Gizmos.DrawLine(playerTransform.position, rawPos);
            Gizmos.DrawWireSphere(rawPos, 0.3f);

            // ��ǰƽ��λ��
            Gizmos.color = Color.green;
            Gizmos.DrawLine(rawPos, currentFollowPosition);
            Gizmos.DrawWireSphere(currentFollowPosition, 0.2f);
        }
    }
}

