using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PressurePlateNum : BaseTrigger
{
    [Header("Pressure Plate Settings")]
    public float pressDistance = 0.2f;
    public float moveSpeed = 1f;
    [Tooltip("Number of objects required to activate the pressure plate")]
    public int requiredActivationCount = 1;  // 可以配置需要的物品数量

    private Vector3 originalPosition;
    private Vector3 pressedPosition;
    private int activationCount;

    protected override void Awake()
    {
        base.Awake();
        originalPosition = transform.position;
        pressedPosition = originalPosition - Vector3.up * pressDistance;
    }

    void Update()
    {
        UpdatePosition();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activatorTags.Contains(other.tag))
        {
            activationCount++;
            UpdateState();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (activatorTags.Contains(other.tag))
        {
            activationCount = Mathf.Max(0, activationCount - 1);
            UpdateState();
        }
    }

    private void UpdateState()
    {
        IsActivated = activationCount >= requiredActivationCount;  // 只有当激活数量达到要求时才激活
    }

    private void UpdatePosition()
    {
        Vector3 target = IsActivated ? pressedPosition : originalPosition;
        transform.position = Vector3.Lerp(transform.position, target, moveSpeed * Time.deltaTime);
    }
}
