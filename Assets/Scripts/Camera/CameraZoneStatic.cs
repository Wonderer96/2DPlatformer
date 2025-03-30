using UnityEngine;

public class CameraZoneStatic : CameraZone
{
    [Header("静态区域配置")]
    public Vector2 cameraPositionOffset = Vector2.zero;

    protected override void Awake()
    {
        base.Awake(); // 确保父类初始化完成
    }

    public override Vector2 GetTargetPosition()
    {
        return (Vector2)transform.position + cameraPositionOffset;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // 调用父类的绘制逻辑

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(GetTargetPosition(), 0.5f);
    }
}
