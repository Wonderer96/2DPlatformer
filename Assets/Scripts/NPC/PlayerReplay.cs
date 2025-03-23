using UnityEngine;
using System.Collections.Generic;

public class PlayerReplay : MonoBehaviour
{
    [Header("��������")]
    public GameObject player; // ��Ҷ���
    public GameObject clone; // ��¡�����

    [Header("�ط�����")]
    public KeyCode replayKey = KeyCode.V; // �����طŵİ���

    private bool isRecording = true; // �Ƿ����ڼ�¼
    private bool isReplaying = false; // �Ƿ����ڻط�
    private float startTime; // ��¼��ʼʱ��
    private List<Vector3> playerPositions = new List<Vector3>(); // ��¼���λ��
    private List<float> playerTimes = new List<float>(); // ��¼��Ӧʱ��

    void Update()
    {
        if (isRecording)
        {
            // ��¼���λ�ú�ʱ��
            RecordPlayerPosition();
        }

        if (Input.GetKeyDown(replayKey))
        {
            // ��ʼ�ط�
            StartReplay();
        }

        if (isReplaying)
        {
            // �طſ�¡���ƶ�
            ReplayCloneMovement();
        }
    }

    // ��¼���λ�ú�ʱ��
    private void RecordPlayerPosition()
    {
        playerPositions.Add(player.transform.position);
        playerTimes.Add(Time.time - startTime);
    }

    // ��ʼ�ط�
    private void StartReplay()
    {
        isRecording = false; // ֹͣ��¼
        isReplaying = true; // ��ʼ�ط�
        startTime = Time.time; // ���ÿ�ʼʱ��
    }

    // �طſ�¡���ƶ�
    private void ReplayCloneMovement()
    {
        float currentTime = Time.time - startTime;

        // ���ҵ�ǰʱ���Ӧ��λ��
        for (int i = 0; i < playerTimes.Count - 1; i++)
        {
            if (currentTime >= playerTimes[i] && currentTime < playerTimes[i + 1])
            {
                // ʹ�ò�ֵ�����¡���λ��
                float t = (currentTime - playerTimes[i]) / (playerTimes[i + 1] - playerTimes[i]);
                clone.transform.position = Vector3.Lerp(playerPositions[i], playerPositions[i + 1], t);
                break;
            }
        }

        // ����ط���ɣ�ֹͣ�ط�
        if (currentTime >= playerTimes[playerTimes.Count - 1])
        {
            isReplaying = false;
            Debug.Log("�ط����");
        }
    }

    // ���ü�¼
    public void ResetRecording()
    {
        playerPositions.Clear();
        playerTimes.Clear();
        startTime = Time.time;
        isRecording = true;
        isReplaying = false;
    }
}
