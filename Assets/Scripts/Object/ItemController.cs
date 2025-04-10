using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField] private float floatSpeed = 1f;          // 漂浮速度
    [SerializeField] private float floatDistance = 0.2f;    // 漂浮距离

    private Vector3 startPosition;  // 初始位置（作为最低点）
    private float randomOffset;     // 随机偏移量，使不同物体的运动不同步

    void Start()
    {
        // 记录初始位置
        startPosition = transform.position;
        // 生成一个随机偏移量，使不同物体的漂浮不同步
        randomOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        // 使用正弦函数计算Y轴位置变化，实现非匀速漂浮
        // 初始位置作为最低点，所以加上1使结果在0-2之间，然后乘以距离的一半
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



