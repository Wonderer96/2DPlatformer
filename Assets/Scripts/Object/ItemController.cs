using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField] private float floatSpeed = 1f;          // Ư���ٶ�
    [SerializeField] private float floatDistance = 0.2f;    // Ư������

    private Vector3 startPosition;  // ��ʼλ�ã���Ϊ��͵㣩
    private float randomOffset;     // ���ƫ������ʹ��ͬ������˶���ͬ��

    void Start()
    {
        // ��¼��ʼλ��
        startPosition = transform.position;
        // ����һ�����ƫ������ʹ��ͬ�����Ư����ͬ��
        randomOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        // ʹ�����Һ�������Y��λ�ñ仯��ʵ�ַ�����Ư��
        // ��ʼλ����Ϊ��͵㣬���Լ���1ʹ�����0-2֮�䣬Ȼ����Ծ����һ��
        float newY = startPosition.y + (Mathf.Sin((Time.time + randomOffset) * floatSpeed + 1) * floatDistance * 0.5f);
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject);
            GameManager.Instance.coin++;
        }
    }
}



