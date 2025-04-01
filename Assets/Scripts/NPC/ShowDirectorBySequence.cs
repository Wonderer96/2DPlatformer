using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class ShowDirectorBySequence : MonoBehaviour
{
    [System.Serializable]
    public class NPCCommand
    {
        public string actionType;   // ��������
        public NPCController targetNPC;
        public Vector3 moveTarget;
        public float jumpForce;
        public string animationName;
        public string dialogueText;
        public float duration;
        public bool cantHappen;
    }

    [Header("�ݳ�����")]
    public List<NPCCommand> commandSequence = new List<NPCCommand>();
    public bool isLoop; // �Ƿ�ѭ������

    [Header("����")]
    [SerializeField] private bool isPlaying;
    [SerializeField] private int currentCommandIndex;

    public List<NPCCommand> sortedCommands;
    private bool isPaused;
    public NPCController thisNPC;
    private Coroutine countdownCoroutine;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) StartShow();
        if (!isPlaying || isPaused) return;

        // ִ�е�ǰָ��
        if (currentCommandIndex < sortedCommands.Count)
        {
            ExecuteCurrentCommand();
        }
        else if (isLoop) // ѭ������
        {
            currentCommandIndex = 0;
            ResetShow();
        }
        else // ���Ž���
        {
            StopShow();
        }
    }

    private void ExecuteCurrentCommand()
    {
        NPCCommand cmd = sortedCommands[currentCommandIndex];
        cmd.targetNPC = thisNPC;
        if (cmd.targetNPC == null) return;
        if (cmd.cantHappen) return;
        switch (cmd.actionType)
        {
            case "Move":
                // �ƶ���Ŀ���������һ������
                if (cmd.targetNPC.MoveToByDistance(cmd.moveTarget))
                {
                    currentCommandIndex++;
                }
                break;
            case "Jump":
                // ˲ʱ������ֱ�ӽ�����һ������
                cmd.targetNPC.Jump(cmd.jumpForce);
                currentCommandIndex++;
                break;
            case "PlayAnim":
                if (!string.IsNullOrEmpty(cmd.animationName))
                {
                    cmd.targetNPC.PlayAnimation(cmd.animationName);
                    if (countdownCoroutine != null)
                    {
                        break; // ����Ѿ��е���ʱ�����У���ֹͣ��
                    }
                    StartCountdown(cmd.duration);
                }
                break;
            case "PlayDialogue":
                if (!string.IsNullOrEmpty(cmd.dialogueText))
                {
                    cmd.targetNPC.PlayDialogue(cmd.dialogueText);
                    currentCommandIndex++;
                }
                break;
            default:
                Debug.LogWarning($"δָ֪������: {cmd.actionType}");
                currentCommandIndex++; // ����δָ֪��
                break;
        }
    }

    public void StartShow()
    {
        sortedCommands = commandSequence.ToList();
        currentCommandIndex = 0;
        isPlaying = true;
        isPaused = false;
    }

    public void PauseShow()
    {
        isPaused = true;
        // ֹͣ����NPC�ĵ�ǰ����
        foreach (var cmd in sortedCommands)
        {
            if (cmd.targetNPC != null)
                cmd.targetNPC.ForceStop();
        }
    }

    public void ResumeShow()
    {
        isPaused = false;
    }

    public void StopShow()
    {
        isPlaying = false;
        currentCommandIndex = 0;
    }

    public void ResetShow()
    {
        // ��������NPC״̬
        foreach (var cmd in sortedCommands)
        {
            if (cmd.targetNPC != null)
                cmd.targetNPC.ResetState();
        }
    }
    public void StartCountdown( float duration)
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine); // ����Ѿ��е���ʱ�����У���ֹͣ��
        }
        countdownCoroutine = StartCoroutine(Countdown(duration)); // ��ʼ3�뵹��ʱ
    }

    // ����ʱЭ��
    private IEnumerator Countdown(float duration)
    {
        Debug.Log("����ʱ��ʼ: " + duration + "��");

        float timeRemaining = duration;
        while (timeRemaining > 0)
        {
            yield return null; // �ȴ���һ֡
            timeRemaining -= Time.deltaTime; // ����ʣ��ʱ��
            Debug.Log("ʣ��ʱ��: " + timeRemaining.ToString("F2") + "��");
        }

        Debug.Log("����ʱ����");
        OnCountdownFinished(); // ����ʱ�����󴥷�
    }

    // ����ʱ��������߼�
    private void OnCountdownFinished()
    {
        currentCommandIndex++; // ������������
        Debug.Log("currentCommandIndex �����ӣ���ǰֵ: " + currentCommandIndex);
    }
}

