using System;
using UnityEngine;
using System.Collections;

public class CameraZoneFollow : CameraZone
{
    [Header("跟随设置")]
    [Range(0.1f, 99f)] public float followSmoothing = 1f;
    [Tooltip("相对于玩家位置的X轴偏移")] public float xOffset = 0f;
    [Tooltip("相对于玩家位置的Y轴偏移")] public float yOffset = 0f;

    [Header("边界限制")]
    [Tooltip("限制摄像机移动范围的触发器区域")]
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
            // 清除所有其他区域
            CameraController.Instance.RemoveZone(this); // 先移除自己（如果已在列表中）
            foreach (var zone in FindObjectsOfType<CameraZone>())
            {
                if (zone != this) CameraController.Instance.RemoveZone(zone);
            }

            // 添加当前区域
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
            // 只有当离开的是当前活动跟随区域时才清除
            if (CameraController.Instance.GetCurrentFollowZone() == this)
            {
                // 延迟一帧清除以避免空状态
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
        yield return null; // 等待一帧确保新区域已添加
        CameraController.Instance.ClearCurrentFollowZone();
    }
}