using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShowDirector : MonoBehaviour
{
    [System.Serializable]
    public class NPCCommand
    {
        public float triggerTime;    // ����ʱ�䣨�룩
        public string actionType;    // ��������
        public NPCController targetNPC;    // Ŀ��NPC
        public Vector3 moveTarget;    // �ƶ�Ŀ��
        public float jumpForce;      // ��Ծ����
        public string animationName; // ��������
        public string dialogueText;  // �Ի��ı�
    }

    [Header("�ݳ�����")]
    public List<NPCCommand> commandSequence = new List<NPCCommand>();

    [Header("����")]
    [SerializeField] private float showTimer;
    [SerializeField] private bool isPlaying;

    private List<NPCCommand> sortedCommands; // ��ʱ������������
    private int currentCommandIndex;         // ��ǰִ�е�����������

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            StartShow();
        }

        if (!isPlaying) return;

        showTimer += Time.deltaTime;

        // �Ż���ֻ���鵱ǰ��֮�������
        for (int i = currentCommandIndex; i < sortedCommands.Count; i++)
        {
            NPCCommand cmd = sortedCommands[i];

            if (cmd.triggerTime > showTimer)
            {
                break; // ��������ʱ��δ������ǰ��ֹ
            }

            if (Mathf.Approximately(showTimer, cmd.triggerTime) ||
                showTimer > cmd.triggerTime)
            {
                ExecuteCommand(cmd);
                currentCommandIndex++;
            }
        }
    }

    private void ExecuteCommand(NPCCommand cmd)
    {
        // ��ӿ����ü��
        if (cmd.targetNPC == null)
        {
            Debug.LogWarning($"δָ��Ŀ��NPC: {cmd.actionType}");
            return;
        }

        switch (cmd.actionType)
        {
            case "Move":
                cmd.targetNPC.MoveTo(cmd.moveTarget);
                break;
            case "Jump":
                cmd.targetNPC.Jump(cmd.jumpForce);
                break;
            case "PlayAnim":
                if (!string.IsNullOrEmpty(cmd.animationName))
                    cmd.targetNPC.PlayAnimation(cmd.animationName);
                break;
            case "PlayDialogue":
                if (!string.IsNullOrEmpty(cmd.dialogueText))
                    cmd.targetNPC.PlayDialogue(cmd.dialogueText);
                break;
            default:
                Debug.LogWarning($"δָ֪������: {cmd.actionType}");
                break;
        }

        Debug.Log($"ִ������: {cmd.actionType} @ {showTimer:F2}s");
    }

    public void StartShow()
    {
        // ������ʱ������
        sortedCommands = commandSequence
            .OrderBy(c => c.triggerTime)
            .ToList();

        showTimer = 0f;
        currentCommandIndex = 0;
        isPlaying = true;

        Debug.Log($"�ݳ���ʼ���� {sortedCommands.Count} ������");
    }

    public void ResetShow()
    {
        isPlaying = false;
        showTimer = 0f;
        currentCommandIndex = 0;

        // ��ѡ����������NPC״̬
        foreach (var cmd in commandSequence)
        {
            if (cmd.targetNPC != null)
                cmd.targetNPC.ResetState();
        }
    }
}

