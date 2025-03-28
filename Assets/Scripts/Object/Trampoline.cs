using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Trampoline : MonoBehaviour
{
    [Header("弹力设置")]
    public float forceMultiplier = 15f;
    public float maxForce = 50f;
    public float minSpeed = 1f;

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
        Vector2 velocity = velocityDetector != null && velocityDetector.trackedRb == rb
            ? velocityDetector.lastRecordedVelocity
            : rb.velocity;

        float speedProjection = Vector2.Dot(velocity, normal);

        //if (speedProjection < minSpeed) return;

        float forceMagnitude = Mathf.Min(
            Mathf.Abs(speedProjection) * forceMultiplier,
            maxForce
        );

        rb.AddForce(-1*normal.normalized * forceMagnitude, ForceMode2D.Impulse);
        PlayBounceEffects();
    }

    private void PlayBounceEffects()
    {
        if (bounceEffect) bounceEffect.Play();
        if (bounceSound) AudioSource.PlayClipAtPoint(bounceSound, transform.position);
    }
}
