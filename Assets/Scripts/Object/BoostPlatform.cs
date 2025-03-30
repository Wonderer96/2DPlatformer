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

        // 处理玩家特殊逻辑
        MainCharacterController player = other.GetComponent<MainCharacterController>();
        if (player != null)
        {
            player.isOnBoostPlatform = true;
            UpdateCharacterFacing(other.transform);
        }

        // 核心加速逻辑
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

        // 计算新速度（保留垂直方向速度）
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

    // 在编辑器中自动配置
    private void OnValidate()
    {
        //GetComponent<Collider2D>().isTrigger = true;
        boostDirection = boostDirection.normalized;
    }

    // 可视化显示加速方向
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)boostDirection * 2f;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(end, 0.2f);
    }
}