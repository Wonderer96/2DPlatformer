using UnityEngine;
using System.Collections.Generic;

public class PlayerReplay : MonoBehaviour
{
    [Header("关联对象")]
    public GameObject player;          // 玩家对象
    public GameObject clonePrefab;    // 克隆体预制体

    [Header("回放设置")]
    public KeyCode replayKey = KeyCode.V;  // 触发回放的按键
    public float recordSpan = 5f;          // 最大记录时长

    public bool isReplaying = false;      // 是否正在回放
    public float startTime;               // 记录开始时间
    public float replayStartTime;         // 回放开始时间
    public GameObject currentClone;       // 当前克隆体实例

    // 记录数据
    private List<Vector3> playerPositions = new List<Vector3>();
    private List<float> playerTimes = new List<float>();

    // 回放数据副本
    private List<Vector3> replayPositions = new List<Vector3>();
    private List<float> replayTimes = new List<float>();

    void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
        // 持续记录玩家位置
        RecordPlayerPosition();

        if (Input.GetKeyDown(replayKey))
        {
            StartReplay();
        }

        if (isReplaying)
        {
            ReplayCloneMovement();
        }
    }

    private void RecordPlayerPosition()
    {
        // 添加新记录
        playerPositions.Add(player.transform.position);
        float currentRecordTime = Time.time - startTime;
        playerTimes.Add(currentRecordTime);

        // 移除超出时间范围的旧记录
        while (playerTimes.Count > 0 &&
              currentRecordTime - playerTimes[0] > recordSpan)
        {
            playerTimes.RemoveAt(0);
            playerPositions.RemoveAt(0);
        }
    }

    private void StartReplay()
    {
        // 销毁现有克隆体（保持原逻辑）
        if (currentClone != null)
        {
            Destroy(currentClone);
        }

        // 创建数据副本（保持原逻辑）
        replayPositions = new List<Vector3>(playerPositions);
        replayTimes = new List<float>(playerTimes);

        // 新增：计算记录时间偏移量
        if (replayTimes.Count == 0)
        {
            Debug.LogWarning("没有可回放的数据");
            return;
        }

        // 关键修改：计算时间基准点
        float timeOffset = replayTimes[0];
        float totalDuration = replayTimes[replayTimes.Count - 1] - timeOffset;

        // 调整时间轴为相对时间（从0开始）
        for (int i = 0; i < replayTimes.Count; i++)
        {
            replayTimes[i] -= timeOffset;
        }

        // 创建克隆体并初始化时间
        currentClone = Instantiate(clonePrefab, replayPositions[0], Quaternion.identity);
        isReplaying = true;
        replayStartTime = Time.time;
    }

    private void ReplayCloneMovement()
    {
        // 关键修改：使用绝对时间比例计算
        float elapsedSinceReplayStart = Time.time - replayStartTime;
        float replayProgress = elapsedSinceReplayStart /
                              (replayTimes[replayTimes.Count - 1]);

        // 边界保护
        replayProgress = Mathf.Clamp01(replayProgress);
        float targetTime = replayProgress * replayTimes[replayTimes.Count - 1];

        // 查找时间区间（优化后的逻辑）
        for (int i = 0; i < replayTimes.Count - 1; i++)
        {
            if (targetTime >= replayTimes[i] &&
                targetTime <= replayTimes[i + 1])
            {
                float t = (targetTime - replayTimes[i]) /
                         (replayTimes[i + 1] - replayTimes[i]);

                currentClone.transform.position = Vector3.Lerp(
                    replayPositions[i],
                    replayPositions[i + 1],
                    t
                );
                break;
            }
        }

        // 完成条件判断
        if (elapsedSinceReplayStart >= replayTimes[replayTimes.Count - 1])
        {
            isReplaying = false;
            Destroy(currentClone);
            Debug.Log("回放完成");
        }
    }

    public void ResetRecording()
    {
        playerPositions.Clear();
        playerTimes.Clear();
        startTime = Time.time;
    }
}