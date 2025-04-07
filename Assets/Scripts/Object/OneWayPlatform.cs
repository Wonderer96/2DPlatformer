using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    private PlatformEffector2D platformEffector;

    void Start()
    {
        platformEffector = GetComponent<PlatformEffector2D>();
    }

    void Update()
    {
        // �������Ƿ������µļ������ڶ�ʱ���ڽ�����ײ��
        //if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        //{
          //  StartCoroutine(DisableCollision());
        //}
    }

    private System.Collections.IEnumerator DisableCollision()
    {
        platformEffector.rotationalOffset = 180f; // ��ת��ײ��������ҿ������µ���
        yield return new WaitForSeconds(0.2f);   // ��ʱ������ײ���ĳ���ʱ��
        platformEffector.rotationalOffset = 0f; // �ָ�ԭʼ����
    }
}

