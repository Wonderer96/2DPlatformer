// NPCCommandData.cs
using UnityEngine;

[System.Serializable]
public class NPCCommand
{
    public float triggerTime;
    public float duration;
    public NPCController targetNPC;
    public string actionType;
    public Vector3 moveTarget;
    public string animationName;
    public string dialogueText;
    public bool cantHappen;

    // 新增可配置跳跃力度字段（可选）
    [Header("跳跃参数")]
    public float jumpForce = 8f;  // 添加默认值

    [System.NonSerialized]
    public bool executed;
}
