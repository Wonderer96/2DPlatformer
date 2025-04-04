using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    // ������ȴʱ�䣨��ͨ���༭���޸ģ�
    public float cooldownTime = 1.0f; // Ĭ����ȴʱ��Ϊ1��
    private bool isOnCooldown = false; // ��ȴ״̬

    void Start()
    {

    }

    void Update()
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && !isOnCooldown)
        {
            // �۳��������ֵ
            collision.gameObject.GetComponent<MainCharacterController>().hP -= 1;

            // ������ȴ
            StartCoroutine(StartCooldown());
        }
    }

    // ��ȴ�߼�
    private IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
    }
}

