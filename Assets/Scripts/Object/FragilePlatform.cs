using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FragilePlatform : MonoBehaviour
{
    [Header("��������")]
    public GameObject linkedObject;  // �����Ŀ��ӻ�����(�����ж���������)

    [Header("ҡ������")]
    public float shakeDuration = 2.0f;  // ҡ�γ���ʱ��
    public float shakeIntensity = 0.1f; // ҡ��ǿ��
    public float shakeSpeed = 10f;     // ҡ���ٶ�

    [Header("��������")]
    public float collapseDelay = 1.0f;  // ҡ�κ󵽿�ʼ�������ӳ�

    [Header("��������")]
    public float destroyDelay = 2.0f;   // ���������ٵ��ӳ�
    public float respawnDelay = 5.0f;   // ���ٺ��������ӳ�

    private Vector3 originalPosition;   // ԭʼλ��
    private Collider2D platformCollider; // ƽ̨����ײ��
    private Animator linkedAnimator;    // ��������Ķ���������
    private float shakeTimer = 0f;      // ҡ�μ�ʱ��
    private bool isShaking = false;     // �Ƿ�����ҡ��
    private bool isCollapsing = false;  // �Ƿ����ڱ���
    private bool isRespawning = false;  // �Ƿ���������

    void Start()
    {
        // ��ʼ�����
        platformCollider = GetComponent<Collider2D>();
        originalPosition = transform.position;

        // ��ȡ���������Animator
        if (linkedObject != null)
        {
            linkedAnimator = linkedObject.GetComponent<Animator>();
            if (linkedAnimator == null)
            {
                Debug.LogWarning("��������û��Animator�����");
            }
        }
        else
        {
            Debug.LogWarning("û�����ù�������");
        }
    }

    void Update()
    {
        // ����ҡ��Ч��
        if (isShaking && !isCollapsing)
        {
            shakeTimer += Time.deltaTime;

            // ����ҡ��ƫ����
            float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
            float offsetY = Mathf.Cos(Time.time * shakeSpeed * 0.5f) * shakeIntensity * 0.5f;

            // Ӧ��ҡ��Ч��
            transform.position = originalPosition + new Vector3(offsetX, offsetY, 0);

            // ���ҡ���Ƿ����
            if (shakeTimer >= shakeDuration)
            {
                StopShaking();
                StartCollapse();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // ��������ײ(���Ը�����Ҫ�޸ı�ǩ)
        if (collision.gameObject.CompareTag("Player") && !isShaking && !isCollapsing && !isRespawning)
        {
            StartShaking();
        }
    }

    void StartShaking()
    {
        isShaking = true;
        shakeTimer = 0f;
    }

    void StopShaking()
    {
        isShaking = false;
        transform.position = originalPosition; // ����λ��
    }

    void StartCollapse()
    {
        isCollapsing = true;

        // ������������ı�������
        if (linkedAnimator != null)
        {
            linkedAnimator.SetBool("isCollapsing", true);
        }

        // ������ײ��
        if (platformCollider != null)
        {
            platformCollider.enabled = false;
        }

        // �ӳ�����
        Invoke("DestroyPlatform", destroyDelay);
    }

    void DestroyPlatform()
    {
        // ����ƽ̨(�������������٣��Ա�����)
        if (linkedObject != null)
        {
            linkedObject.SetActive(false);
        }

        // ׼������
        Invoke("RespawnPlatform", respawnDelay);
    }

    void RespawnPlatform()
    {
        isRespawning = true;

        // ���¼����������
        if (linkedObject != null)
        {
            linkedObject.SetActive(true);

            // ���ö���״̬
            if (linkedAnimator != null)
            {
                linkedAnimator.SetBool("isCollapsing", false);
            }
        }

        // ����������ײ��
        if (platformCollider != null)
        {
            platformCollider.enabled = true;
        }

        // ��������״̬
        isCollapsing = false;
        isRespawning = false;
    }

    // �ڱ༭���п��ӻ�ҡ�η�Χ
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(shakeIntensity * 2, shakeIntensity, 0));
    }
}
