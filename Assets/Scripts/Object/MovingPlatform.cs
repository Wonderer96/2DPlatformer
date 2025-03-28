using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public enum MovementType
    {
        UpDown,
        LeftRight,
        PatrolLoop,
        PatrolPingPong
    }

    [Header("基础设置")]
    public MovementType movementType;
    public float moveDistance = 3f;    // 单边移动距离
    public float moveSpeed = 2f;

    [Header("路径模式设置")]
    public Transform[] waypoints;      // 路径点数组
    public float pathMoveSpeed = 1.5f;
    public float waypointThreshold = 0.1f; // 到达判定阈值

    private Vector3 startPos;
    private int currentWaypointIndex = 0;
    private bool isForward = true;     // 用于PingPong模式的移动方向

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        switch (movementType)
        {
            case MovementType.UpDown:
                VerticalMovement();
                break;
            case MovementType.LeftRight:
                HorizontalMovement();
                break;
            case MovementType.PatrolLoop:
                PatrolLoopMovement();
                break;
            case MovementType.PatrolPingPong:
                PatrolPingPongMovement();
                break;
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
    // 垂直上下移动
    void VerticalMovement()
    {
        float newY = startPos.y + Mathf.PingPong(Time.time * moveSpeed, moveDistance * 2) - moveDistance;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    // 水平左右移动
    void HorizontalMovement()
    {
        float newX = startPos.x + Mathf.PingPong(Time.time * moveSpeed, moveDistance * 2) - moveDistance;
        transform.position = new Vector3(newX, startPos.y, startPos.z);
    }

    // 循环巡逻模式
    void PatrolLoopMovement()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            pathMoveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < waypointThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    // 往返巡逻模式
    void PatrolPingPongMovement()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            pathMoveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < waypointThreshold)
        {
            if (isForward)
            {
                if (currentWaypointIndex >= waypoints.Length - 1)
                {
                    isForward = false;
                }
                else
                {
                    currentWaypointIndex++;
                }
            }
            else
            {
                if (currentWaypointIndex <= 0)
                {
                    isForward = true;
                }
                else
                {
                    currentWaypointIndex--;
                }
            }
        }
    }

    // 可视化路径（仅在编辑器显示）
    void OnDrawGizmos()
    {
        if (movementType == MovementType.PatrolLoop ||
            movementType == MovementType.PatrolPingPong)
        {
            if (waypoints != null && waypoints.Length > 1)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < waypoints.Length; i++)
                {
                    if (waypoints[i] == null) continue;
                    if (i < waypoints.Length - 1)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    }
                    Gizmos.DrawWireSphere(waypoints[i].position, 0.2f);
                }
            }
        }
        else
        {
            Gizmos.color = Color.yellow;
            Vector3 center = Application.isPlaying ? startPos : transform.position;
            Vector3 size = new Vector3(
                movementType == MovementType.LeftRight ? moveDistance * 2 : 0.5f,
                movementType == MovementType.UpDown ? moveDistance * 2 : 0.5f,
                0
            );
            Gizmos.DrawWireCube(center, size);
        }
    }
}
