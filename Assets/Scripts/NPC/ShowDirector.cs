using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShowDirector : MonoBehaviour
{
    [System.Serializable]
    public class NPCCommand
    {
        public float triggerTime;    // 触发时间（秒）
        public string actionType;    // 动作类型
        public NPCController targetNPC;    // 目标NPC
        public Vector3 moveTarget;    // 移动目标
        public float jumpForce;      // 跳跃力度
        public string animationName; // 动画名称
        public string dialogueText;  // 对话文本
    }

    [Header("演出配置")]
    public List<NPCCommand> commandSequence = new List<NPCCommand>();

    [Header("调试")]
    [SerializeField] private float showTimer;
    [SerializeField] private bool isPlaying;

    private List<NPCCommand> sortedCommands; // 按时间排序后的命令
    private int currentCommandIndex;         // 当前执行到的命令索引

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            StartShow();
        }

        if (!isPlaying) return;

        showTimer += Time.deltaTime;

        // 优化：只需检查当前及之后的命令
        for (int i = currentCommandIndex; i < sortedCommands.Count; i++)
        {
            NPCCommand cmd = sortedCommands[i];

            if (cmd.triggerTime > showTimer)
            {
                break; // 后续命令时间未到，提前终止
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
        // 添加空引用检查
        if (cmd.targetNPC == null)
        {
            Debug.LogWarning($"未指定目标NPC: {cmd.actionType}");
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
                Debug.LogWarning($"未知指令类型: {cmd.actionType}");
                break;
        }

        Debug.Log($"执行命令: {cmd.actionType} @ {showTimer:F2}s");
    }

    public void StartShow()
    {
        // 按触发时间排序
        sortedCommands = commandSequence
            .OrderBy(c => c.triggerTime)
            .ToList();

        showTimer = 0f;
        currentCommandIndex = 0;
        isPlaying = true;

        Debug.Log($"演出开始，共 {sortedCommands.Count} 条命令");
    }

    public void ResetShow()
    {
        isPlaying = false;
        showTimer = 0f;
        currentCommandIndex = 0;

        // 可选：重置所有NPC状态
        foreach (var cmd in commandSequence)
        {
            if (cmd.targetNPC != null)
                cmd.targetNPC.ResetState();
        }
    }
}

