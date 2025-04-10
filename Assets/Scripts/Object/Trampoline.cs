using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Trampoline : MonoBehaviour
{
    [Header("��������")]
    public float forceMultiplier = 15f;
    public float maxForce = 50f;
    // ������������Сʩ��������������Զ�С���ٶȽӽ��������ٻ��ܵ������
    public float minForce = 5f;

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
        Vector2 velocity = (velocityDetector != null && velocityDetector.trackedRb == rb)
            ? velocityDetector.lastRecordedVelocity
            : rb.velocity;

        float speedProjection = Vector2.Dot(velocity, normal);

        // ����ԭʼ����������ֵ������ֵ����һ���Ŵ������������Ʋ��������ֵ
        float calculatedForce = Mathf.Min(Mathf.Abs(speedProjection) * forceMultiplier, maxForce);
        // ȷ��ʩ�ӵ�������ΪminForce
        float appliedForce = Mathf.Max(minForce, calculatedForce);

        // ʩ�ӷ������� (Impulse ģʽ)
        rb.AddForce(-normal.normalized * appliedForce, ForceMode2D.Impulse);

        PlayBounceEffects();
    }

    private void PlayBounceEffects()
    {
        if (bounceEffect) bounceEffect.Play();
        if (bounceSound) AudioSource.PlayClipAtPoint(bounceSound, transform.position);
    }
}

