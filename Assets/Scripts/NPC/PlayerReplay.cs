using UnityEngine;
using System.Collections.Generic;

public class PlayerReplay : MonoBehaviour
{
    [Header("关联对象")]
    public GameObject player; // 玩家对象
    public GameObject clone; // 克隆体对象

    [Header("回放设置")]
    public KeyCode replayKey = KeyCode.V; // 触发回放的按键

    private bool isRecording = true; // 是否正在记录
    private bool isReplaying = false; // 是否正在回放
    private float startTime; // 记录开始时间
    private List<Vector3> playerPositions = new List<Vector3>(); // 记录玩家位置
    private List<float> playerTimes = new List<float>(); // 记录对应时间

    void Update()
    {
        if (isRecording)
        {
            // 记录玩家位置和时间
            RecordPlayerPosition();
        }

        if (Input.GetKeyDown(replayKey))
        {
            // 开始回放
            StartReplay();
        }

        if (isReplaying)
        {
            // 回放克隆体移动
            ReplayCloneMovement();
        }
    }

    // 记录玩家位置和时间
    private void RecordPlayerPosition()
    {
        playerPositions.Add(player.transform.position);
        playerTimes.Add(Time.time - startTime);
    }

    // 开始回放
    private void StartReplay()
    {
        isRecording = false; // 停止记录
        isReplaying = true; // 开始回放
        startTime = Time.time; // 重置开始时间
    }

    // 回放克隆体移动
    private void ReplayCloneMovement()
    {
        float currentTime = Time.time - startTime;

        // 查找当前时间对应的位置
        for (int i = 0; i < playerTimes.Count - 1; i++)
        {
            if (currentTime >= playerTimes[i] && currentTime < playerTimes[i + 1])
            {
                // 使用插值计算克隆体的位置
                float t = (currentTime - playerTimes[i]) / (playerTimes[i + 1] - playerTimes[i]);
                clone.transform.position = Vector3.Lerp(playerPositions[i], playerPositions[i + 1], t);
                break;
            }
        }

        // 如果回放完成，停止回放
        if (currentTime >= playerTimes[playerTimes.Count - 1])
        {
            isReplaying = false;
            Debug.Log("回放完成");
        }
    }

    // 重置记录
    public void ResetRecording()
    {
        playerPositions.Clear();
        playerTimes.Clear();
        startTime = Time.time;
        isRecording = true;
        isReplaying = false;
    }
}
