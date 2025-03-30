// CameraZoneFollow.cs
using UnityEngine;

public class CameraZoneFollow : CameraZone
{
    private Transform playerTransform;

    [Header("跟随设置")]
    [Range(0.1f, 5f)] public float followSmoothing = 1f;
    [Tooltip("相对于玩家位置的X轴偏移")] public float xOffset = 0f;
    [Tooltip("相对于玩家位置的Y轴偏移")] public float yOffset = 0f;

    private Vector2 currentFollowPosition;

    public override Vector2 GetTargetPosition()
    {
        if (playerTransform != null)
        {
            // 计算带偏移的目标位置
            Vector2 targetWithOffset = new Vector2(
                playerTransform.position.x + xOffset,
                playerTransform.position.y + yOffset
            );

            // 应用平滑
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
            currentFollowPosition = GetRawTargetPosition(); // 初始化位置
            base.OnTriggerEnter2D(other);
        }
    }

    // 获取原始目标位置（不包含平滑）
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
            // 绘制偏移辅助线
            Gizmos.color = Color.cyan;
            Vector3 rawPos = GetRawTargetPosition();
            Gizmos.DrawLine(playerTransform.position, rawPos);
            Gizmos.DrawWireSphere(rawPos, 0.3f);

            // 当前平滑位置
            Gizmos.color = Color.green;
            Gizmos.DrawLine(rawPos, currentFollowPosition);
            Gizmos.DrawWireSphere(currentFollowPosition, 0.2f);
        }
    }
}

