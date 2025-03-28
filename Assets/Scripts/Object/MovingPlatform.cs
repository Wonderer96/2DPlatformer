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

    [Header("��������")]
    public MovementType movementType;
    public float moveDistance = 3f;    // �����ƶ�����
    public float moveSpeed = 2f;

    [Header("·��ģʽ����")]
    public Transform[] waypoints;      // ·��������
    public float pathMoveSpeed = 1.5f;
    public float waypointThreshold = 0.1f; // �����ж���ֵ

    private Vector3 startPos;
    private int currentWaypointIndex = 0;
    private bool isForward = true;     // ����PingPongģʽ���ƶ�����

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
    // ��ֱ�����ƶ�
    void VerticalMovement()
    {
        float newY = startPos.y + Mathf.PingPong(Time.time * moveSpeed, moveDistance * 2) - moveDistance;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    // ˮƽ�����ƶ�
    void HorizontalMovement()
    {
        float newX = startPos.x + Mathf.PingPong(Time.time * moveSpeed, moveDistance * 2) - moveDistance;
        transform.position = new Vector3(newX, startPos.y, startPos.z);
    }

    // ѭ��Ѳ��ģʽ
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

    // ����Ѳ��ģʽ
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

    // ���ӻ�·�������ڱ༭����ʾ��
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
