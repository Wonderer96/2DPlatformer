using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    // 定义冷却时间（可通过编辑器修改）
    public float cooldownTime = 1.0f; // 默认冷却时间为1秒
    private bool isOnCooldown = false;
    public int damage = 1; // 冷却状态

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
            // 扣除玩家生命值
            collision.gameObject.GetComponent<MainCharacterController>().hP = collision.gameObject.GetComponent<MainCharacterController>().hP - damage;

            // 启动冷却
            StartCoroutine(StartCooldown());
        }
    }

    // 冷却逻辑
    private IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
    }
}

