// NPCController3D.cs
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class NPCController : MonoBehaviour
{
    [Header("动画控制")]
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

        // 2D视角的面向控制
        if (direction.x != 0)
        {
            // 根据方向翻转角色的朝向（2D视角）
            float scaleX = Mathf.Sign(direction.x); // 获取方向符号（1或-1）
            transform.localScale = new Vector3(0.2f * scaleX, 0.2f, 0.2f); // 翻转X轴
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

    // 可视化地面检测
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
        // 示例重置逻辑：
        StopAllCoroutines();      // 停止正在执行的移动/跳跃
        GetComponent<Rigidbody>().velocity = Vector3.zero; // 重置物理状态
        transform.position = initialPosition; // 回到初始位置
        PlayAnimation("Idle");   // 播放默认待机动画
      // 关闭对话框
    }

    private Vector3 initialPosition; // 初始位置

    void Start()
    {
        initialPosition = transform.position; // 记录初始位置
    }
    public void ForceStop()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            animator.Play(idleAnim, 0, 0); // 硬切待机动画
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
            return true; // 已到达目标点
        }

        // 持续移动
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );
        return false;
    }
}

