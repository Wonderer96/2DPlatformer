using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class SingleUseTrigger : BaseTrigger
{
    [Header("Flip Settings")]
    [Tooltip("是否允许连续触发")]
    public bool allowContinuousFlip = false;

    public float cooldownTime = 2f; // 设置倒计时时间
    private bool isCooldown = false; // 用来记录倒计时状态

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activatorTags.Contains(other.tag) && !isCooldown)
        {
            // 开始倒计时
            StartCoroutine(Cooldown());
            IsActivated = !IsActivated;
        }
    }

    private IEnumerator Cooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime); // 等待指定的时间
        isCooldown = false;
    }
}