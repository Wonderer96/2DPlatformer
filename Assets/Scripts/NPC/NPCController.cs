// NPCController3D.cs
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class NPCController : MonoBehaviour
{
    [Header("��������")]
    public Animator animator;
    public string movingAnim = "Main_Moving";
    public string idleAnim = "Main_Idle";
    [Range(0, 0.3f)] public float animTransitionTime = 0.1f;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;
    public TextMeshProUGUI dialogue;

    private Rigidbody rb;
    private bool isGrounded;
    private Coroutine moveCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        CheckGroundStatus();
        UpdateAnimations();
        //UpdateAnimations();
    }

    public void MoveTo(Vector3 targetPosition)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = StartCoroutine(MoveToPosition(targetPosition));
    }

    private IEnumerator MoveToPosition(Vector3 target)
    {
        Vector3 startPos = transform.position;
        Vector3 direction = (target - startPos).normalized;
        float distance = Vector3.Distance(startPos, target);
        float duration = distance / moveSpeed;

        // 2D�ӽǵ��������
        if (direction.x != 0)
        {
            // ���ݷ���ת��ɫ�ĳ���2D�ӽǣ�
            float scaleX = Mathf.Sign(direction.x); // ��ȡ������ţ�1��-1��
            transform.localScale = new Vector3(0.2f * scaleX, 0.2f, 0.2f); // ��תX��
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            rb.MovePosition(Vector3.Lerp(startPos, target, t));
            yield return null;
        }
        moveCoroutine = null;
        moveCoroutine = null;
    }

    public void Jump(float force)
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * force, ForceMode.Impulse);
            animator.SetTrigger("Jump");
        }
    }

    public void PlayAnimation(string animationName)
    {
        animator.Play(animationName);
    }

    private void CheckGroundStatus()
    {
        isGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            groundCheckDistance + 0.1f,
            groundLayer
        );
    }

   // private void UpdateAnimations()
    //{
        //animator.SetBool("IsGrounded", isGrounded);
        //animator.SetFloat("VerticalVelocity", rb.velocity.y);
        //animator.SetFloat("MoveSpeed",
            //new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude
       // );
    //}

    // ���ӻ�������
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            transform.position + Vector3.up * 0.1f,
            transform.position + Vector3.down * (groundCheckDistance - 0.1f)
        );
    }
    public void PlayDialogue(string dialogueText)
    {
        dialogue.text = dialogueText;
    }
    public void ResetState()
    {
        // ʾ�������߼���
        StopAllCoroutines();      // ֹͣ����ִ�е��ƶ�/��Ծ
        GetComponent<Rigidbody>().velocity = Vector3.zero; // ��������״̬
        transform.position = initialPosition; // �ص���ʼλ��
        PlayAnimation("Idle");   // ����Ĭ�ϴ�������
      // �رնԻ���
    }

    private Vector3 initialPosition; // ��ʼλ��

    void Start()
    {
        initialPosition = transform.position; // ��¼��ʼλ��
    }
    public void ForceStop()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            animator.Play(idleAnim, 0, 0); // Ӳ�д�������
            moveCoroutine = null;
        }
    }
    void UpdateAnimations()
    {
        animator.SetFloat("MoveSpeed", Mathf.Abs(rb.velocity.x));
    }
    public bool MoveToByDistance(Vector3 target)
    {
        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            return true; // �ѵ���Ŀ���
        }

        // �����ƶ�
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );
        return false;
    }
}

