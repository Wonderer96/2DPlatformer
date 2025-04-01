using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class ShowDirectorBySequence : MonoBehaviour
{
    [System.Serializable]
    public class NPCCommand
    {
        public string actionType;   // 动作类型
        public NPCController targetNPC;
        public Vector3 moveTarget;
        public float jumpForce;
        public string animationName;
        public string dialogueText;
        public float duration;
        public bool cantHappen;
    }

    [Header("演出配置")]
    public List<NPCCommand> commandSequence = new List<NPCCommand>();
    public bool isLoop; // 是否循环播放

    [Header("调试")]
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

        // 执行当前指令
        if (currentCommandIndex < sortedCommands.Count)
        {
            ExecuteCurrentCommand();
        }
        else if (isLoop) // 循环播放
        {
            currentCommandIndex = 0;
            ResetShow();
        }
        else // 播放结束
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
                // 移动到目标点后进入下一个动作
                if (cmd.targetNPC.MoveToByDistance(cmd.moveTarget))
                {
                    currentCommandIndex++;
                }
                break;
            case "Jump":
                // 瞬时动作，直接进入下一个动作
                cmd.targetNPC.Jump(cmd.jumpForce);
                currentCommandIndex++;
                break;
            case "PlayAnim":
                if (!string.IsNullOrEmpty(cmd.animationName))
                {
                    cmd.targetNPC.PlayAnimation(cmd.animationName);
                    if (countdownCoroutine != null)
                    {
                        break; // 如果已经有倒计时在运行，先停止它
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
                Debug.LogWarning($"未知指令类型: {cmd.actionType}");
                currentCommandIndex++; // 跳过未知指令
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
        // 停止所有NPC的当前动作
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
        // 重置所有NPC状态
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
            StopCoroutine(countdownCoroutine); // 如果已经有倒计时在运行，先停止它
        }
        countdownCoroutine = StartCoroutine(Countdown(duration)); // 开始3秒倒计时
    }

    // 倒计时协程
    private IEnumerator Countdown(float duration)
    {
        Debug.Log("倒计时开始: " + duration + "秒");

        float timeRemaining = duration;
        while (timeRemaining > 0)
        {
            yield return null; // 等待下一帧
            timeRemaining -= Time.deltaTime; // 减少剩余时间
            Debug.Log("剩余时间: " + timeRemaining.ToString("F2") + "秒");
        }

        Debug.Log("倒计时结束");
        OnCountdownFinished(); // 倒计时结束后触发
    }

    // 倒计时结束后的逻辑
    private void OnCountdownFinished()
    {
        currentCommandIndex++; // 增加命令索引
        Debug.Log("currentCommandIndex 已增加，当前值: " + currentCommandIndex);
    }
}

