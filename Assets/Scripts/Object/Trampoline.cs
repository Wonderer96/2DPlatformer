using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Trampoline : MonoBehaviour
{
    [Header("��������")]
    public float forceMultiplier = 15f;
    public float maxForce = 50f;
    public float minSpeed = 1f;

    [Header("�ٶ�̽����")]
    [Tooltip("�����������ϵ��ٶ�̽����")]
    public VelocityDetector velocityDetector;

    [Header("Ч������")]
    public ParticleSystem bounceEffect;
    public AudioClip bounceSound;
    public Vector2 normal;
    public float forceMagnitude;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
        if (!rb) return;

        Vector2 normal = collision.contacts[0].normal;

        // ʹ��̽������¼���ٶȣ�������ã�
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
