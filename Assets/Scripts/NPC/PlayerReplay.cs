using UnityEngine;
using System.Collections.Generic;

public class PlayerReplay : MonoBehaviour
{
    [Header("关联对象")]
    public GameObject player;          // 玩家对象
    public GameObject clonePrefab;     // 克隆体预制体

    [Header("回放设置")]
    public KeyCode replayKey = KeyCode.V;  // 触发回放的按键
    public float recordSpan = 5f;          // 回放记录的时长
    public float maxRecordTime = 20f;      // 持续记录时最长保存时间

    [Header("倒流设置")]
    public KeyCode reverseKey = KeyCode.B; // 触发倒流的按键
    public float reverseSpan = 3f;         // 倒流采样时间窗口，同时作为冷却时长

    // 持续记录相关（始终记录，不会中断）
    private List<Vector3> playerPositions = new List<Vector3>();
    private List<float> playerTimestamps = new List<float>();

    // 回放触发相关（略，见原实现）
    private bool replayTriggered = false;
    private float keyPressTime;
    private Vector3 keyPressPosition;

    // 回放相关（用于克隆体回放）
    private float cloneReplayStartTime;
    private List<Vector3> replayPositions = new List<Vector3>();
    private List<float> replayLocalTimes = new List<float>();
    public GameObject currentClone;
    private bool isReplaying = false;

    // 时光倒流相关
    private bool isReversing = false;          // 是否处于倒流模式
    private float reverseStartTime;            // 倒流启动时间
    private float reverseCooldown = 0f;        // 冷却截止时间

    // 新增两个列表用于存放倒流的快照数据
    private List<Vector3> reversePositions = new List<Vector3>();      // 倒流快照位置（顺序将经过反转）
    private List<float> reverseRelativeTimes = new List<float>();        // 每个位置对应的相对倒流时间（单位：秒）

    void Update()
    {
        // 持续记录玩家位置，并确保不超过maxRecordTime
        RecordPlayerPosition();

        // 处理回放逻辑（略，不再重复）
        HandleReplayInput();
        if (isReplaying)
        {
            ReplayCloneMovement();
        }

        // 处理时光倒流：检测倒流键输入
        HandleTimeReverseInput();
        if (isReversing)
        {
            TimeReverse();
        }
    }

    #region 玩家位置记录与回放（代码略，与之前版本保持一致）

    private void RecordPlayerPosition()
    {
        float currentTime = Time.time;
        playerPositions.Add(player.transform.position);
        playerTimestamps.Add(currentTime);

        while (playerTimestamps.Count > 0 && currentTime - playerTimestamps[0] > maxRecordTime)
        {
            playerTimestamps.RemoveAt(0);
            playerPositions.RemoveAt(0);
        }
    }

    private void HandleReplayInput()
    {
        if (Input.GetKeyDown(replayKey))
        {
            if (currentClone != null)
            {
                Destroy(currentClone);
                currentClone = null;
                isReplaying = false;
            }
            keyPressTime = Time.time;
            keyPressPosition = player.transform.position;
            replayTriggered = true;
        }

        if (replayTriggered && Time.time >= keyPressTime + recordSpan)
        {
            CreateReplaySnapshot();
            player.transform.position = keyPressPosition;
            StartCloneReplay();
            replayTriggered = false;
        }
    }

    private void CreateReplaySnapshot()
    {
        replayPositions.Clear();
        replayLocalTimes.Clear();

        for (int i = 0; i < playerTimestamps.Count; i++)
        {
            if (playerTimestamps[i] >= keyPressTime && playerTimestamps[i] <= keyPressTime + recordSpan)
            {
                replayPositions.Add(playerPositions[i]);
                replayLocalTimes.Add(playerTimestamps[i] - keyPressTime);
            }
        }

        if (replayPositions.Count == 0)
        {
            replayPositions.Add(keyPressPosition);
            replayLocalTimes.Add(0f);
        }
    }

    private void StartCloneReplay()
    {
        if (replayPositions.Count == 0) return;

        currentClone = Instantiate(clonePrefab, keyPressPosition, Quaternion.identity);
        cloneReplayStartTime = Time.time;
        isReplaying = true;
    }

    private void ReplayCloneMovement()
    {
        float elapsed = Time.time - cloneReplayStartTime;

        if (elapsed >= recordSpan)
        {
            currentClone.transform.position = replayPositions[replayPositions.Count - 1];
            Destroy(currentClone);
            currentClone = null;
            isReplaying = false;
            return;
        }

        for (int i = 0; i < replayLocalTimes.Count - 1; i++)
        {
            if (elapsed >= replayLocalTimes[i] && elapsed <= replayLocalTimes[i + 1])
            {
                float segmentDuration = replayLocalTimes[i + 1] - replayLocalTimes[i];
                float t = segmentDuration > 0 ? (elapsed - replayLocalTimes[i]) / segmentDuration : 0f;
                currentClone.transform.position = Vector3.Lerp(replayPositions[i], replayPositions[i + 1], t);
                break;
            }
        }
    }

    #endregion

    #region 时光倒流

    /// <summary>
    /// 检测倒流按键输入，并在满足冷却条件时捕获倒流快照（取最近 reverseSpan 秒的记录）
    /// </summary>
    private void HandleTimeReverseInput()
    {
        if (Input.GetKeyDown(reverseKey) && !isReversing && Time.time >= reverseCooldown)
        {
            float currentTime = Time.time;
            List<Vector3> snapshotPositions = new List<Vector3>();
            List<float> snapshotTimes = new List<float>();

            // 提取最近 reverseSpan 秒内的记录（注意 playerTimestamps 是升序的）
            for (int i = 0; i < playerTimestamps.Count; i++)
            {
                if (playerTimestamps[i] >= currentTime - reverseSpan)
                {
                    snapshotPositions.Add(playerPositions[i]);
                    snapshotTimes.Add(playerTimestamps[i]);
                }
            }
            if (snapshotPositions.Count == 0)
                return;

            // 以快照中的最新时间作为参考
            float maxTime = snapshotTimes[snapshotTimes.Count - 1];

            // 生成倒流快照：计算每个记录相对于最新记录的时间差
            reversePositions.Clear();
            reverseRelativeTimes.Clear();
            for (int i = 0; i < snapshotPositions.Count; i++)
            {
                float relTime = maxTime - snapshotTimes[i];  // 最新记录 relTime = 0
                reversePositions.Add(snapshotPositions[i]);
                reverseRelativeTimes.Add(relTime);
            }
            // 此时 reverseRelativeTimes 数组是降序的（例如：[2.9, 1.5, 0.2, 0]），将两个列表反转，使倒流时间从0开始递增
            reversePositions.Reverse();
            reverseRelativeTimes.Reverse();

            isReversing = true;
            reverseStartTime = Time.time;
        }
    }

    /// <summary>
    /// TimeReverse 方法：按照捕获的倒流快照，让玩家倒退移动，速度与原来记录时一致
    /// </summary>
    private void TimeReverse()
    {
        if (reversePositions.Count == 0 || reverseRelativeTimes.Count == 0)
        {
            isReversing = false;
            return;
        }

        // 倒流总时长：实际倒流数据覆盖的时间（例如可能为 2.5 秒）
        float totalDuration = reverseRelativeTimes[reverseRelativeTimes.Count - 1];
        float elapsed = Time.time - reverseStartTime;

        if (elapsed >= totalDuration)
        {
            // 达到或超过总倒流时长后，将玩家位置设置为快照中最早的记录
            player.transform.position = reversePositions[reversePositions.Count - 1];
            isReversing = false;
            reverseCooldown = Time.time + reverseSpan; // 冷却时长，可根据需要调整
            return;
        }

        // 根据 elapsed 在倒流快照中找到对应的时刻
        // 此处采用查找区间的方式进行线性插值
        int count = reverseRelativeTimes.Count;
        // 若快照只有一个点则直接设置位置
        if (count == 1)
        {
            player.transform.position = reversePositions[0];
            return;
        }
        for (int i = 0; i < count - 1; i++)
        {
            if (elapsed >= reverseRelativeTimes[i] && elapsed <= reverseRelativeTimes[i + 1])
            {
                float segDuration = reverseRelativeTimes[i + 1] - reverseRelativeTimes[i];
                float t = segDuration > 0 ? (elapsed - reverseRelativeTimes[i]) / segDuration : 0f;
                player.transform.position = Vector3.Lerp(reversePositions[i], reversePositions[i + 1], t);
                break;
            }
        }
    }

    #endregion
}


