using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Trampoline : MonoBehaviour
{
    [Header("弹力设置")]
    public float forceMultiplier = 15f;
    public float maxForce = 50f;
    // 新增：定义最小施加力，玩家无论以多小的速度接近，都至少会受到这个力
    public float minForce = 5f;

    [Header("速度探测器")]
    [Tooltip("拖入子物体上的速度探测器")]
    public VelocityDetector velocityDetector;

    [Header("效果设置")]
    public ParticleSystem bounceEffect;
    public AudioClip bounceSound;
    public Vector2 normal;
    public float forceMagnitude;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
        if (!rb) return;

        Vector2 normal = collision.contacts[0].normal;

        // 使用探测器记录的速度（如果可用）
        Vector2 velocity = (velocityDetector != null && velocityDetector.trackedRb == rb)
            ? velocityDetector.lastRecordedVelocity
            : rb.velocity;

        float speedProjection = Vector2.Dot(velocity, normal);

        // 计算原始的作用力数值（绝对值乘以一个放大倍数），再限制不超过最大值
        float calculatedForce = Mathf.Min(Mathf.Abs(speedProjection) * forceMultiplier, maxForce);
        // 确保施加的力至少为minForce
        float appliedForce = Mathf.Max(minForce, calculatedForce);

        // 施加反方向力 (Impulse 模式)
        rb.AddForce(-normal.normalized * appliedForce, ForceMode2D.Impulse);

        PlayBounceEffects();
    }

    private void PlayBounceEffects()
    {
        if (bounceEffect) bounceEffect.Play();
        if (bounceSound) AudioSource.PlayClipAtPoint(bounceSound, transform.position);
    }
}

