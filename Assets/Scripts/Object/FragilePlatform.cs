using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FragilePlatform : MonoBehaviour
{
    [Header("关联对象")]
    public GameObject linkedObject;  // 关联的可视化对象(比如有动画的物体)

    [Header("摇晃设置")]
    public float shakeDuration = 2.0f;  // 摇晃持续时间
    public float shakeIntensity = 0.1f; // 摇晃强度
    public float shakeSpeed = 10f;     // 摇晃速度

    [Header("崩溃设置")]
    public float collapseDelay = 1.0f;  // 摇晃后到开始崩溃的延迟

    [Header("重生设置")]
    public float destroyDelay = 2.0f;   // 崩溃后销毁的延迟
    public float respawnDelay = 5.0f;   // 销毁后重生的延迟

    private Vector3 originalPosition;   // 原始位置
    private Collider2D platformCollider; // 平台的碰撞体
    private Animator linkedAnimator;    // 关联对象的动画控制器
    private float shakeTimer = 0f;      // 摇晃计时器
    private bool isShaking = false;     // 是否正在摇晃
    private bool isCollapsing = false;  // 是否正在崩溃
    private bool isRespawning = false;  // 是否正在重生

    void Start()
    {
        // 初始化组件
        platformCollider = GetComponent<Collider2D>();
        originalPosition = transform.position;

        // 获取关联对象的Animator
        if (linkedObject != null)
        {
            linkedAnimator = linkedObject.GetComponent<Animator>();
            if (linkedAnimator == null)
            {
                Debug.LogWarning("关联对象没有Animator组件！");
            }
        }
        else
        {
            Debug.LogWarning("没有设置关联对象！");
        }
    }

    void Update()
    {
        // 处理摇晃效果
        if (isShaking && !isCollapsing)
        {
            shakeTimer += Time.deltaTime;

            // 计算摇晃偏移量
            float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
            float offsetY = Mathf.Cos(Time.time * shakeSpeed * 0.5f) * shakeIntensity * 0.5f;

            // 应用摇晃效果
            transform.position = originalPosition + new Vector3(offsetX, offsetY, 0);

            // 检查摇晃是否结束
            if (shakeTimer >= shakeDuration)
            {
                StopShaking();
                StartCollapse();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 检测玩家碰撞(可以根据需要修改标签)
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
        transform.position = originalPosition; // 重置位置
    }

    void StartCollapse()
    {
        isCollapsing = true;

        // 触发关联对象的崩溃动画
        if (linkedAnimator != null)
        {
            linkedAnimator.SetBool("isCollapsing", true);
        }

        // 禁用碰撞体
        if (platformCollider != null)
        {
            platformCollider.enabled = false;
        }

        // 延迟销毁
        Invoke("DestroyPlatform", destroyDelay);
    }

    void DestroyPlatform()
    {
        // 隐藏平台(而不是立即销毁，以便重生)
        if (linkedObject != null)
        {
            linkedObject.SetActive(false);
        }

        // 准备重生
        Invoke("RespawnPlatform", respawnDelay);
    }

    void RespawnPlatform()
    {
        isRespawning = true;

        // 重新激活关联对象
        if (linkedObject != null)
        {
            linkedObject.SetActive(true);

            // 重置动画状态
            if (linkedAnimator != null)
            {
                linkedAnimator.SetBool("isCollapsing", false);
            }
        }

        // 重新启用碰撞体
        if (platformCollider != null)
        {
            platformCollider.enabled = true;
        }

        // 重置所有状态
        isCollapsing = false;
        isRespawning = false;
    }

    // 在编辑器中可视化摇晃范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(shakeIntensity * 2, shakeIntensity, 0));
    }
}
