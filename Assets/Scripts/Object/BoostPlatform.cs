// BoostPlatform.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BoostPlatform : MonoBehaviour
{
    [Header("Boost Settings")]
    public float maxSpeed = 20f;
    public float acceleration = 15f;
    public Vector2 boostDirection = Vector2.right;

    private void OnTriggerStay2D(Collider2D other)
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // ������������߼�
        MainCharacterController player = other.GetComponent<MainCharacterController>();
        if (player != null)
        {
            player.isOnBoostPlatform = true;
            UpdateCharacterFacing(other.transform);
        }

        // ���ļ����߼�
        ApplyAcceleration(rb);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        MainCharacterController player = other.GetComponent<MainCharacterController>();
        if (player != null)
        {
            player.isOnBoostPlatform = false;
        }
    }

    private void ApplyAcceleration(Rigidbody2D rb)
    {
        Vector2 normalizedDir = boostDirection.normalized;
        float currentSpeed = Vector2.Dot(rb.velocity, normalizedDir);

        // �������ٶȣ�������ֱ�����ٶȣ�
        float newSpeed = Mathf.Min(currentSpeed + acceleration * Time.fixedDeltaTime, maxSpeed);
        Vector2 verticalVelocity = rb.velocity - normalizedDir * currentSpeed;

        rb.velocity = normalizedDir * newSpeed + verticalVelocity;
    }

    private void UpdateCharacterFacing(Transform character)
    {
        if (Mathf.Abs(boostDirection.x) > 0.1f)
        {
            Vector3 newScale = character.localScale;
            newScale.x = Mathf.Abs(newScale.x) * Mathf.Sign(boostDirection.x);
            character.localScale = newScale;
        }
    }

    // �ڱ༭�����Զ�����
    private void OnValidate()
    {
        //GetComponent<Collider2D>().isTrigger = true;
        boostDirection = boostDirection.normalized;
    }

    // ���ӻ���ʾ���ٷ���
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)boostDirection * 2f;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(end, 0.2f);
    }
}