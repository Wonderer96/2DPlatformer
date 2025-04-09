using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SingleUseTriggerButton : BaseTrigger
{
    [Header("Flip Settings")]
    [Tooltip("�Ƿ�������������")]
    public bool allowContinuousFlip = false;

    public float cooldownTime = 2f; // ���õ���ʱʱ��
    private bool isCooldown = false; // ������¼����ʱ״̬
    private Animator animator;
    private bool isPlayerInside = false; // ��¼����Ƿ��ڴ�����������
    public GameObject activateButton;
    public List<string> TriggerTags = new List<string>() { "Player" };

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        animator.SetBool("isOn", IsActivated);

        // �������Ƿ��� J ���Ҵ��ڴ�����������
        if (isPlayerInside && Input.GetKeyDown(KeyCode.J) && !isCooldown)
        {
            // ��ʼ����ʱ
            StartCoroutine(Cooldown());
            IsActivated = !IsActivated;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activatorTags.Contains(other.tag))
        {
            isPlayerInside = true; // ��¼��ҽ��봥��������
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
            isPlayerInside = false; // ��¼����˳�����������
            activateButton.SetActive(false);
        }
    }

    private IEnumerator Cooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime); // �ȴ�ָ����ʱ��
        isCooldown = false;
    }
}
