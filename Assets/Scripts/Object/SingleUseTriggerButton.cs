using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SingleUseTriggerButton : BaseTrigger
{
    [Header("Flip Settings")]
    [Tooltip("是否允许连续触发")]
    public bool allowContinuousFlip = false;

    public float cooldownTime = 2f; // 设置倒计时时间
    private bool isCooldown = false; // 用来记录倒计时状态
    private Animator animator;
    private bool isPlayerInside = false; // 记录玩家是否在触发器区域内
    public GameObject activateButton;
    public List<string> TriggerTags = new List<string>() { "Player" };

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        animator.SetBool("isOn", IsActivated);

        // 检测玩家是否按下 J 键且处于触发器区域内
        if (isPlayerInside && Input.GetKeyDown(KeyCode.J) && !isCooldown)
        {
            // 开始倒计时
            StartCoroutine(Cooldown());
            IsActivated = !IsActivated;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activatorTags.Contains(other.tag))
        {
            isPlayerInside = true; // 记录玩家进入触发器区域
            activateButton.SetActive(true);
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("Hook"))
        {
            IsActivated = !IsActivated;
        }

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (activatorTags.Contains(other.tag))
        {
            isPlayerInside = false; // 记录玩家退出触发器区域
            activateButton.SetActive(false);
        }
    }

    private IEnumerator Cooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime); // 等待指定的时间
        isCooldown = false;
    }
}
