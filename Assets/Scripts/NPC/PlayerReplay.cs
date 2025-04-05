using UnityEngine;
using System.Collections.Generic;

public class PlayerReplay : MonoBehaviour
{
    [Header("��������")]
    public GameObject player;          // ��Ҷ���
    public GameObject clonePrefab;    // ��¡��Ԥ����

    [Header("�ط�����")]
    public KeyCode replayKey = KeyCode.V;  // �����طŵİ���
    public float recordSpan = 5f;          // ��¼ʱ��

    // ״̬��ʶ
    private bool isRecording = false;      // �Ƿ����ڼ�¼
    public bool isReplaying = false;      // �Ƿ����ڻط�

    // ��¼���
    private Vector3 initialPosition;      // ��¼��ʼλ��
    private float startRecordingTime;     // ��¼��ʼʱ��
    private List<Vector3> playerPositions = new List<Vector3>();
    private List<float> playerTimes = new List<float>();

    // �ط����
    private float replayStartTime;        // �طſ�ʼʱ��
    public GameObject currentClone;       // ��ǰ��¡��ʵ��
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

        // ������¼λ��
        RecordPlayerPosition();

        // ����¼ʱ��
        if (Time.time - startRecordingTime >= recordSpan)
        {
            StopRecordingAndStartReplay();
        }
    }

    private void StartNewRecording()
    {
        // �������м�¼
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

        // ׼���ط�����
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

        // �򻯰��ֵ��ʵ����Ŀ����ʹ�ø���Ч�㷨��
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

        // �����ط�
        if (elapsed >= recordSpan)
        {
            isReplaying = false;
            Destroy(currentClone);
        }
    }
}