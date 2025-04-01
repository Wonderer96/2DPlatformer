using UnityEngine;

public class CameraZoneStatic : CameraZone
{
    [Header("æ≤Ã¨«¯”Ú≈‰÷√")]
    public Vector2 cameraPositionOffset = Vector2.zero;

    protected override void Awake()
    {
        base.Awake();
    }

    public override Vector2 GetTargetPosition()
    {
        return (Vector2)transform.position + cameraPositionOffset;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (CameraController.Instance != null &&
            CameraController.Instance.playerTransform != null &&
            other.transform == CameraController.Instance.playerTransform)
        {
            // «ø÷∆¡¢º¥«–ªª«¯”Ú
            CameraController.Instance.ForceSwitchToStaticZone(this);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(GetTargetPosition(), 0.5f);
    }
}



