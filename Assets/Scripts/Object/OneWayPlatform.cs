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
        // 检测玩家是否按下向下的键，并在短时间内禁用碰撞器
        //if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        //{
          //  StartCoroutine(DisableCollision());
        //}
    }

    private System.Collections.IEnumerator DisableCollision()
    {
        platformEffector.rotationalOffset = 180f; // 旋转碰撞器，让玩家可以向下掉落
        yield return new WaitForSeconds(0.2f);   // 暂时禁用碰撞器的持续时间
        platformEffector.rotationalOffset = 0f; // 恢复原始设置
    }
}

