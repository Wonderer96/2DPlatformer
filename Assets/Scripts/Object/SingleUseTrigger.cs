using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class SingleUseTrigger : BaseTrigger
{
    [Header("Flip Settings")]
    [Tooltip("�Ƿ�������������")]
    public bool allowContinuousFlip = false;

    public float cooldownTime = 2f; // ���õ���ʱʱ��
    private bool isCooldown = false; // ������¼����ʱ״̬

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activatorTags.Contains(other.tag) && !isCooldown)
        {
            // ��ʼ����ʱ
            StartCoroutine(Cooldown());
            IsActivated = !IsActivated;
        }
    }

    private IEnumerator Cooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime); // �ȴ�ָ����ʱ��
        isCooldown = false;
    }
}