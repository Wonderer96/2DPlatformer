using UnityEngine;
using System.Collections.Generic;

public class PlayerReplay : MonoBehaviour
{
    [Header("��������")]
    public GameObject player;          // ��Ҷ���
    public GameObject clonePrefab;     // ��¡��Ԥ����

    [Header("�ط�����")]
    public KeyCode replayKey = KeyCode.V;  // �����طŵİ���
    public float recordSpan = 5f;          // �طż�¼��ʱ��
    public float maxRecordTime = 20f;      // ������¼ʱ�����ʱ��

    [Header("��������")]
    public KeyCode reverseKey = KeyCode.B; // ���������İ���
    public float reverseSpan = 3f;         // ��������ʱ�䴰�ڣ�ͬʱ��Ϊ��ȴʱ��

    // ������¼��أ�ʼ�ռ�¼�������жϣ�
    private List<Vector3> playerPositions = new List<Vector3>();
    private List<float> playerTimestamps = new List<float>();

    // �طŴ�����أ��ԣ���ԭʵ�֣�
    private bool replayTriggered = false;
    private float keyPressTime;
    private Vector3 keyPressPosition;

    // �ط���أ����ڿ�¡��طţ�
    private float cloneReplayStartTime;
    private List<Vector3> replayPositions = new List<Vector3>();
    private List<float> replayLocalTimes = new List<float>();
    public GameObject currentClone;
    private bool isReplaying = false;

    // ʱ�⵹�����
    private bool isReversing = false;          // �Ƿ��ڵ���ģʽ
    private float reverseStartTime;            // ��������ʱ��
    private float reverseCooldown = 0f;        // ��ȴ��ֹʱ��

    // ���������б����ڴ�ŵ����Ŀ�������
    private List<Vector3> reversePositions = new List<Vector3>();      // ��������λ�ã�˳�򽫾�����ת��
    private List<float> reverseRelativeTimes = new List<float>();        // ÿ��λ�ö�Ӧ����Ե���ʱ�䣨��λ���룩

    void Update()
    {
        // ������¼���λ�ã���ȷ��������maxRecordTime
        RecordPlayerPosition();

        // ����ط��߼����ԣ������ظ���
        HandleReplayInput();
        if (isReplaying)
        {
            ReplayCloneMovement();
        }

        // ����ʱ�⵹������⵹��������
        HandleTimeReverseInput();
        if (isReversing)
        {
            TimeReverse();
        }
    }

    #region ���λ�ü�¼��طţ������ԣ���֮ǰ�汾����һ�£�

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

    #region ʱ�⵹��

    /// <summary>
    /// ��⵹���������룬����������ȴ����ʱ���������գ�ȡ��� reverseSpan ��ļ�¼��
    /// </summary>
    private void HandleTimeReverseInput()
    {
        if (Input.GetKeyDown(reverseKey) && !isReversing && Time.time >= reverseCooldown)
        {
            float currentTime = Time.time;
            List<Vector3> snapshotPositions = new List<Vector3>();
            List<float> snapshotTimes = new List<float>();

            // ��ȡ��� reverseSpan ���ڵļ�¼��ע�� playerTimestamps ������ģ�
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

            // �Կ����е�����ʱ����Ϊ�ο�
            float maxTime = snapshotTimes[snapshotTimes.Count - 1];

            // ���ɵ������գ�����ÿ����¼��������¼�¼��ʱ���
            reversePositions.Clear();
            reverseRelativeTimes.Clear();
            for (int i = 0; i < snapshotPositions.Count; i++)
            {
                float relTime = maxTime - snapshotTimes[i];  // ���¼�¼ relTime = 0
                reversePositions.Add(snapshotPositions[i]);
                reverseRelativeTimes.Add(relTime);
            }
            // ��ʱ reverseRelativeTimes �����ǽ���ģ����磺[2.9, 1.5, 0.2, 0]�����������б�ת��ʹ����ʱ���0��ʼ����
            reversePositions.Reverse();
            reverseRelativeTimes.Reverse();

            isReversing = true;
            reverseStartTime = Time.time;
        }
    }

    /// <summary>
    /// TimeReverse ���������ղ���ĵ������գ�����ҵ����ƶ����ٶ���ԭ����¼ʱһ��
    /// </summary>
    private void TimeReverse()
    {
        if (reversePositions.Count == 0 || reverseRelativeTimes.Count == 0)
        {
            isReversing = false;
            return;
        }

        // ������ʱ����ʵ�ʵ������ݸ��ǵ�ʱ�䣨�������Ϊ 2.5 �룩
        float totalDuration = reverseRelativeTimes[reverseRelativeTimes.Count - 1];
        float elapsed = Time.time - reverseStartTime;

        if (elapsed >= totalDuration)
        {
            // �ﵽ�򳬹��ܵ���ʱ���󣬽����λ������Ϊ����������ļ�¼
            player.transform.position = reversePositions[reversePositions.Count - 1];
            isReversing = false;
            reverseCooldown = Time.time + reverseSpan; // ��ȴʱ�����ɸ�����Ҫ����
            return;
        }

        // ���� elapsed �ڵ����������ҵ���Ӧ��ʱ��
        // �˴����ò�������ķ�ʽ�������Բ�ֵ
        int count = reverseRelativeTimes.Count;
        // ������ֻ��һ������ֱ������λ��
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


