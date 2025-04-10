using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;

    private void Update()
    {
        // 每帧按一定速度向左移动
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;
    }
}


