using UnityEngine;
using System.Collections.Generic;

public class PlayerReplay : MonoBehaviour
{
    [Header("关联对象")]
    public GameObject player;          // 玩家对象
    public GameObject clonePrefab;    // 克隆体预制体

    [Header("回放设置")]
    public KeyCode replayKey = KeyCode.V;  // 触发回放的按键
    public float recordSpan = 5f;          // 记录时长

    // 状态标识
    private bool isRecording = false;      // 是否正在记录
    public bool isReplaying = false;      // 是否正在回放

    // 记录相关
    private Vector3 initialPosition;      // 记录起始位置
    private float startRecordingTime;     // 记录开始时间
    private List<Vector3> playerPositions = new List<Vector3>();
    private List<float> playerTimes = new List<float>();

    // 回放相关
    private float replayStartTime;        // 回放开始时间
    public GameObject currentClone;       // 当前克隆体实例
    private List<Vector3> replayPositions = new List<Vector3>();
    private List<float> replayTimes = new List<float>();

    void Update()
    {
        HandleRecording();
        HandleReplayInput();

        if (isReplaying)
        {
            ReplayCloneMovement();
        }
    }

    private void HandleReplayInput()
    {
        if (Input.GetKeyDown(replayKey))
        {
            if (isReplaying)
            {
                Destroy(currentClone);
                isReplaying = false;
            }
            StartNewRecording();
        }
    }

    private void HandleRecording()
    {
        if (!isRecording) return;

        // 持续记录位置
        RecordPlayerPosition();

        // 检查记录时长
        if (Time.time - startRecordingTime >= recordSpan)
        {
            StopRecordingAndStartReplay();
        }
    }

    private void StartNewRecording()
    {
        // 重置所有记录
        isRecording = true;
        playerPositions.Clear();
        playerTimes.Clear();
        initialPosition = player.transform.position;
        startRecordingTime = Time.time;
    }

    private void RecordPlayerPosition()
    {
        playerPositions.Add(player.transform.position);
        playerTimes.Add(Time.time - startRecordingTime);
    }

    private void StopRecordingAndStartReplay()
    {
        isRecording = false;
        player.transform.position = initialPosition;

        // 准备回放数据
        replayPositions = new List<Vector3>(playerPositions);
        replayTimes = new List<float>(playerTimes);

        StartReplay();
    }

    private void StartReplay()
    {
        if (replayPositions.Count == 0) return;

        currentClone = Instantiate(clonePrefab, initialPosition, Quaternion.identity);
        isReplaying = true;
        replayStartTime = Time.time;
    }

    private void ReplayCloneMovement()
    {
        float elapsed = Time.time - replayStartTime;
        float progress = Mathf.Clamp01(elapsed / recordSpan);

        // 简化版插值（实际项目建议使用更高效算法）
        for (int i = 0; i < replayTimes.Count - 1; i++)
        {
            if (progress >= replayTimes[i] / recordSpan &&
                progress <= replayTimes[i + 1] / recordSpan)
            {
                float t = (progress - replayTimes[i] / recordSpan) /
                         ((replayTimes[i + 1] - replayTimes[i]) / recordSpan);

                currentClone.transform.position = Vector3.Lerp(
                    replayPositions[i],
                    replayPositions[i + 1],
                    t
                );
                break;
            }
        }

        // 结束回放
        if (elapsed >= recordSpan)
        {
            isReplaying = false;
            Destroy(currentClone);
        }
    }
}