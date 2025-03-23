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

    // ������������Ծ�����ֶΣ���ѡ��
    [Header("��Ծ����")]
    public float jumpForce = 8f;  // ���Ĭ��ֵ

    [System.NonSerialized]
    public bool executed;
}
