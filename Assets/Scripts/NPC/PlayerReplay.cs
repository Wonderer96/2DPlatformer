using UnityEngine;
using System.Collections.Generic;

public class PlayerReplay : MonoBehaviour
{
    [Header("��������")]
    public GameObject player;          // ��Ҷ���
    public GameObject clonePrefab;    // ��¡��Ԥ����

    [Header("�ط�����")]
    public KeyCode replayKey = KeyCode.V;  // �����طŵİ���
    public float recordSpan = 5f;          // ����¼ʱ��

    public bool isReplaying = false;      // �Ƿ����ڻط�
    public float startTime;               // ��¼��ʼʱ��
    public float replayStartTime;         // �طſ�ʼʱ��
    public GameObject currentClone;       // ��ǰ��¡��ʵ��

    // ��¼����
    private List<Vector3> playerPositions = new List<Vector3>();
    private List<float> playerTimes = new List<float>();

    // �ط����ݸ���
    private List<Vector3> replayPositions = new List<Vector3>();
    private List<float> replayTimes = new List<float>();

    void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
        // ������¼���λ��
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
        // ����¼�¼
        playerPositions.Add(player.transform.position);
        float currentRecordTime = Time.time - startTime;
        playerTimes.Add(currentRecordTime);

        // �Ƴ�����ʱ�䷶Χ�ľɼ�¼
        while (playerTimes.Count > 0 &&
              currentRecordTime - playerTimes[0] > recordSpan)
        {
            playerTimes.RemoveAt(0);
            playerPositions.RemoveAt(0);
        }
    }

    private void StartReplay()
    {
        // �������п�¡�壨����ԭ�߼���
        if (currentClone != null)
        {
            Destroy(currentClone);
        }

        // �������ݸ���������ԭ�߼���
        replayPositions = new List<Vector3>(playerPositions);
        replayTimes = new List<float>(playerTimes);

        // �����������¼ʱ��ƫ����
        if (replayTimes.Count == 0)
        {
            Debug.LogWarning("û�пɻطŵ�����");
            return;
        }

        // �ؼ��޸ģ�����ʱ���׼��
        float timeOffset = replayTimes[0];
        float totalDuration = replayTimes[replayTimes.Count - 1] - timeOffset;

        // ����ʱ����Ϊ���ʱ�䣨��0��ʼ��
        for (int i = 0; i < replayTimes.Count; i++)
        {
            replayTimes[i] -= timeOffset;
        }

        // ������¡�岢��ʼ��ʱ��
        currentClone = Instantiate(clonePrefab, replayPositions[0], Quaternion.identity);
        isReplaying = true;
        replayStartTime = Time.time;
    }

    private void ReplayCloneMovement()
    {
        // �ؼ��޸ģ�ʹ�þ���ʱ���������
        float elapsedSinceReplayStart = Time.time - replayStartTime;
        float replayProgress = elapsedSinceReplayStart /
                              (replayTimes[replayTimes.Count - 1]);

        // �߽籣��
        replayProgress = Mathf.Clamp01(replayProgress);
        float targetTime = replayProgress * replayTimes[replayTimes.Count - 1];

        // ����ʱ�����䣨�Ż�����߼���
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

        // ��������ж�
        if (elapsedSinceReplayStart >= replayTimes[replayTimes.Count - 1])
        {
            isReplaying = false;
            Destroy(currentClone);
            Debug.Log("�ط����");
        }
    }

    public void ResetRecording()
    {
        playerPositions.Clear();
        playerTimes.Clear();
        startTime = Time.time;
    }
}