using UnityEngine;

public class CameraZoneStatic : CameraZone
{
    [Header("��̬��������")]
    public Vector2 cameraPositionOffset = Vector2.zero;

    protected override void Awake()
    {
        base.Awake(); // ȷ�������ʼ�����
    }

    public override Vector2 GetTargetPosition()
    {
        return (Vector2)transform.position + cameraPositionOffset;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // ���ø���Ļ����߼�

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(GetTargetPosition(), 0.5f);
    }
}
