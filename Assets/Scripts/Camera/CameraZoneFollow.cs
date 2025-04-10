using System;
using UnityEngine;
using System.Collections;

public class CameraZoneFollow : CameraZone
{
    [Header("��������")]
    [Range(0.1f, 99f)] public float followSmoothing = 1f;
    [Tooltip("��������λ�õ�X��ƫ��")] public float xOffset = 0f;
    [Tooltip("��������λ�õ�Y��ƫ��")] public float yOffset = 0f;

    [Header("�߽�����")]
    [Tooltip("����������ƶ���Χ�Ĵ���������")]
    public Collider2D boundaryTrigger;

    private Vector2 currentFollowPosition;

    public override Vector2 GetTargetPosition()
    {
        if (CameraController.Instance != null && CameraController.Instance.playerTransform != null)
        {
            Transform playerTransform = CameraController.Instance.playerTransform;
            Vector2 targetWithOffset = new Vector2(
                playerTransform.position.x + xOffset,
                playerTransform.position.y + yOffset
            );

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
        if (CameraController.Instance != null &&
            CameraController.Instance.playerTransform != null &&
            other.transform == CameraController.Instance.playerTransform)
        {
            // ���������������
            CameraController.Instance.RemoveZone(this); // ���Ƴ��Լ�����������б��У�
            foreach (var zone in FindObjectsOfType<CameraZone>())
            {
                if (zone != this) CameraController.Instance.RemoveZone(zone);
            }

            // ��ӵ�ǰ����
            CameraController.Instance.AddZone(this);
            CameraController.Instance.SetCurrentFollowZone(this);

            currentFollowPosition = GetRawTargetPosition();
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }
protected override void OnTriggerExit2D(Collider2D other)
    {
        if (CameraController.Instance != null &&
            CameraController.Instance.playerTransform != null &&
            other.transform == CameraController.Instance.playerTransform)
        {
            // ֻ�е��뿪���ǵ�ǰ���������ʱ�����
            if (CameraController.Instance.GetCurrentFollowZone() == this)
            {
                // �ӳ�һ֡����Ա����״̬
                StartCoroutine(DelayedClearFollowZone());
            }
        }
    }

    private Vector2 GetRawTargetPosition()
    {
        if (CameraController.Instance != null && CameraController.Instance.playerTransform != null)
        {
            Transform playerTransform = CameraController.Instance.playerTransform;
            return new Vector2(
                playerTransform.position.x + xOffset,
                playerTransform.position.y + yOffset
            );
        }
        return Vector2.zero;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (CameraController.Instance != null && CameraController.Instance.playerTransform != null)
        {
            Transform playerTransform = CameraController.Instance.playerTransform;
            Gizmos.color = Color.cyan;
            Vector3 rawPos = GetRawTargetPosition();
            Gizmos.DrawLine(playerTransform.position, rawPos);
            Gizmos.DrawWireSphere(rawPos, 0.3f);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(rawPos, currentFollowPosition);
            Gizmos.DrawWireSphere(currentFollowPosition, 0.2f);
        }
    }
    private IEnumerator DelayedClearFollowZone()
    {
        yield return null; // �ȴ�һ֡ȷ�������������
        CameraController.Instance.ClearCurrentFollowZone();
    }
}